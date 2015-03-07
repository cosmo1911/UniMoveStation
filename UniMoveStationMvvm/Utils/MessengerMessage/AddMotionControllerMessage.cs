using UniMoveStation.Model;

namespace UniMoveStation.Utils.MessengerMessage
{
    public class AddMotionControllerMessage
    {
        public AddMotionControllerMessage(MotionControllerModel motionController)
        {
            MotionController = motionController;
        }

        public MotionControllerModel MotionController
        {
            get;
            private set;
        }
    }
}
