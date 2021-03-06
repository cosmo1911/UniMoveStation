﻿using UniMoveStation.Business.Model;

namespace UniMoveStation.Representation.MessengerMessage
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
