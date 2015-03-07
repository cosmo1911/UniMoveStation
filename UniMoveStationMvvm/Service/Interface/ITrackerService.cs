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

        void Initialize(CameraModel camera);

        bool Start();

        bool Stop();

        void Destroy();

        void AddMotionController(MotionControllerModel mc);

        void RemoveMotionController(MotionControllerModel mc);
    }
}
