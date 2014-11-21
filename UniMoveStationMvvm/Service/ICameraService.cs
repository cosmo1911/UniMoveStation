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
        bool Start();

        bool Stop();

        void Initialize(SingleCameraModel camera);

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
