using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace UniMoveStation.IO
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    /// <summary>
    /// http://tech.pro/tutorial/704/csharp-tutorial-simple-threaded-tcp-server
    /// </summary>
    class TCPServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;

        public bool isAlive = true;

        public TCPServer(int port)
        {
            this.tcpListener = new TcpListener(IPAddress.Any, port);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
            MainWindow.console.AppendText(string.Format("Starting Server on port {0}...\n", port));
        }

        public virtual void Dispose()
        {
            isAlive = false;

            listenThread.Join(1000);

            if (listenThread.IsAlive)
            {
                listenThread.Abort();
            }

        }

        private void ListenForClients()
        {
            this.tcpListener.Start();

            while (isAlive)
            {
                //blocks until a client has connected to the server
                TcpClient client = this.tcpListener.AcceptTcpClient();

                //create a thread to handle communication 
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));

                clientThread.Start(client);
            }

            this.tcpListener.Stop();
        }

        private void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient) client;
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

                //message has successfully been received
                ASCIIEncoding encoder = new ASCIIEncoding();
                String msg = encoder.GetString(message, 0, bytesRead);
                Console.WriteLine(string.Format("Server in: {0}", msg));

                handleMessages(msg, stream);
            }
            tcpClient.Close();
        }

        protected virtual void handleMessages(String msg, NetworkStream clientStream)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] message = new byte[4096];

            if (msg.Equals("Hello Server!"))
            {
                string tmp = "Hello Client!";
                message = encoder.GetBytes(tmp);

                Console.WriteLine(string.Format("Server out: {0}", tmp));
                clientStream.Write(message, 0, message.Length);
                clientStream.Flush();
            }
        }
    }
}