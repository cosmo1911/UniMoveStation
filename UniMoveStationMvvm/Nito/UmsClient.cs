using System;
using UnityEngine;
using UniMoveStation.Messages;


namespace UniMoveStation.Nito
{
    public class UmsClient : NitoClient
    {
        Vector3[,] positions = new Vector3[10, 10];

        public event EventHandler<PositionReceivedEventArgs> OnPositionReceived;

        public UmsClient() : base()
        {
            //add EventHandler
            OnPositionReceived += HandleOnPositionReceived;

            //initialize Array
            for (int i = 0; i <= positions.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= positions.GetUpperBound(1); j++)
                {
                    positions[i, j] = Vector3.zero;
                }
            }
        }

        public class PositionReceivedEventArgs : EventArgs
        {
            public PositionMessage message;

            public PositionReceivedEventArgs(PositionMessage message)
            {
                this.message = message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleOnPositionReceived(object sender, PositionReceivedEventArgs e)
        {
            positions[e.message.TrackerIndex, e.message.MoveIndex] = e.message.Message;
        }

        protected override bool handleMessages(object message)
        {
            UniMoveStation.Messages.PositionMessage positionMessage = message as UniMoveStation.Messages.PositionMessage;
            if (positionMessage != null)
            {
                Debug.Log("Socket read got a string message: " + positionMessage.Message
                    + ", Time=" + (System.DateTimeOffset.Now.Ticks - positionMessage.StartTick) / TimeSpan.TicksPerMillisecond + "ms"
                    + Environment.NewLine);
                OnPositionReceived(this, new PositionReceivedEventArgs(positionMessage));
                return true;
            }

            return base.handleMessages(message);
        }

        public Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            sendMessage(string.Format("getFusionPosition({0}, {1})", trackerIndex, moveIndex));
            return positions[trackerIndex, moveIndex];
        }
    } // UmsClient
} // namespace