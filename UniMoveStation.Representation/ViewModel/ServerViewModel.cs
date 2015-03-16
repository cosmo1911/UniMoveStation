using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Event;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Representation.MessengerMessage;

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
        private RelayCommand<bool> _toggleServerCommand;
        private RelayCommand _disconnectClientsCommand;
        private RelayCommand _sendMessageCommand;

        public ServerModel Server { get; private set; }

        public IConsoleService ConsoleService { get; private set; }

        public ServerService ServerService { get; private set; }

        #region Constructor
        public ServerViewModel()
        {
            ConsoleService = new ConsoleService();
            ServerService = SimpleIoc.Default.GetInstance<ServerService>();

            Server = new ServerModel();
            ServerService.Initialize(
                ConsoleService, 
                Server, 
                SimpleIoc.Default.GetInstance<CamerasViewModel>().CamerasModel);

            ServerService.OnClientAddedHandler += delegate(object sender, EventArgs e)
            {
                OnClientAddedEventArgs args = (OnClientAddedEventArgs) e;
                new ClientViewModel(args.Client);
                Messenger.Default.Send(new AddClientMessage(args.Client));
            };

            ServerService.OnClientRemovedHandler += delegate(object sender, EventArgs e)
            {
                OnClientRemovedEventArgs args = (OnClientRemovedEventArgs)e;
                Messenger.Default.Send(new RemoveClientMessage(args.Client));
            };
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the ToggleServerCommand.
        /// </summary>
        public RelayCommand<bool> ToggleServerCommand
        {
            get
            {
                return _toggleServerCommand
                    ?? (_toggleServerCommand = new RelayCommand<bool>(DoToggleServer));
            }
        }

        /// <summary>
        /// Gets the DisconnectClientsCommand.
        /// </summary>
        public RelayCommand DisconnectClientsCommand
        {
            get
            {
                return _disconnectClientsCommand
                    ?? (_disconnectClientsCommand = new RelayCommand(DoDisconnectClients, 
                        () => Server.Enabled));
            }
        }

        /// <summary>
        /// Gets the SendMessageCommand.
        /// </summary>
        public RelayCommand SendMessageCommand
        {
            get
            {
                return _sendMessageCommand
                    ?? (_sendMessageCommand = new RelayCommand(DoSendMessage,
                        () => Server.Enabled));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleServer(bool enabled)
        {
            if(enabled)
            {
                ServerService.Start(Server.Port);
            }
            else
            {
                ServerService.Stop();
            }
            
        }

        public void DoDisconnectClients()
        {
            if(Server.DisconnectAbortively)
            {
                ServerService.CloseAbortively();
            }
            else
            {
                ServerService.Disconnect();
            }
        }

        public void DoSendMessage()
        {
            if(Server.IsMessageComplex)
            {
                ServerService.SendComplexMessageAll(Server.Message);
            }
            else
            {
                ServerService.SendMessageAll(Server.Message);
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