using System;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Nito.Async;
using Nito.Async.Sockets;
using UniMoveStation.Common;
using UniMoveStation.Common.Utils;

namespace UniMoveStation.Business.Nito
{
    public class NitoClient
    {
        /// <summary>
        /// The connected state of the socket.
        /// </summary>
        protected enum SocketState
        {
            /// <summary>
            /// The socket is closed; we are not trying to connect.
            /// </summary>
            Closed,

            /// <summary>
            /// The socket is attempting to connect.
            /// </summary>
            Connecting,

            /// <summary>
            /// The socket is connected.
            /// </summary>
            Connected,

            /// <summary>
            /// The socket is attempting to disconnect.
            /// </summary>
            Disconnecting
        }

        /// <summary>
        /// The socket that connects to the server. This is null if ClientSocketState is SocketState.Closed.
        /// </summary>
        protected SimpleClientTcpSocket ClientSocket;

        /// <summary>
        /// The connected state of the socket. If this is SocketState.Closed, then ClientSocket is null.
        /// </summary>
        protected SocketState ClientSocketState;

        /// <summary>
        /// Closes and clears the socket, without causing exceptions.
        /// </summary>
        private void ResetSocket()
        {
            // Close the socket
            ClientSocket.Close();
            ClientSocket = null;

            // Indicate there is no socket connection
            ClientSocketState = SocketState.Closed;
        }

        /// <summary>
        /// Ensures the display matches the socket state.
        /// </summary>
        private void RefreshDisplay()
        {
            //// If the socket is connected, don't allow connecting it; if it's not, then don't allow disconnecting it
            //buttonConnect.Enabled = (ClientSocketState == SocketState.Closed);
            //buttonDisconnect.Enabled = (ClientSocketState == SocketState.Connected);
            //buttonAbortiveClose.Enabled = (ClientSocketState == SocketState.Connected);

            //// We can only send messages if we have a connection
            //buttonSendMessage.Enabled = (ClientSocketState == SocketState.Connected);
            //buttonSendComplexMessage.Enabled = (ClientSocketState == SocketState.Connected);

            // Display status
            switch (ClientSocketState)
            {
                case SocketState.Closed:
                    Console.WriteLine("Client stopped");
                    break;
                case SocketState.Connecting:
                    Console.WriteLine("Client connecting");
                    break;
                case SocketState.Connected:
                    Console.WriteLine("Client connected to " + ClientSocket.RemoteEndPoint);
                    break;
                case SocketState.Disconnecting:
                    Console.WriteLine("Client disconnecting");
                    break;
            }
        }

        private void ClientSocket_ConnectCompleted(AsyncCompletedEventArgs e)
        {
            try
            {
                // Check for errors
                if (e.Error != null)
                {
                    ResetSocket();
                    Console.WriteLine("Socket error during Connect: [" + e.Error.GetType().Name + "] " + e.Error.Message);
                    return;
                }

                // Adjust state
                ClientSocketState = SocketState.Connected;

                // Display the connection information
                Console.WriteLine("Connection established to " + ClientSocket.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Socket error during Connection: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        private void ClientSocket_WriteCompleted(object sender, AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                // Note: WriteCompleted may be called as the result of a normal write or a keepalive packet.

                ResetSocket();

                // If you want to get fancy, you can tell if the error is the result of a write failure or a keepalive
                //  failure by testing e.UserState, which is set by normal writes.
                if (e.UserState is string)
                    Console.WriteLine("Socket error during Write: [" + e.Error.GetType().Name + "] " + e.Error.Message);
                else
                    Console.WriteLine("Socket error detected by keepalive: [" + e.Error.GetType().Name + "] " + e.Error.Message);
            }
            else
            {
                string description = (string)e.UserState;
                Console.WriteLine("Socket write completed for message " + description);
            }

            RefreshDisplay();
        }

        private void ClientSocket_ShutdownCompleted(AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                ResetSocket();
                Console.WriteLine("Socket error during Shutdown: [" + e.Error.GetType().Name + "] " + e.Error.Message);
            }
            else
            {
                Console.WriteLine("Socket shutdown completed");

                // Close the socket and set the socket state
                ResetSocket();
            }

            RefreshDisplay();
        }

        private void ClientSocket_PacketArrived(AsyncResultEventArgs<byte[]> e)
        {
            try
            {
                // Check for errors
                if (e.Error != null)
                {
                    ResetSocket();
                    Console.WriteLine("Socket error during Read: [" + e.Error.GetType().Name + "] " + e.Error.Message);
                }
                else if (e.Result == null)
                {
                    // PacketArrived completes with a null packet when the other side gracefully closes the connection
                    Console.WriteLine("Socket graceful close detected");

                    // Close the socket and handle the state transition to disconnected.
                    ResetSocket();
                }
                else
                {
                    // At this point, we know we actually got a message.

                    // Deserialize the message
                    object message = SerializationHelper.Deserialize(e.Result);

                    if(handleMessages(message) == false)
                    {
                        Console.WriteLine("Socket read got an unknown message of type " + message.GetType().Name);
                    }
                }
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error reading from socket: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        protected virtual bool handleMessages(object message)
        {
            // Handle the message
            NitoMessages.StringMessage stringMessage = message as NitoMessages.StringMessage;
            if (stringMessage != null)
            {
                Console.WriteLine("Socket read got a string message: " + stringMessage.Message);
                if(stringMessage.Message.Equals("foo"))
                {
                    sendMessage("bar");
                }
                return true;
            }

            NitoMessages.ComplexMessage complexMessage = message as NitoMessages.ComplexMessage;
            if (complexMessage != null)
            {
                Console.WriteLine("Socket read got a complex message: (UniqueID = " + complexMessage.UniqueID +
                    ", Time = " + complexMessage.Time + ", Message = " + complexMessage.Message + ")");
                return true;
            }

            return false;
        }

        public void connect(string ip, int port)
        {
            try
            {
                // Read the IP address
                IPAddress serverIPAddress;
                if (!IPAddress.TryParse(ip, out serverIPAddress))
                {
                    Console.WriteLine("Invalid IP address: " + ip);
                    return;
                }

                // Begin connecting to the remote IP
                ClientSocket = new SimpleClientTcpSocket();
                ClientSocket.ConnectCompleted += ClientSocket_ConnectCompleted;
                ClientSocket.PacketArrived += ClientSocket_PacketArrived;
                ClientSocket.WriteCompleted += args => ClientSocket_WriteCompleted(ClientSocket, args);
                ClientSocket.ShutdownCompleted += ClientSocket_ShutdownCompleted;
                ClientSocket.ConnectAsync(serverIPAddress, port);
                ClientSocketState = SocketState.Connecting;
                Console.WriteLine("Connecting socket to " + (new IPEndPoint(serverIPAddress, port)));
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error creating connecting socket: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        public void disconnect()
        {
            try
            {
                ClientSocket.ShutdownAsync();
                ClientSocketState = SocketState.Disconnecting;
                Console.WriteLine("Disconnecting socket");
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error disconnecting socket: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        public void closeAbortively()
        {
            try
            {
                ClientSocket.AbortiveClose();
                ClientSocket = null;
                ClientSocketState = SocketState.Closed;
                Console.WriteLine("Abortively closed socket");
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error aborting socket: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        public void sendMessage(string message)
        {
            try
            {
                // Create the message to send
                NitoMessages.StringMessage msg = new NitoMessages.StringMessage();
                msg.Message = message;

                // Serialize the message to a binary array
                byte[] binaryMessage = SerializationHelper.Serialize(msg);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description = "<string message: " + msg.Message + ">";
                ClientSocket.WriteAsync(binaryMessage, description);

                Console.WriteLine("Sending message " + description);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error sending message to socket: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        public void sendComplexMessage(string message)
        {
            try
            {
                // Create the message to send
                NitoMessages.ComplexMessage msg = new NitoMessages.ComplexMessage();
                msg.UniqueID = Guid.NewGuid();
                msg.Time = DateTimeOffset.Now;
                msg.Message = message;

                // Serialize the message to a binary array
                byte[] binaryMessage = SerializationHelper.Serialize(msg);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description = "<complex message: " + msg.UniqueID + ">";
                ClientSocket.WriteAsync(binaryMessage, description);

                Console.WriteLine("Sending message " + description);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error sending message to socket: [" + ex.GetType().Name + "] " + ex.Message);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        // This is just a utility function for displaying all the IP(v4) addresses of a computer; it is not
        //  necessary in order to use ClientTcpSocket/ServerTcpSocket.
        public void displayIP(object sender, EventArgs e)
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

            Console.WriteLine(sb.ToString());
        }
    } // NitoClient
} // namespace