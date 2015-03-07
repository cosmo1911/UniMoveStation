using UniMoveStation.Business.Model;
namespace UniMoveStation.Representation.MessengerMessage
{
    public class AddCameraMessage
    {
        public AddCameraMessage(CameraModel camera)
        {
            Camera = camera;
        }

        public CameraModel Camera
        {
            get;
            private set;
        }
    }
}
