using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMoveStation.Model;

namespace UniMoveStation.Helper
{
    public class RemoveCameraMessage
    {
        public RemoveCameraMessage(SingleCameraModel camera)
        {
            Camera = camera;
        }

        public RemoveCameraMessage(SingleCameraModel camera, Action<bool> feedback)
        {
            Camera = camera;
            Feedback = feedback;
        }

        public SingleCameraModel Camera
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
