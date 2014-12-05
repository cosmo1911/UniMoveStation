using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniMoveStation.Model
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
