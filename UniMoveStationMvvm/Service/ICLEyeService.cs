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
    public interface ICLEyeService
    {
        bool Start(SingleCameraModel camera);

        bool Stop();

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
    }
}
