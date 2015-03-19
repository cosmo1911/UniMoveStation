using System;
using System.Collections;
using UniMoveStation.NitoMessages;
using UnityEngine;

namespace UniMoveStation.Business.Nito
{
    public class UmsClient : NitoClient
    {
        public event EventHandler<PositionReceivedEventArgs> OnPositionReceived;

        public Vector3[,] Positions { get; set; }
        private Vector3 _newPosition;
        private Vector3 _oldPosition;

        public UmsClient()
        {
            // add handler to update the position
            OnPositionReceived += HandleOnPositionReceived;

            const int maxCameraCount = 5;
            const int maxControllerCount = 5;

            // initialize positions array
            for (int i = 0; i < maxCameraCount; i++)
            {
                for (int j = 0; j < maxControllerCount; j++)
                {
                    Positions[i, j] = Vector3.zero;
                }
            }
        }

        public void HandleOnPositionReceived(object sender, PositionReceivedEventArgs e)
        {
            _newPosition = e.Message.Position;

            Positions[e.Message.CameraIndex, e.Message.ControllerIndex] = e.Message.Position;
        }

        IEnumerator GetPosition(int trackerIndex, int moveIndex, PositionType type)
        {
            _oldPosition = _newPosition;

            SendPositionRequest(new PositionRequest
            {
                CameraIndex = trackerIndex,
                ControllerIndex = moveIndex,
                Type = type,
                StartTick = DateTimeOffset.Now.Ticks
            });

            while (ClientSocketState == SocketState.Connected && _oldPosition == _newPosition)
            {
                yield return _oldPosition;
            }
            Debug.Log("coroutine new position received");
            yield return _newPosition;
        }

        #region Message Handling
        public class PositionReceivedEventArgs : EventArgs
        {
            public PositionMessage Message { get; set; }

            public PositionReceivedEventArgs(PositionMessage message)
            {
                Message = message;
            }
        }

        public void SendPositionRequest(PositionRequest request)
        {
            try
            {
                // Serialize the message to a binary array
                byte[] binaryMessage = SerializationHelper.Serialize(request);

                // Send the message; the state is used by ClientSocket_WriteCompleted to display an output to the log
                string description =
                    "<position request message: Type = " +
                    request.Type + ", Controller = " +
                    request.ControllerIndex + ", Camera = " +
                    request.CameraIndex + ">";

                ClientSocket.WriteAsync(binaryMessage, description);

                Console.WriteLine("Sending position request " + description);
            }
            catch (Exception ex)
            {
                ResetSocket();
            }
        }

        protected override bool HandleMessages(object message)
        {
            PositionMessage positionMessage = message as PositionMessage;
            if (positionMessage != null)
            {
                Debug.Log("Socket read got a string message: " + positionMessage.Position
                    + ", Time=" + (DateTimeOffset.Now.Ticks - positionMessage.StartTick) / TimeSpan.TicksPerMillisecond + "ms"
                    + Environment.NewLine);
                if (OnPositionReceived != null)
                    OnPositionReceived(this, new PositionReceivedEventArgs(positionMessage));
                return true;
            }

            return base.HandleMessages(message);
        }
        #endregion
    } // UmsClient
} // namespace