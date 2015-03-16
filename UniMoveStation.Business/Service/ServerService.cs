using System;
using System.Collections.Generic;
using Nito.Async.Sockets;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Nito;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common.Utils;
using UnityEngine;

namespace UniMoveStation.Business.Service
{
    public class ServerService : NitoServer
    {
        private CamerasModel _cameras;

        public void Initialize(
            IConsoleService consoleService, 
            ServerModel server, 
            CamerasModel cameras)
        {
            base.Initialize(consoleService, server);
            _cameras = cameras;
        }

        protected override bool HandleMessage(object message, SimpleServerChildTcpSocket socket)
        {
            ChildSocketState socketState = ChildSocketState.Disconnecting;
            Server.ChildSockets.TryGetValue(socket, out socketState);
            KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket = new KeyValuePair<SimpleServerChildTcpSocket,ChildSocketState>(socket, socketState);

            NitoMessages.StringMessage stringMessage = message as NitoMessages.StringMessage;
            if (stringMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a string message from " + socket.RemoteEndPoint + ": " + stringMessage.Message);
                if (stringMessage.Message.Contains("GetFusionPosition"))
                {
                    String[] splitMessage = null;
                    char[] splitCharacters = { '(', ')', ',', ' ' };

                    splitMessage = stringMessage.Message.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);

                    int trackerIndex = Convert.ToInt32(splitMessage[1]);
                    int moveIndex = Convert.ToInt32(splitMessage[2]);
                    Vector3 position = GetFusionPosition(trackerIndex, moveIndex);

                    SendPositionMessage(childSocket, position, trackerIndex, moveIndex);
                }
                return true;
            }

            NitoMessages.ComplexMessage complexMessage = message as NitoMessages.ComplexMessage;
            if (complexMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a complex message from " + socket.RemoteEndPoint + ": (UniqueID = " + complexMessage.UniqueID +
                    ", Time = " + complexMessage.Time + ", Message = " + complexMessage.Message + ")");
                ConsoleService.WriteLine(DateTime.Now.Millisecond.ToString());
                return true;
            }

            return base.HandleMessage(message, socket);
        }

        public void SendPositionMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, Vector3 position, int trackerIndex, int moveIndex)
        {
            NitoMessages.PositionMessage msg = new NitoMessages.PositionMessage();
            msg.Message = position;
            msg.StartTick = DateTimeOffset.Now.Ticks;
            msg.TrackerIndex = trackerIndex;
            msg.MoveIndex = moveIndex;

            string description = string.Format("<{0} message: {1}, StartTick={2}, TrackerIndex={3}, MoveIndex={4}>", 
                msg.Message.GetType(), msg.Message, msg.StartTick, msg.TrackerIndex, msg.MoveIndex);

            // Serialize it to a binary array
            byte[] binaryObject = SerializationHelper.Serialize(msg);

            SendSerializedMessage(childSocket, binaryObject, description);
        }

        private Vector3 GetFusionPosition(int trackerIndex, int moveIndex)
        {
            CameraModel camera = GetCamera(trackerIndex);
            MotionControllerModel mc = GetMotionController(moveIndex);

            if(mc == null)
            {
                Console.WriteLine("[Server] Controller {0} not available.", moveIndex);
                return Vector3.zero;
            }
            if(camera == null)
            {
                Console.WriteLine("[Server] Tracker {0} not available.", trackerIndex);
                return Vector3.zero;
            }
            return mc.FusionPosition[camera];
        }

        private Vector3 GetFusionPosition(string trackerName, string moveName)
        {
            CameraModel camera = GetCamera(trackerName);
            MotionControllerModel mc = GetMotionController(moveName);

            if (mc == null)
            {
                Console.WriteLine("[Server] Controller \"{0}\" not available.", moveName);
                return Vector3.zero;
            }
            if (camera == null)
            {
                Console.WriteLine("[Server] Tracker \"{0}\" not available.", trackerName);
                return Vector3.zero;
            }
            return mc.FusionPosition[camera];
        }

        private CameraModel GetCamera(int index)
        {
            foreach (CameraModel camera in _cameras.Cameras)
            {
                if (camera.TrackerId == index)
                {
                    return camera;
                }
            }
            return null;
        }

        private CameraModel GetCamera(string name)
        {
            foreach (CameraModel camera in _cameras.Cameras)
            {
                if (camera.Name.Equals(name))
                {
                    return camera;
                }
            }
            return null;
        }

        private MotionControllerModel GetMotionController(int index)
        {
            foreach (MotionControllerModel controller in _cameras.Controllers)
            {
                if (controller.Id == index)
                {
                    return controller;
                }
            }
            return null;
        }

        private MotionControllerModel GetMotionController(string name)
        {
            foreach (MotionControllerModel controller in _cameras.Controllers)
            {
                if (controller.Name.Equals(name))
                {
                    return controller;
                }
            }
            return null;
        }
    } // ServerService
} // namespace
