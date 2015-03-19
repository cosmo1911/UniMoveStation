using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Nito.Async;
using Nito.Async.Sockets;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Event;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.NitoMessages;

namespace UniMoveStation.Business.Nito
{
    public class NitoServer
    {
        public ServerModel Server { get; set; }

        public IConsoleService ConsoleService { get; set; }

        public EventHandler OnClientAddedHandler;

        public EventHandler OnClientRemovedHandler;

        public void Initialize(IConsoleService consoleService, ServerModel server)
        {
            ConsoleService = consoleService;
            Server = server;
        }

        /// <summary>
        /// The socket that listens for connections. This is null if we are not listening.
        /// </summary>
        protected SimpleServerTcpSocket ListeningSocket;


        /// <summary>
        /// Closes and clears the listening socket and all connected sockets, without causing exceptions.
        /// </summary>
        private void ResetListeningSocket()
        {
            // Close all child sockets
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> socket in Server.ChildSockets)
                socket.Key.ShutdownAsync();

            Server.ChildSockets.Clear();

            // Close the listening socket
            ListeningSocket.Close();
            ListeningSocket = null;
        }

        /// <summary>
        /// Closes and clears a child socket (established connection), without causing exceptions.
        /// </summary>
        /// <param name="childSocket">The child socket to close. May be null.</param>
        private void ResetChildSocket(SimpleServerChildTcpSocket childSocket)
        {
            // Close the child socket if possible
            if (childSocket != null)
                childSocket.Close();

            // Remove it from the list of child sockets
            Server.ChildSockets.Remove(childSocket);
        }

        public virtual void RefreshDisplay()
        {
            
        }

        private void ListeningSocket_ConnectionArrived(AsyncResultEventArgs<SimpleServerChildTcpSocket> e)
        {
            // Check for errors
            if (e.Error != null)
            {
                ResetListeningSocket();
                ConsoleService.WriteLine("Socket error during Accept: [" + e.Error.GetType().Name + "] " + e.Error.Message);
                RefreshDisplay();
                return;
            }

            SimpleServerChildTcpSocket socket = e.Result;

            try
            {
                // Save the new child socket connection
                Server.ChildSockets.Add(socket, ChildSocketState.Connected);
                Server.Clients.Add(socket, new ClientModel
                {
                    RemoteEndPoint = socket.RemoteEndPoint.ToString()
                });
                if (OnClientAddedHandler != null) OnClientAddedHandler(this, new OnClientAddedEventArgs(Server.Clients[socket]));

                socket.PacketArrived += args => ChildSocket_PacketArrived(socket, args);
                socket.WriteCompleted += args => ChildSocket_WriteCompleted(socket, args);
                socket.ShutdownCompleted += args => ChildSocket_ShutdownCompleted(socket, args);

                // Display the connection information
                ConsoleService.WriteLine("Connection established to " + socket.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                ResetChildSocket(socket);
                ConsoleService.WriteLine("Socket error accepting connection: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void ChildSocket_PacketArrived(SimpleServerChildTcpSocket socket, AsyncResultEventArgs<byte[]> e)
        {
            try
            {
                // Check for errors
                if (e.Error != null)
                {
                    ConsoleService.WriteLine("Client socket error during Read from " + socket.RemoteEndPoint + ": [" + e.Error.GetType().Name + "] " + e.Error.Message);
                    ResetChildSocket(socket);
                }
                else if (e.Result == null)
                {
                    // PacketArrived completes with a null packet when the other side gracefully closes the connection
                    ConsoleService.WriteLine("Socket graceful close detected from " + socket.RemoteEndPoint);

                    // Close the socket and remove it from the list
                    ResetChildSocket(socket);
                }
                else
                {
                    // At this point, we know we actually got a message.

                    // Deserialize the message
                    object message = SerializationHelper.Deserialize(e.Result);

                    // Handle the message
                    if(HandleMessage(message, socket) == false)
                    {
                        ConsoleService.WriteLine("Socket read got an unknown message from " + socket.RemoteEndPoint + " of type " + message.GetType().Name);
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleService.WriteLine("Error reading from socket " + socket.RemoteEndPoint + ": [" + ex.GetType().Name + "] " + ex.Message);
                ResetChildSocket(socket);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        protected virtual bool HandleMessage(object message, SimpleServerChildTcpSocket socket)
        {
            StringMessage stringMessage = message as StringMessage;
            if (stringMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a string message from " + socket.RemoteEndPoint + ": " + stringMessage.Message);
                return true;
            }

            ComplexMessage complexMessage = message as ComplexMessage;
            if (complexMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a complex message from " + socket.RemoteEndPoint + ": (UniqueID = " + complexMessage.UniqueID +
                    ", Time = " + complexMessage.Time + ", Message = " + complexMessage.Message + ")");
                return true;
            }

            return false;
        }

        private void ChildSocket_ShutdownCompleted(object sender, AsyncCompletedEventArgs e)
        {
            SimpleServerChildTcpSocket socket = (SimpleServerChildTcpSocket)sender;

            // Check for errors
            if (e.Error != null)
            {
                ConsoleService.WriteLine("Socket error during Shutdown of " + socket.RemoteEndPoint + ": [" + e.Error.GetType().Name + "] " + e.Error.Message);
                ResetChildSocket(socket);
            }
            else
            {
                ConsoleService.WriteLine("Socket shutdown completed on " + socket.RemoteEndPoint);

                // Close the socket and remove it from the list
                ResetChildSocket(socket);
            }
            if(OnClientRemovedHandler != null) OnClientRemovedHandler(this, new OnClientRemovedEventArgs(Server.Clients[socket]));
            RefreshDisplay();
        }

        private void ChildSocket_WriteCompleted(SimpleServerChildTcpSocket socket, AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                // Note: WriteCompleted may be called as the result of a normal write (SocketPacketizer.WritePacketAsync),
                //  or as the result of a call to SocketPacketizer.WriteKeepaliveAsync. However, WriteKeepaliveAsync
                //  will never invoke WriteCompleted if the write was successful; it will only invoke WriteCompleted if
                //  the keepalive packet failed (indicating a loss of connection).

                // If you want to get fancy, you can tell if the error is the result of a write failure or a keepalive
                //  failure by testing e.UserState, which is set by normal writes.
                if (e.UserState is string)
                    ConsoleService.WriteLine("Socket error during Write to " + socket.RemoteEndPoint + ": [" + e.Error.GetType().Name + "] " + e.Error.Message);
                else
                    ConsoleService.WriteLine("Socket error detected by keepalive to " + socket.RemoteEndPoint + ": [" + e.Error.GetType().Name + "] " + e.Error.Message);

                ResetChildSocket(socket);
            }
            else
            {
                string description = (string)e.UserState;
                ConsoleService.WriteLine("Socket write completed to " + socket.RemoteEndPoint + " for message " + description);
            }

            RefreshDisplay();
        }

        public void Start(int port)
        {
            try
            {
                // Define the socket, bind to the port, and start accepting connections
                ListeningSocket = new SimpleServerTcpSocket();
                ListeningSocket.ConnectionArrived += ListeningSocket_ConnectionArrived;
                ListeningSocket.Listen(port);

                ConsoleService.WriteLine("Listening on port " + port);
            }
            catch (Exception ex)
            {
                ResetListeningSocket();
                ConsoleService.WriteLine("Error creating listening socket on port " + port + ": [" + ex.GetType().Name + "] " + ex.Message);
            }

            RefreshDisplay();
        }

        public void Stop()
        {
            ConsoleService.WriteLine("Stopped listening.");
            // Close the listening socket cleanly
            ResetListeningSocket();
            RefreshDisplay();
        }

        public void SendMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, string message)
        {
            // This function sends a simple (string) message to all connected clients
            StringMessage msg = new StringMessage();
            msg.Message = message;

            string description = "<string message: " + msg.Message + ">";

            // Serialize it to a binary array
            byte[] binaryObject = SerializationHelper.Serialize(msg);

            // Start a send on child socket

            // Ignore sockets that are disconnecting
            if (childSocket.Value == ChildSocketState.Connected)
            {
                try
                {
                    ConsoleService.WriteLine("Sending to " + childSocket.Key.RemoteEndPoint + ": " + description);
                    childSocket.Key.WriteAsync(binaryObject, description);
                }
                catch (Exception ex)
                {
                    // Handle error
                    ConsoleService.WriteLine("Child Socket error sending message to " + childSocket.Key.RemoteEndPoint + ": [" + ex.GetType().Name + "] " + ex.Message);
                    ResetChildSocket(childSocket.Key);
                }
            }

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        public void SendMessageAll(string message)
        {
            // Start a send on each child socket
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket in Server.ChildSockets)
            {
                SendMessage(childSocket, message);
            }
        }

        public void SendComplexMessageAll(string message)
        {
            // Start a send on each child socket
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket in Server.ChildSockets)
            {
                SendComplexMessage(childSocket, message);
            }
        }

        protected void SendSerializedMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, byte[] binaryObject, string description)
        {
            // Ignore sockets that are disconnecting
            if (childSocket.Value == ChildSocketState.Connected)
            {
                try
                {
                    ConsoleService.WriteLine("Sending to " + childSocket.Key.RemoteEndPoint + ": " + description);
                    childSocket.Key.WriteAsync(binaryObject, description);
                }
                catch (Exception ex)
                {
                    // Handle error
                    ConsoleService.WriteLine("Child Socket error sending message to " + childSocket.Key.RemoteEndPoint + ": [" + ex.GetType().Name + "] " + ex.Message);
                    ResetChildSocket(childSocket.Key);
                }
            }

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        public void SendComplexMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, string message)
        {
            // This function sends a complex message to all connected clients
            ComplexMessage msg = new ComplexMessage();
            msg.UniqueID = Guid.NewGuid();
            msg.Time = DateTimeOffset.Now;
            msg.Message = message;

            string description = "<complex message: " + msg.UniqueID + ">";

            // Serialize it to a binary array
            byte[] binaryObject = SerializationHelper.Serialize(msg);

            SendSerializedMessage(childSocket, binaryObject, description);
        }

        public void Disconnect(ClientModel client)
        {
            SimpleServerChildTcpSocket[] children = new SimpleServerChildTcpSocket[Server.ChildSockets.Keys.Count];
            Server.ChildSockets.Keys.CopyTo(children, 0);
            foreach (SimpleServerChildTcpSocket child in children)
            {
                if (child.RemoteEndPoint.ToString().Equals(client.RemoteEndPoint))
                {
                    try
                    {
                        child.ShutdownAsync();
                        Server.ChildSockets[child] = ChildSocketState.Disconnecting;
                    }
                    catch (Exception ex)
                    {
                        ConsoleService.WriteLine("Child Socket error disconnecting from " + child.RemoteEndPoint + ": [" + ex.GetType().Name + "] " + ex.Message);
                        ResetChildSocket(child);
                    }
                }
            }
        }

        public void Disconnect()
        {
            // Initiate a graceful disconnect for all clients
            SimpleServerChildTcpSocket[] children = new SimpleServerChildTcpSocket[Server.ChildSockets.Keys.Count];
            Server.ChildSockets.Keys.CopyTo(children, 0);
            foreach (SimpleServerChildTcpSocket child in children)
            {
                try
                {
                    child.ShutdownAsync();
                    Server.ChildSockets[child] = ChildSocketState.Disconnecting;
                }
                catch (Exception ex)
                {
                    ConsoleService.WriteLine("Child Socket error disconnecting from " + child.RemoteEndPoint + ": [" + ex.GetType().Name + "] " + ex.Message);
                    ResetChildSocket(child);
                }
            }

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        public void CloseAbortively()
        {
            // Keep a list of all errors for child sockets
            Dictionary<SimpleServerChildTcpSocket, Exception> SocketErrors = new Dictionary<SimpleServerChildTcpSocket, Exception>();

            // Abortively close all clients
            foreach (KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket in Server.ChildSockets)
            {
                try
                {
                    childSocket.Key.AbortiveClose();
                }
                catch (Exception ex)
                {
                    // Make a note of the error to handle later
                    SocketErrors.Add(childSocket.Key, ex);
                }
            }

            // Handle all errors. This is done outside the enumeration loop because the child socket
            //  error recovery will remove the socket from the list of child sockets.
            foreach (KeyValuePair<SimpleServerChildTcpSocket, Exception> error in SocketErrors)
            {
                ConsoleService.WriteLine("Child Socket error aborting " + error.Key.RemoteEndPoint + ": [" + error.Value.GetType().Name + "] " + error.Value.Message);
                ResetChildSocket(error.Key);
            }

            Server.ChildSockets.Clear();

            // In case there were any errors, the display may need to be updated
            RefreshDisplay();
        }

        public void DisplayIP()
        {
            StringBuilder sb = new StringBuilder();

            // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection)
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface network in networkInterfaces)
            {
                // Read the IP configuration for each network
                IPInterfaceProperties properties = network.GetIPProperties();

                // Each network interface may have multiple IP addresses
                foreach (IPAddressInformation address in properties.UnicastAddresses)
                {
                    // We're only interested in IPv4 addresses for now
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    // Ignore loopback addresses (e.g., 127.0.0.1)
                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    sb.AppendLine(address.Address + " (" + network.Name + ")");
                }
            }

            ConsoleService.WriteLine(sb.ToString());
        }
    } // TCPServerNito
} // namespace