using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

using UnityEngine;

using UniMove;

namespace UniMoveStation.IO
{
    class TCPServerMove : TCPServer
    {
        private List<UniMoveTracker> trackers;
        private List<CameraControlBox> cameraControls;

        public TCPServerMove(int port, List<CameraControlBox> cameraControls)
            : base(port)
        {
            this.cameraControls = cameraControls;
            trackers = new List<UniMoveTracker>();
            foreach (CameraControlBox cc in cameraControls)
            {
                trackers.Add(cc.tracker);
            }
        }

        protected override void handleMessages(string msg, NetworkStream clientStream)
        {
            base.handleMessages(msg, clientStream);

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] message = new byte[4096];

            String[] splitMessage = null;
            char[] splitCharacters = { '(', ')', ',' };

            if(msg.Contains('('))
            {
                splitMessage = msg.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);
            }
           
            if(msg.Contains("getFusionPosition"))
            {
                int trackerIndex = Convert.ToInt32(splitMessage[1]);
                int moveIndex = Convert.ToInt32(splitMessage[2]);
                Vector3 position = getFusionPosition(trackerIndex, moveIndex);
                String output = "Vector3=" + position.ToString("G4");
                message = encoder.GetBytes(output);
                Console.WriteLine(string.Format("Server out: Tracker {0} / Move {1} -> {2}", trackerIndex, moveIndex, output));
                clientStream.Write(message, 0, message.Length);
                clientStream.Flush();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        private Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            Vector3 vector = Vector3.zero;
            if(!cameraControls[trackerIndex].enabledForTracking)
            {
                Console.WriteLine(string.Format("Server: Tracker {0} not enabled.", trackerIndex));
                return vector;
            }
            else if(trackers[trackerIndex] != null && trackers[trackerIndex].controllers[moveIndex] != null )
            {
                vector = trackers[trackerIndex].controllers[moveIndex].m_position;
            }
            return vector;
        }
    }
}