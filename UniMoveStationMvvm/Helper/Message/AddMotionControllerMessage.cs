using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMoveStation.Model;

namespace UniMoveStation.Helper
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
