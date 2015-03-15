using GalaSoft.MvvmLight;
using UniMoveStation.Business.Nito;

namespace UniMoveStation.Business.Model
{
    public class ClientModel : ObservableObject
    {
        private string _name;
        private string _remoteEndPoint;
        private ChildSocketState _clientSocketState;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public string RemoteEndPoint
        {
            get { return _remoteEndPoint; }
            set { Set(() => RemoteEndPoint, ref _remoteEndPoint, value); }
        }

        /// <summary>
        /// The connected state of the socket. If this is SocketState.Closed, then ClientSocket is null.
        /// </summary>
        public ChildSocketState ClientSocketState
        {
            get { return _clientSocketState; }
            set { Set(() => ClientSocketState, ref _clientSocketState, value); }
        }
    }
}
