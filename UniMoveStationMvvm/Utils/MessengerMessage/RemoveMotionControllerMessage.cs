using UniMoveStation.Model;

namespace UniMoveStation.Utils.MessengerMessage
{
    public class RemoveMotionControllerMessage
    {
        public RemoveMotionControllerMessage(MotionControllerModel motionController)
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
