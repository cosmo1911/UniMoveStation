using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    public interface IMotionControllerService
    {
        void Start(MotionControllerModel motionController);
        void Stop();

        bool Enabled
        {
            get;
            set;
        }
    }
}
