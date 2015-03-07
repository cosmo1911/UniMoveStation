using UniMoveStation.Business.CLEyeMulticam;
using UniMoveStation.Business.Model;

namespace UniMoveStation.Business.Service.Interfaces
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
