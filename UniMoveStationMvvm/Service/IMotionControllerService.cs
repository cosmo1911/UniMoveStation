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
        MotionControllerModel Start();
        void Stop();
        void Initialize(int id);

        bool Enabled
        {
            get;
            set;
        }
    }
}
