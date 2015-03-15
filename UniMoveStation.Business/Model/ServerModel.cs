using System.Collections.Concurrent;
using GalaSoft.MvvmLight;
using Nito.Async.Sockets;
using UniMoveStation.Business.Nito;

namespace UniMoveStation.Business.Model
{
    public class ServerModel : ObservableObject
    {
        private bool _enabled;
        private ObservableConcurrentDictionary<SimpleServerChildTcpSocket, ChildSocketState> _childSockets;
        private ObservableConcurrentDictionary<SimpleServerChildTcpSocket, ClientModel> _clients;
        private int _port;
        private string _message;
        private bool _isMessageComplex;
        private bool _disconnectAbortively;

        public ServerModel()
        {
            _childSockets = new ObservableConcurrentDictionary<SimpleServerChildTcpSocket, ChildSocketState>();
            _clients = new ObservableConcurrentDictionary<SimpleServerChildTcpSocket, ClientModel>();
            _port = 3000;
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { Set(() => Enabled, ref _enabled, value); }
        }

        public bool IsMessageComplex
        {
            get { return _isMessageComplex; }
            set { Set(() => IsMessageComplex, ref _isMessageComplex, value); }
        }

        public bool DisconnectAbortively
        {
            get { return _disconnectAbortively; }
            set { Set(() => DisconnectAbortively, ref _disconnectAbortively, value); }
        }

        public int Port
        {
            get { return _port; }
            set { Set(() => Port, ref _port, value); }
        }

        public string Message
        {
            get { return _message; }
            set { Set(() => Message, ref _message, value); }
        }
        
        /// <summary>
        /// A mapping of sockets (with established connections) to their state.
        /// </summary>
        public ObservableConcurrentDictionary<SimpleServerChildTcpSocket, ChildSocketState> ChildSockets
        {
            get { return _childSockets; }
            set { Set(() => ChildSockets, ref _childSockets, value); }
        }

        public ObservableConcurrentDictionary<SimpleServerChildTcpSocket, ClientModel> Clients
        {
            get { return _clients; }
            set { Set(() => Clients, ref _clients, value); }
        }
    }
}
