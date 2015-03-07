using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace UniMoveStation.Business.Model
{
    public class ServerModel : ObservableObject
    {
        private bool _enabled;
        private List<object> _clients;

        public bool Enabled
        {
            get { return _enabled; }
            set { Set(() => Enabled, ref _enabled, value); }
        }

        public List<object> Clients
        {
            get { return _clients; }
            set { Set(() => Clients, ref _clients, value); }
        }
    }
}
