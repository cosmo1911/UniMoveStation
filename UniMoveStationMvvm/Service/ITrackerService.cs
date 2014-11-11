using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMove;

namespace UniMoveStation.Model
{
    public interface ITrackerService
    {
        void AddMotionController(UniMoveController motionController);
        void RemoveMotionController(UniMoveController motionController);
        void UpdateImage();
        void ToggleCamera(SingleCameraModel.CameraState state);
        
    }
}
