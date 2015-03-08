using System;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Event;

namespace UniMoveStation.Business.Service.Interfaces
{
    public interface ITrackerService
    {
        bool Enabled
        {
            get;
            set;
        }

        void UpdateImage();

        void Initialize(CameraModel camera);

        bool Start();

        bool Stop();

        void Destroy();

        void AddMotionController(MotionControllerModel mc);

        void RemoveMotionController(MotionControllerModel mc);

        event EventHandler OnImageReady;
    }
}
