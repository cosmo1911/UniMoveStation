using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UniMoveStation.IO
{
    class TCPClient : IDisposable
    {
        private Thread listenThread;
        public bool isAlive = true;

        private ASCIIEncoding encoder = new ASCIIEncoding();

        private TcpClient receiver;

        public TCPClient(string address, int port)
        {
            receiver = connect(address, port);
            this.listenThread = new Thread(new ThreadStart(ListenToServer));
            this.listenThread.Start();
            send(receiver, "Hello Server!");
        }

        private void ListenToServer()
        {

            //create a thread to handle communication 
            //with connected client
            Thread serverThread = new Thread(new ParameterizedThreadStart(HandleServerComm));
            serverThread.Start(receiver);
        }

        private void HandleServerComm(object server)
        {
            TcpClient tcpClient = (TcpClient) server;
            NetworkStream stream = tcpClient.GetStream();

            byte[] message = new byte[4096];
            int bytesRead;

            while (this.isAlive)
            {
                bytesRead = 0;

                try
                {
                    //blocks until a client sends a message
                    bytesRead = stream.Read(message, 0, 4096);
                }
                catch
                {
                    //a socket error has occured
                    break;
                }

                if (bytesRead == 0)
                {
                    //the client has disconnected from the server
                    break;
                }
                stream.Flush();
                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                String msg = encoder.GetString(message, 0, bytesRead);
                Console.WriteLine(string.Format("Client in: {0}", msg));

                handleMessages(msg, stream);
            }
        }

        protected virtual void handleMessages(string msg, NetworkStream stream)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] message = new byte[4096];

            String[] splitMessage = null;
            char[] splitCharacters = { '(', ')', ',', '=' };

            splitMessage = msg.Split(splitCharacters);

            if (splitMessage[0].Equals("Vector3"))
            {

            }
        }

        private TcpClient connect(string ip, int port)
        {
            TcpClient client = new TcpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            client.Connect(serverEndPoint);

            return client;
        }

        private void send(TcpClient receiver, string message)
        {
            NetworkStream clientStream = receiver.GetStream();
            byte[] buffer = encoder.GetBytes(message);

            Console.WriteLine(string.Format("Client out: {0}", message));
            clientStream.Write(buffer, 0, buffer.Length);
            clientStream.Flush();
        }

        public void send(string message)
        {
            send(receiver, message);
        }

        public void Dispose()
        {
            isAlive = false;
            listenThread.Join(1000);

            if(listenThread.IsAlive)
            {
                listenThread.Abort();
            }
        }
    }
}