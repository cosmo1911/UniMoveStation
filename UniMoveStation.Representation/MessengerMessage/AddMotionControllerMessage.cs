using UniMoveStation.Business.Model;

namespace UniMoveStation.Representation.MessengerMessage
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
