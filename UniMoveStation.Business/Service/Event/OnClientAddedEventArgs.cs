using System;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.Service.Event
{
    public class OnClientAddedEventArgs : EventArgs
    {
        public ClientModel Client { get; set; }

        public OnClientAddedEventArgs(ClientModel client)
        {
            Client = client;
        }
    }
}
