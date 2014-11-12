using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMove;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    public interface ITrackerService
    {
        bool Enabled
        {
            get;
            set;
        }

        void AddMotionController(UniMoveController motionController);

        void RemoveMotionController(UniMoveController motionController);

        void UpdateImage();

        bool Start();

        bool Stop();
    }
}
