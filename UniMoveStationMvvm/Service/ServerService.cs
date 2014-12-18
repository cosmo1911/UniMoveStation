using GalaSoft.MvvmLight.Ioc;
using Nito.Async.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            UniMoveStation.Messages.StringMessage stringMessage = message as UniMoveStation.Messages.StringMessage;
            if (stringMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a string message from " + socket.RemoteEndPoint.ToString() + ": " + stringMessage.Message);
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

            UniMoveStation.Messages.ComplexMessage complexMessage = message as UniMoveStation.Messages.ComplexMessage;
            if (complexMessage != null)
            {
                ConsoleService.WriteLine("Socket read got a complex message from " + socket.RemoteEndPoint.ToString() + ": (UniqueID = " + complexMessage.UniqueID.ToString() +
                    ", Time = " + complexMessage.Time.ToString() + ", Message = " + complexMessage.Message + ")");
                ConsoleService.WriteLine(System.DateTime.Now.Millisecond.ToString());
                return true;
            }

            return base.HandleMessage(message, socket);
        }

        public void SendPositionMessage(KeyValuePair<SimpleServerChildTcpSocket, ChildSocketState> childSocket, Vector3 position, int trackerIndex, int moveIndex)
        {
            UniMoveStation.Messages.PositionMessage msg = new UniMoveStation.Messages.PositionMessage();
            msg.Message = position;
            msg.StartTick = System.DateTimeOffset.Now.Ticks;
            msg.TrackerIndex = trackerIndex;
            msg.MoveIndex = moveIndex;

            string description = string.Format("<{0} message: {1}, StartTick={2}, TrackerIndex={3}, MoveIndex={4}>", 
                msg.Message.GetType(), msg.Message, msg.StartTick, msg.TrackerIndex, msg.MoveIndex);

            // Serialize it to a binary array
            byte[] binaryObject = UniMoveStation.Messages.Util.Serialize(msg);

            base.SendSerializedMessage(childSocket, binaryObject, description);
        }

        private Vector3 getFusionPosition(int trackerIndex, int moveIndex)
        {
            Vector3 position = Vector3.zero;
            SingleCameraModel camera = GetCamera(trackerIndex);
            MotionControllerModel mc = GetMotionController(moveIndex);

            if(mc == null)
            {
                Console.WriteLine(string.Format("[Server] Controller {0} not available.", moveIndex));
                return Vector3.zero;
            }
            else if(camera == null)
            {
                Console.WriteLine(string.Format("[Server] Tracker {0} not available.", trackerIndex));
                return Vector3.zero;
            }
            else
            {
                return mc.Position[camera];
            }
        }

        private Vector3 getFusionPosition(string trackerName, string moveName)
        {
            Vector3 position = Vector3.zero;
            SingleCameraModel camera = GetCamera(trackerName);
            MotionControllerModel mc = GetMotionController(moveName);

            if (mc == null)
            {
                Console.WriteLine(string.Format("[Server] Controller \"{0}\" not available.", moveName));
                return Vector3.zero;
            }
            else if (camera == null)
            {
                Console.WriteLine(string.Format("[Server] Tracker \"{0}\" not available.", trackerName));
                return Vector3.zero;
            }
            else
            {
                return mc.Position[camera];
            }
        }

        private SingleCameraModel GetCamera(int index)
        {
            foreach (SingleCameraViewModel scvw in SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>())
            {
                if (scvw.Camera.TrackerId == index)
                {
                    return scvw.Camera;
                }
            }
            return null;
        }

        private SingleCameraModel GetCamera(string name)
        {
            foreach (SingleCameraViewModel scvw in SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>())
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
