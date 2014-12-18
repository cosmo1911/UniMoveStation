using CLEyeMulticam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    public interface ICameraService
    {
        void Initialize(SingleCameraModel camera);

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
