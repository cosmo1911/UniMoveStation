using UniMoveStation.Model;

namespace UniMoveStation.Utils.MessengerMessage
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
