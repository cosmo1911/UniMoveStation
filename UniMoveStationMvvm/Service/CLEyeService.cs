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
    public class CLEyeService : ICLEyeService
    {
        private SingleCameraModel _camera;

        public bool Start(SingleCameraModel camera)
        {
            _camera = camera;
            Device = new CLEyeCameraDevice();
            Device.BitmapReady += Device_BitmapReady;
            Device.Resolution = CLEyeCameraResolution.CLEYE_VGA;
            Device.ColorMode = CLEyeCameraColorMode.CLEYE_COLOR_RAW;
            //CLEYE_QVGA - 15, 30, 60, 75, 100, 125
            //CLEYE_VGA - 15, 30, 40, 50, 60, 75
            Device.Framerate = 60;
            Device.Create(CLEyeCameraDevice.CameraUUID(camera.Id));
            Device.AutoExposure = true;
            Device.AutoGain = true;
            Device.AutoWhiteBalance = true;
            Device.Start();
            return Enabled = true;
        }

        void Device_BitmapReady(object sender, EventArgs e)
        {
            _camera.BitmapSource = Device.BitmapSource;
        }

        public bool Stop()
        {
            Device.Stop();
            return Enabled = false;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public CLEyeCameraDevice Device
        {
            get;
            set;
        }
    }
}
