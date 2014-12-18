using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        void UpdateImage();

        void Initialize(SingleCameraModel camera);

        bool Start();

        bool Stop();

        void Destroy();

        void AddMotionController(MotionControllerModel mc);

        void RemoveMotionController(MotionControllerModel mc);
    }
}
