using UniMoveStation.Business.Model;
namespace UniMoveStation.Representation.MessengerMessage
{
    public class AddClientMessage
    {
        public AddClientMessage(ClientModel client)
        {
            Client = client;
        }

        public ClientModel Client
        {
            get;
            private set;
        }
    }
}
