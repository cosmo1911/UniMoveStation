using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Nito.Async;
using Nito.Async.Sockets;

using UnityEngine;

using UniMove;

namespace UniMoveStation.IO
{
    public class TCPServerNitoMove : TCPServerNito
    {
        private TCPServerNitoControl control;
        private List<CameraControlBox> cameraControls;
        private Button button_startListening;
        private Button button_stopListening;
        private Button button_sendMessage;
        private Button button_sendComplexMessage;
        private Button button_disconnectClients;
        private Button button_abortivelyCloseClients;

        private List<UniMoveTracker> trackers;

        public TCPServerNitoMove(TCPServerNitoControl control) : base(control.Controls.Find("textBox_server", true)[0] as TextBox)
        {
            this.control = control;

            button_startListening = control.Controls.Find("button_startListening", true)[0] as Button;
            button_stopListening = control.Controls.Find("button_stopListening", true)[0] as Button;
            button_sendMessage = control.Controls.Find("button_sendMessage", true)[0] as Button;
            button_sendComplexMessage = control.Controls.Find("button_sendComplexMessage", true)[0] as Button;
            button_disconnectClients = control.Controls.Find("button_disconnectClients", true)[0] as Button;
            button_abortivelyCloseClients = control.Controls.Find("button_abortivelyCloseClients", true)[0] as Button;
        }

        public void init(List<CameraControlBox> cameraControls)
        {
            this.cameraControls = cameraControls;
            trackers = new List<UniMoveTracker>();
            foreach (CameraControlBox cc in cameraControls)
            {
                trackers.Add(cc.tracker);
            }
        }

        public override void RefreshDisplay()
        {
            // if the server socket is running, don't allow starting it; if it's not, then don't allow stopping it
            button_startListening.Enabled = (ListeningSocket == null);
            button_stopListening.Enabled = (ListeningSocket != null);

            // we can only send messages or disconnect if we have connected clients
            button_sendMessage.Enabled = (ChildSockets.Count != 0);
            button_sendComplexMessage.Enabled = (ChildSockets.Count != 0);
            button_disconnectClients.Enabled = (ChildSockets.Count != 0);
            button_abortivelyCloseClients.Enabled = (ChildSockets.Count != 0);

            // Display status in status strip
            if (control.Parent != null)
            {
                StatusStrip strip = control.Parent.Parent.Controls.Find("StatusStrip", true)[0] as StatusStrip;
                // listening state
                if (ListeningSocket == null)
                    strip.Items[0].Text = "Stopped";
                else
                    strip.Items[0].Text = "Listening on " + ListeningSocket.LocalEndPoint.ToString();
                // connection count
                strip.Items[1].Text = ChildSockets.Count + " Connections";
            }
        }

        /// <summary>
        /// handles incoming messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="socket">Client that send the message</param>
        /// <returns></returns>
        protected override bool handleMessage(object message, SimpleServerChildTcpSocket socket)
        {
            ChildSocketState socketState = ChildSocketState.Disconnecting;
            ChildSockets.TryGetValue(socket, out socketState);
            KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket = new KeyValuePair<SimpleServerChildTcpSocket,ChildSocketState>(socket, socketState);

            // handle string message
            UniMoveStation.NitoMessages.StringMessage stringMessage = message as UniMoveStation.NitoMessages.StringMessage;
            if (stringMessage != null)
            {
                textBox.AppendText("Socket read got a string message from " + socket.RemoteEndPoint.ToString() + ": " + stringMessage.Message + Environment.NewLine);
                // handle position request
                if (stringMessage.Message.Contains("getFusionPosition"))
                {
                    // split message
                    String[] splitMessage = null;
                    char[] splitCharacters = { '(', ')', ',', ' ' };
                    splitMessage = stringMessage.Message.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);

                    // first parameter
                    int trackerIndex = Convert.ToInt32(splitMessage[1]);
                    // second paramater
                    int moveIndex = Convert.ToInt32(splitMessage[2]);
                    // get position according to parameters
                    Vector3 position = getFusionPosition(trackerIndex, moveIndex);
                    // send back the position
                    sendPositionMessage(childSocket, position, trackerIndex, moveIndex);
                }
                return true;
            }

            // handle complex message
            UniMoveStation.NitoMessages.ComplexMessage complexMessage = message as UniMoveStation.NitoMessages.ComplexMessage;
            if (complexMessage != null)
            {
                textBox.AppendText("Socket read got a complex message from " + socket.RemoteEndPoint.ToString() + ": (UniqueID = " + complexMessage.UniqueID.ToString() +
                    ", Time = " + complexMessage.Time.ToString() + ", Message = " + complexMessage.Message + ")" + Environment.NewLine);
                textBox.AppendText(System.DateTime.Now.Millisecond.ToString() + Environment.NewLine);
                return true;
            }

            // process base if nothing above was valid
            return base.handleMessage(message, socket);
        }

        /// <summary>
        /// sends information of a position of a tracker and controller to a client
        /// </summary>
        /// <param name="childSocket">The client receiving the message</param>
        /// <param name="position">Value of the position</param>
        /// <param name="trackerIndex">Index of the tracker (starting at 0)</param>
        /// <param name="moveIndex">Index of the controller (starting at 0)</param>
        public void sendPositionMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, Vector3 position, int trackerIndex, int moveIndex)
        {
            // initialize message
            UniMoveStation.NitoMessages.PositionMessage msg = new UniMoveStation.NitoMessages.PositionMessage();
            msg.Message = position;
            msg.StartTick = System.DateTimeOffset.Now.Ticks;
            msg.TrackerIndex = trackerIndex;
            msg.MoveIndex = moveIndex;

            // initialize message description
            string description = string.Format("<{0} message: {1}, StartTick={2}, TrackerIndex={3}, MoveIndex={4}>", 
                msg.Message.GetType(), msg.Message, msg.StartTick, msg.TrackerIndex, msg.MoveIndex);

            // serialize it to a binary array
            byte[] binaryObject = UniMoveStation.NitoMessages.Util.Serialize(msg);
            // send binary message
            base.sendSerializedMessage(childSocket, binaryObject, description);
        }

        /// <summary>
        /// retrieves the position of a controller from a tracker
        /// </summary>
        /// <param name="trackerIndex">Index of the tracker (starting at 0)</param>
        /// <param name="moveIndex">Index of the controller (starting at 0)</param>
        /// <returns></returns>
        private Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            Vector3 vector = Vector3.zero;
            // check if tracker is disabled
            if (!cameraControls[trackerIndex].enabledForTracking)
            {
                MainWindow.console.AppendText(string.Format("Server: Tracker {0} not enabled." + Environment.NewLine, trackerIndex));
                return vector;
            }
            // tracker is enabled and both the tracker and controller are available
            else if (trackers[trackerIndex] != null && trackers[trackerIndex].controllers[moveIndex] != null)
            {
                return trackers[trackerIndex].controllers[moveIndex].m_position;
            }
            return vector;
        }
    }
}