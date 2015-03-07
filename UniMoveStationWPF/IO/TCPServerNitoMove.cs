using Nito.Async.Sockets;
using System;
using System.Collections.Generic;
using UniMove;
using UnityEngine;

namespace UniMoveStation.IO
{
    public class TCPServerNitoMove : TCPServerNito
    {
        private ServerControl serverControl;
        private List<CameraControl> cameraControls;

        private List<UniMoveTracker> trackers;

        public TCPServerNitoMove(ServerControl serverControl) : base(serverControl.textBox_server)
        {
            this.serverControl = serverControl;
        }

        public void init(List<CameraControl> cameraControls)
        {
            this.cameraControls = cameraControls;
            trackers = new List<UniMoveTracker>();
            foreach (CameraControl cc in cameraControls)
            {
                trackers.Add(cc.tracker);
            }
        }

        public override void RefreshDisplay()
        {
            // we can only send messages or disconnect if we have connected clients
            serverControl.button_sendMessage.IsEnabled = (ChildSockets.Count != 0);
            serverControl.button_disconnectClients.IsEnabled = (ChildSockets.Count != 0);

            if (System.Windows.Application.Current != null)
            {
                MainWindow mainWindow = System.Windows.Application.Current.MainWindow as UniMoveStation.MainWindow;
                // Display status
                if (ListeningSocket == null)
                    mainWindow.statusBarTextBlock_listeningState.Text = "Stopped";
                else
                    mainWindow.statusBarTextBlock_listeningState.Text = "Listening on " + ListeningSocket.LocalEndPoint.ToString();
                mainWindow.statusBarTextBlock_connectionCount.Text = ChildSockets.Count + " Connections";
            }
        }

        protected override bool handleMessage(object message, SimpleServerChildTcpSocket socket)
        {
            ChildSocketState socketState = ChildSocketState.Disconnecting;
            ChildSockets.TryGetValue(socket, out socketState);
            KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket = new KeyValuePair<SimpleServerChildTcpSocket,ChildSocketState>(socket, socketState);

            UniMoveStation.NitoMessages.StringMessage stringMessage = message as UniMoveStation.NitoMessages.StringMessage;
            if (stringMessage != null)
            {
                textBox.AppendText("Socket read got a string message from " + socket.RemoteEndPoint.ToString() + ": " + stringMessage.Message + Environment.NewLine);
                if (stringMessage.Message.Contains("getFusionPosition"))
                {
                    String[] splitMessage = null;
                    char[] splitCharacters = { '(', ')', ',', ' ' };

                    splitMessage = stringMessage.Message.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);

                    int trackerIndex = Convert.ToInt32(splitMessage[1]);
                    int moveIndex = Convert.ToInt32(splitMessage[2]);
                    Vector3 position = getFusionPosition(trackerIndex, moveIndex);

                    sendPositionMessage(childSocket, position, trackerIndex, moveIndex);
                }
                return true;
            }

            UniMoveStation.NitoMessages.ComplexMessage complexMessage = message as UniMoveStation.NitoMessages.ComplexMessage;
            if (complexMessage != null)
            {
                textBox.AppendText("Socket read got a complex message from " + socket.RemoteEndPoint.ToString() + ": (UniqueID = " + complexMessage.UniqueID.ToString() +
                    ", Time = " + complexMessage.Time.ToString() + ", Message = " + complexMessage.Message + ")" + Environment.NewLine);
                textBox.AppendText(System.DateTime.Now.Millisecond.ToString() + Environment.NewLine);
                return true;
            }

            return base.handleMessage(message, socket);
        }

        public void sendPositionMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, Vector3 position, int trackerIndex, int moveIndex)
        {
            UniMoveStation.NitoMessages.PositionMessage msg = new UniMoveStation.NitoMessages.PositionMessage();
            msg.Message = position;
            msg.StartTick = System.DateTimeOffset.Now.Ticks;
            msg.TrackerIndex = trackerIndex;
            msg.MoveIndex = moveIndex;

            string description = string.Format("<{0} message: {1}, StartTick={2}, TrackerIndex={3}, MoveIndex={4}>", 
                msg.Message.GetType(), msg.Message, msg.StartTick, msg.TrackerIndex, msg.MoveIndex);

            // Serialize it to a binary array
            byte[] binaryObject = UniMoveStation.NitoMessages.Util.Serialize(msg);

            base.sendSerializedMessage(childSocket, binaryObject, description);
        }

        private Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            Vector3 vector = Vector3.zero;
            if (!cameraControls[trackerIndex].enabledForTracking)
            {
                Console.WriteLine(string.Format("Server: Tracker {0} not enabled." + Environment.NewLine, trackerIndex));
                return vector;
            }
            else if (trackers[trackerIndex] != null && trackers[trackerIndex].controllers[moveIndex] != null)
            {
                vector = trackers[trackerIndex].controllers[moveIndex].m_position;
            }
            return vector;
        }
    }
}