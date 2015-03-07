using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Representation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ServerViewModel : ViewModelBase
    {
        private RelayCommand<object> _toggleServerCommand;
        private RelayCommand<bool> _disconnectClientsCommand;
        private RelayCommand<object> _sendMessageCommand;

        public ServerModel Server
        {
            get;
            private set;
        }

        public IConsoleService ConsoleService
        {
            get;
            private set;
        }

        public ServerService ServerService
        {
            get;
            private set;
        }

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        public ServerViewModel(IConsoleService consoleService, ServerService serverService)
        {
            Server = new ServerModel();
            ConsoleService = consoleService;
            ServerService = serverService;
            serverService.ConsoleService = consoleService;
        }

        [PreferredConstructor]
        public ServerViewModel() : this(new ConsoleService(), new ServerService())
        {

        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the ToggleServerCommand.
        /// </summary>
        public RelayCommand<object> ToggleServerCommand
        {
            get
            {
                return _toggleServerCommand
                    ?? (_toggleServerCommand = new RelayCommand<object>(DoToggleServer));
            }
        }

        /// <summary>
        /// Gets the DisconnectClientsCommand.
        /// </summary>
        public RelayCommand<bool> DisconnectClientsCommand
        {
            get
            {
                return _disconnectClientsCommand
                    ?? (_disconnectClientsCommand = new RelayCommand<bool>(DoDisconnectClients));
            }
        }

        /// <summary>
        /// Gets the SendMessageCommand.
        /// </summary>
        public RelayCommand<object> SendMessageCommand
        {
            get
            {
                return _sendMessageCommand
                    ?? (_sendMessageCommand = new RelayCommand<object>(DoSendMessage));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleServer(object param)
        {
            bool enabled = (bool) ((object[]) param)[0];
            
            if(enabled)
            {
                string tmp = ((object[])param)[1].ToString().Trim();
                int port = 3000;
                if (tmp.Length > 0) port = int.Parse(tmp);
                ServerService.Start(port);
            }
            else
            {
                ServerService.Stop();
            }
            
        }

        public void DoDisconnectClients(bool abortively)
        {
            if(abortively)
            {
                ServerService.CloseAbortively();
            }
            else
            {
                ServerService.Disconnect();
            }
        }

        public void DoSendMessage(object param)
        {
            bool complex = (bool) ((object[]) param)[0];
            string message = (string) ((object[]) param)[1];
            if(complex)
            {
                ServerService.SendComplexMessageAll(message);
            }
            else
            {
                ServerService.SendMessageAll(message);
            }
        }
        #endregion

        #region Misc
        public override void Cleanup()
        {
            ServerService.Stop();
            base.Cleanup();
        }
        #endregion
    } // ServerViewModel
} // namespace