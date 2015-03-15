using System;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.Service.Event
{
    public class OnClientRemovedEventArgs : EventArgs
    {
        public ClientModel Client { get; set; }

        public OnClientRemovedEventArgs(ClientModel client)
        {
            Client = client;
        }
    }
}
