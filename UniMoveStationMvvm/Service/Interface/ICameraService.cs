using CLEyeMulticam;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    public interface ICameraService
    {
        void Initialize(CameraModel camera);

        bool Start();

        bool Stop();

        void Destroy();

        bool Enabled
        {
            get;
            set;
        }

        CLEyeCameraDevice Device
        {
            get;
            set;
        }

        int GetConnectedCount();
    }
}
