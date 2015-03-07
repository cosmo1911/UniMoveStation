using GalaSoft.MvvmLight.Ioc;
using Nito.Async.Sockets;
using System;
using System.Collections.Generic;
using UniMoveStation.Model;
using UniMoveStation.Nito;
using UniMoveStation.ViewModel;
using UnityEngine;

namespace UniMoveStation.Service
{
    public class ServerService : NitoServer
    {
        public ServerService() : base(null)
        {

        }

        public ServerService(IConsoleService consoleService) : base(consoleService)
        {

        }

        public override void RefreshDisplay()
        {
            // we can only send messages or disconnect if we have connected clients
            //serverControl.button_sendMessage.IsEnabled = (ChildSockets.Count != 0);
            //serverControl.button_disconnectClients.IsEnabled = (ChildSockets.Count != 0);

            //if (System.Windows.Application.Current != null)
            //{
            //    MainWindow mainWindow = System.Windows.Application.Current.MainWindow as UniMoveStation.MainWindow;
            //    // Display status
            //    if (ListeningSocket == null)
            //        mainWindow.statusBarTextBlock_listeningState.Text = "Stopped";
            //    else
            //        mainWindow.statusBarTextBlock_listeningState.Text = "Listening on " + ListeningSocket.LocalEndPoint.ToString();
            //    mainWindow.statusBarTextBlock_connectionCount.Text = ChildSockets.Count + " Connections";
            //}
        }

        protected override bool HandleMessage(object message, SimpleServerChildTcpSocket socket)
        {
            ChildSocketState socketState = ChildSocketState.Disconnecting;
            ChildSockets.TryGetValue(socket, out socketState);
            KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket = new KeyValuePair<SimpleServerChildTcpSocket,ChildSocketState>(socket, socketState);

            NitoMessages.StringMessage stringMessage = message as NitoMessages.StringMessage;
            if (stringMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a string message from " + socket.RemoteEndPoint + ": " + stringMessage.Message);
                if (stringMessage.Message.Contains("getFusionPosition"))
                {
                    String[] splitMessage = null;
                    char[] splitCharacters = { '(', ')', ',', ' ' };

                    splitMessage = stringMessage.Message.Split(splitCharacters, StringSplitOptions.RemoveEmptyEntries);

                    int trackerIndex = Convert.ToInt32(splitMessage[1]);
                    int moveIndex = Convert.ToInt32(splitMessage[2]);
                    Vector3 position = getFusionPosition(trackerIndex, moveIndex);

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
            byte[] binaryObject = Utils.SerializationHelper.Serialize(msg);

            SendSerializedMessage(childSocket, binaryObject, description);
        }

        private Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            Vector3 position = Vector3.zero;
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
            return mc.RawPosition[camera];
        }

        private Vector3 getFusionPosition(string trackerName, string moveName)
        {
            Vector3 position = Vector3.zero;
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
            return mc.RawPosition[camera];
        }

        private CameraModel GetCamera(int index)
        {
            foreach (CameraViewModel scvw in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
            {
                if (scvw.Camera.TrackerId == index)
                {
                    return scvw.Camera;
                }
            }
            return null;
        }

        private CameraModel GetCamera(string name)
        {
            foreach (CameraViewModel scvw in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
            {
                if (scvw.Camera.Name.Equals(name))
                {
                    return scvw.Camera;
                }
            }
            return null;
        }

        private MotionControllerModel GetMotionController(int index)
        {
            foreach (MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
            {
                if (mcvw.MotionController.Id == index)
                {
                    return mcvw.MotionController;
                }
            }
            return null;
        }

        private MotionControllerModel GetMotionController(string name)
        {
            foreach (MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
            {
                if (mcvw.MotionController.Name.Equals(name))
                {
                    return mcvw.MotionController;
                }
            }
            return null;
        }
    } // ServerService
} // namespace
