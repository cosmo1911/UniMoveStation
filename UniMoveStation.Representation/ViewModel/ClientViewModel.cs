using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Nito;
using UniMoveStation.Business.Service;
using UniMoveStation.Representation.MessengerMessage;

namespace UniMoveStation.Representation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ClientViewModel : ViewModelBase
    {
        private ClientModel _client;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the ClientViewModel class.
        /// </summary>
        public ClientViewModel(ClientModel client)
        {
            Client = client;
            client.Name = "Client";

            Messenger.Default.Register<RemoveClientMessage>(this, (message) =>
            {
                if (message.Client.RemoteEndPoint == Client.RemoteEndPoint)
                {
                    if (Client.ClientSocketState == ChildSocketState.Connected)
                    {
                        SimpleIoc.Default.GetInstance<ServerService>().Disconnect(Client);
                    }
                    SimpleIoc.Default.Unregister(this);
                }
            });
            SimpleIoc.Default.Register(() => this, client.RemoteEndPoint);
        }
        #endregion

        public ClientModel Client
        {
            get { return _client; }
            set { Set(() => Client, ref _client, value); }
        }

        public override void Cleanup()
        {
            SimpleIoc.Default.GetInstance<ServerService>().Disconnect(Client);
            Messenger.Default.Send(new RemoveClientMessage(Client));
            SimpleIoc.Default.Unregister(this);
            base.Cleanup();
        }
    } // ClientViewModel
} // namespace