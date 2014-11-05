using System;
using UnityEngine;
using UniMoveStation.Messages;


namespace UniMoveStation.IO
{
    class TCPClientNitoMove : TCPClientNito
    {
        // Array containing the positions of controllers beloning to a tracker
        Vector3[,] positions = new Vector3[10, 10];

        public event EventHandler<MyEventArgs> OnPositionReceived;

        public TCPClientNitoMove() : base()
        {
            // add EventHandler
            OnPositionReceived += HandleOnPositionReceived;

            // initialize array
            for (int i = 0; i <= positions.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= positions.GetUpperBound(1); j++)
                {
                    positions[i, j] = Vector3.zero;
                }
            }
        }

        public class MyEventArgs : EventArgs
        {
            public PositionMessage message;

            public MyEventArgs(PositionMessage message)
            {
                this.message = message;
            }
        }

        public void HandleOnPositionReceived(object sender, MyEventArgs e)
        {
            positions[e.message.TrackerIndex, e.message.MoveIndex] = e.message.Message;
        }

        protected override bool handleMessages(object message)
        {
            UniMoveStation.Messages.PositionMessage positionMessage = message as UniMoveStation.Messages.PositionMessage;
            if (positionMessage != null)
            {
                Debug.Log("Socket read got a string message: " + positionMessage.Message + ", Ticks=" + (System.DateTimeOffset.Now.Ticks - positionMessage.StartTick) / TimeSpan.TicksPerMillisecond + Environment.NewLine);
                OnPositionReceived(this, new MyEventArgs(positionMessage));
                //				positions[positionMessage.TrackerIndex, positionMessage.MoveIndex] = positionMessage.Message;
                return true;
            }

            return base.handleMessages(message);
        }

        public Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            sendMessage(string.Format("getFusionPosition({0}, {1})", trackerIndex, moveIndex));
            return positions[trackerIndex, moveIndex];
        }


    } // class
} // namespace