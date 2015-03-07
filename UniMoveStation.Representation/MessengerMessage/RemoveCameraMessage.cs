using System;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Representation.MessengerMessage
{
    public class RemoveCameraMessage
    {
        public RemoveCameraMessage(CameraModel camera)
        {
            Camera = camera;
        }

        public RemoveCameraMessage(CameraModel camera, Action<bool> feedback)
        {
            Camera = camera;
            Feedback = feedback;
        }

        public CameraModel Camera
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
