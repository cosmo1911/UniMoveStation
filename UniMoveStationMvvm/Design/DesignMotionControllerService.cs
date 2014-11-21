using io.thp.psmove;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniMove;
using UniMoveStation.Model;
using UniMoveStation.Service;

namespace UniMoveStation.Design
{
    class DesignMotionControllerService : IMotionControllerService
    {
        private MotionControllerModel _motionController;

        public MotionControllerModel MotionController
        {
            get
            {
                return _motionController;
            }
            set
            {
                _motionController = value;
            }
        }

        public MotionControllerModel Start()
        {
            return MotionController;
        }


        public void Initialize(int id)
        {
           
        }

        public void Stop()
        {

        }

        public bool Enabled
        {
            get;
            set;
        }
    }
}
