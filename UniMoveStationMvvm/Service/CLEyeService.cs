using CLEyeMulticam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    public class CLEyeService : DependencyObject, ICameraService
    {
        private SingleCameraModel _camera;

        public void Initialize(SingleCameraModel camera)
        {
            _camera = camera;
            Device = new CLEyeCameraDevice();
            Device.BitmapReady += Device_BitmapReady;
            Device.Resolution = CLEyeCameraResolution.CLEYE_VGA;
            Device.ColorMode = CLEyeCameraColorMode.CLEYE_COLOR_RAW;
            //CLEYE_QVGA - 15, 30, 60, 75, 100, 125
            //CLEYE_VGA - 15, 30, 40, 50, 60, 75
            Device.Framerate = 60;
            _camera.GUID = CLEyeCameraDevice.CameraUUID(_camera.TrackerId).ToString();
        }

        public int GetConnectedCount()
        {
            return CLEyeCameraDevice.CLEyeGetCameraCount();
        }

        public bool Start()
        {
            Device.Create(CLEyeCameraDevice.CameraUUID(_camera.TrackerId));
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

        #region Depedency Properties
        /// <summary>
        /// The <see cref="Enabled" /> dependency property's name.
        /// </summary>
        public const string EnabledPropertyName = "Enabled";

        /// <summary>
        /// Gets or sets the value of the <see cref="Enabled" />
        /// property. This is a dependency property.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return (bool)GetValue(EnabledProperty);
            }
            set
            {
                SetValue(EnabledProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Enabled" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnabledProperty = DependencyProperty.Register(
            EnabledPropertyName,
            typeof(bool),
            typeof(CLEyeService),
            new UIPropertyMetadata(default(bool)));


        public CLEyeCameraDevice Device
        {
            get;
            set;
        }

        #endregion
    }
}
