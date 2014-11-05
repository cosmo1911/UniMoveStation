using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.ComponentModel;
using UnityEngine;

namespace UniMoveStation.IO
{
    class TCPClientMove : TCPClient
    {
        public Vector3 position;

        public TCPClientMove(string address, int port) : base(address, port)
        {
            
        }

        protected override void handleMessages(string msg, NetworkStream stream)
        {
            base.handleMessages(msg, stream);

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] message = new byte[4096];

            String[] splitMessage = null;
            char[] splitCharacters = { '(', ')', ',', '=', ' ' };

            splitMessage = msg.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
            

            if (splitMessage[0].Equals("Vector3"))
            {
                float x = float.Parse(splitMessage[1]);
                float y = float.Parse(splitMessage[2]);
                float r = float.Parse(splitMessage[3]);
                position = new Vector3(x, y, r);
                Console.WriteLine("position " + position);
            }
        }
    }
}
