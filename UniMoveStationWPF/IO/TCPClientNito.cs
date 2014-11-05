using System;
using System.ComponentModel;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using Nito.Async.Sockets;
using Nito.Async;

namespace UniMoveStation.IO
{
    class TCPClientNito
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
                    Console.WriteLine("Client connected to " + ClientSocket.RemoteEndPoint.ToString());
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
                    Console.WriteLine("Socket error during Connect: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                    return;
                }

                // Adjust state
                ClientSocketState = SocketState.Connected;

                // Display the connection information
                Console.WriteLine("Connection established to " + ClientSocket.RemoteEndPoint.ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Socket error during Connection: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
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
                    Console.WriteLine("Socket error during Write: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                else
                    Console.WriteLine("Socket error detected by keepalive: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
            }
            else
            {
                string description = (string)e.UserState;
                Console.WriteLine("Socket write completed for message " + description + Environment.NewLine);
            }

            RefreshDisplay();
        }

        private void ClientSocket_ShutdownCompleted(AsyncCompletedEventArgs e)
        {
            // Check for errors
            if (e.Error != null)
            {
                ResetSocket();
                Console.WriteLine("Socket error during Shutdown: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
            }
            else
            {
                Console.WriteLine("Socket shutdown completed" + Environment.NewLine);

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
                    Console.WriteLine("Socket error during Read: [" + e.Error.GetType().Name + "] " + e.Error.Message + Environment.NewLine);
                }
                else if (e.Result == null)
                {
                    // PacketArrived completes with a null packet when the other side gracefully closes the connection
                    Console.WriteLine("Socket graceful close detected" + Environment.NewLine);

                    // Close the socket and handle the state transition to disconnected.
                    ResetSocket();
                }
                else
                {
                    // At this point, we know we actually got a message.

                    // Deserialize the message
                    object message = UniMoveStation.Messages.Util.Deserialize(e.Result);

                    if(handleMessages(message) == false)
                    {
                        Console.WriteLine("Socket read got an unknown message of type " + message.GetType().Name + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error reading from socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
            }
            finally
            {
                RefreshDisplay();
            }
        }

        protected virtual bool handleMessages(object message)
        {
            // Handle the message
            UniMoveStation.Messages.StringMessage stringMessage = message as UniMoveStation.Messages.StringMessage;
            if (stringMessage != null)
            {
                Console.WriteLine("Socket read got a string message: " + stringMessage.Message + Environment.NewLine);
                return true;
            }

            UniMoveStation.Messages.ComplexMessage complexMessage = message as UniMoveStation.Messages.ComplexMessage;
            if (complexMessage != null)
            {
                Console.WriteLine("Socket read got a complex message: (UniqueID = " + complexMessage.UniqueID.ToString() +
                    ", Time = " + complexMessage.Time.ToString() + ", Message = " + complexMessage.Message + ")" + Environment.NewLine);
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
                ClientSocket.WriteCompleted += (args) => ClientSocket_WriteCompleted(ClientSocket, args);
                ClientSocket.ShutdownCompleted += ClientSocket_ShutdownCompleted;
                ClientSocket.ConnectAsync(serverIPAddress, port);
                ClientSocketState = SocketState.Connecting;
                Console.WriteLine("Connecting socket to " + (new IPEndPoint(serverIPAddress, port)).ToString() + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error creating connecting socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
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
                Console.WriteLine("Disconnecting socket" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error disconnecting socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
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
                Console.WriteLine("Abortively closed socket" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error aborting socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
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
                UniMoveStation.Messages.StringMessage msg = new UniMoveStation.Messages.StringMessage();
                msg.Message = message;

                // Serialize the message to a binary array
                byte[] binaryMessage = UniMoveStation.Messages.Util.Serialize(msg);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description = "<string message: " + msg.Message + ">";
                ClientSocket.WriteAsync(binaryMessage, description);

                Console.WriteLine("Sending message " + description + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error sending message to socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
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
                UniMoveStation.Messages.ComplexMessage msg = new UniMoveStation.Messages.ComplexMessage();
                msg.UniqueID = Guid.NewGuid();
                msg.Time = DateTimeOffset.Now;
                msg.Message = message;

                // Serialize the message to a binary array
                byte[] binaryMessage = UniMoveStation.Messages.Util.Serialize(msg);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description = "<complex message: " + msg.UniqueID + ">";
                ClientSocket.WriteAsync(binaryMessage, description);

                Console.WriteLine("Sending message " + description + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ResetSocket();
                Console.WriteLine("Error sending message to socket: [" + ex.GetType().Name + "] " + ex.Message + Environment.NewLine);
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

                    sb.AppendLine(address.Address.ToString() + " (" + network.Name + ")");
                }
            }

            Console.WriteLine(sb.ToString());
        }
    }
}