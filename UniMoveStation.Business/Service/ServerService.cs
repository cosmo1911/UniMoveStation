using System;
using System.Collections.Generic;
using System.Linq;
using Nito.Async.Sockets;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Nito;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.NitoMessages;
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

            PositionRequest positionRequest = message as PositionRequest;
            if (positionRequest != null)
            {
                string consoleString;
                Float3 position = Float3.Zero;

                if (positionRequest.Type != PositionType.Bundled)
                {
                    consoleString = String.Format("{0} requests {1} position of controller {2} from camera {3}.", 
                        socket.RemoteEndPoint,
                        Enum.GetName(typeof(PositionType), positionRequest.Type),
                        positionRequest.ControllerIndex,
                        positionRequest.CameraIndex);
                }
                else
                {
                    consoleString = String.Format("{0} requests {1} position of controller {2}.", 
                        socket.RemoteEndPoint,
                        Enum.GetName(typeof(PositionType), positionRequest.Type),
                        positionRequest.ControllerIndex);
                }

                switch (positionRequest.Type)
                {
                    case PositionType.Bundled:
                        position = GetBundledPosition(positionRequest.ControllerIndex);
                        break;
                    case PositionType.Camera:
                        break;
                    case PositionType.Fusion:
                        position = GetFusionPosition(positionRequest.CameraIndex, positionRequest.ControllerIndex);
                        break;
                    case PositionType.Raw:
                        break;
                    case PositionType.World:
                        position = GetWorldPosition(positionRequest.CameraIndex, positionRequest.ControllerIndex);
                        break;
                }

                ConsoleService.WriteLine(consoleString);

                SendPositionMessage(childSocket, new PositionMessage()
                {
                    Position = position,
                    Type = positionRequest.Type,
                    StartTick = positionRequest.StartTick,
                    CameraIndex = positionRequest.CameraIndex,
                    ControllerIndex = positionRequest.CameraIndex
                });
                return true;
            }

            return base.HandleMessage(message, socket);
        }

        private Float3 GetBundledPosition(int controllerIndex)
        {
            Float3 position = Float3.Zero;
            int visibleCount = 0;
            MotionControllerModel mc = GetController(controllerIndex);

            foreach (CameraModel camera in _cameras.Cameras)
            {
                if (mc.Tracking[camera])
                {
                    visibleCount++;
                    position += mc.WorldPosition[camera];
                }
            }

            return new Float3(position.X / visibleCount, position.Y / visibleCount, position.Z / visibleCount);
        }

        public void SendPositionMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, PositionMessage msg)
        {
            string description = string.Format("<{0} message: {1}, StartTick={2}, TrackerIndex={3}, MoveIndex={4}>", 
                msg.Position.GetType(), msg.Position, msg.StartTick, msg.CameraIndex, msg.ControllerIndex);

            // Serialize it to a binary array
            byte[] binaryObject = SerializationHelper.Serialize(msg);

            SendSerializedMessage(childSocket, binaryObject, description);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraIndex">calibration index</param>
        /// <param name="moveIndex"></param>
        /// <returns></returns>
        private Float3 GetFusionPosition(int cameraIndex, int moveIndex)
        {
            CameraModel camera = GetCamera(cameraIndex);
            MotionControllerModel mc = GetController(moveIndex);

            if (mc == null)
            {
                Console.WriteLine("[Server] Controller {0} not available.", moveIndex);
                return Vector3.zero;
            }
            if (camera == null)
            {
                Console.WriteLine("[Server] Tracker {0} not available.", cameraIndex);
                return Vector3.zero;
            }
            return mc.FusionPosition[camera];
        }

        private Float3 GetFusionPosition(string trackerName, string moveName)
        {
            CameraModel camera = GetCamera(trackerName);
            MotionControllerModel mc = GetController(moveName);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraIndex">calibration index</param>
        /// <param name="moveIndex"></param>
        /// <returns></returns>
        private Float3 GetWorldPosition(int cameraIndex, int moveIndex)
        {
            CameraModel camera = GetCamera(cameraIndex);
            MotionControllerModel mc = GetController(moveIndex);

            if (mc == null)
            {
                Console.WriteLine("[Server] Controller {0} not available.", moveIndex);
                return Float3.Zero;
            }
            if (camera == null)
            {
                Console.WriteLine("[Server] Tracker {0} not available.", cameraIndex);
                return Float3.Zero;
            }
            return mc.WorldPosition[camera];
        }

        private Float3 GetWorldPosition(string trackerName, string moveName)
        {
            CameraModel camera = GetCamera(trackerName);
            MotionControllerModel mc = GetController(moveName);

            if (mc == null)
            {
                Console.WriteLine("[Server] Controller \"{0}\" not available.", moveName);
                return Float3.Zero;
            }
            if (camera == null)
            {
                Console.WriteLine("[Server] Tracker \"{0}\" not available.", trackerName);
                return Float3.Zero;
            }
            return mc.FusionPosition[camera];
        }

        private CameraModel GetCamera(int index)
        {
            return _cameras.Cameras.FirstOrDefault(camera => camera.Calibration.Index == index);
        }

        private CameraModel GetCamera(string name)
        {
            return _cameras.Cameras.FirstOrDefault(camera => camera.Name.Equals(name));
        }

        private MotionControllerModel GetController(int index)
        {
            return _cameras.Controllers.FirstOrDefault(controller => controller.Id == index);
        }

        private MotionControllerModel GetController(string name)
        {
            return _cameras.Controllers.FirstOrDefault(controller => controller.Name.Equals(name));
        }
    } // ServerService
} // namespace
