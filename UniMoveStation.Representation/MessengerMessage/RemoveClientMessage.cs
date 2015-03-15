using System;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Representation.MessengerMessage
{
    public class RemoveClientMessage
    {
        public RemoveClientMessage(ClientModel client)
        {
            Client = client;
        }

        public RemoveClientMessage(ClientModel client, Action<bool> feedback)
        {
            Client = client;
            Feedback = feedback;
        }

        public ClientModel Client
        {
            get;
            private set;
        }

        public Action<bool> Feedback
        {
            get;
            private set;
        }
    }
}
