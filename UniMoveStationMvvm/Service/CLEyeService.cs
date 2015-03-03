using CLEyeMulticam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UniMoveStation.Design;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    public class CLEyeService : DependencyObject, ICameraService
    {
        private SingleCameraModel _camera;

        public IConsoleService ConsoleService
        {
            get;
            set;
        }

        #region
        public CLEyeService(IConsoleService consoleService)
        {
            ConsoleService = consoleService;
        }

        public CLEyeService()
        {
            ConsoleService = new DesignConsoleService();
        }

        #endregion
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
            
            ConsoleService.WriteLine(string.Format("[Camera, {0}] Initialized", _camera.GUID));
            
        }

        public int GetConnectedCount()
        {
            ConsoleService.WriteLine("Camera Count: " + CLEyeCameraDevice.CLEyeGetCameraCount());
            
            return CLEyeCameraDevice.CLEyeGetCameraCount();
        }

        public bool Start()
        {
            Device.Create(CLEyeCameraDevice.CameraUUID(_camera.TrackerId));
            Device.AutoExposure = true;
            Device.AutoGain = true;
            Device.AutoWhiteBalance = true;
            Device.Start();
            
            ConsoleService.WriteLine(string.Format("[Camera, {0}] Started", _camera.GUID));
            ConsoleService.WriteLine(string.Format("[Camera, {0}] Resolution={1}, ColorMode={2}", _camera.GUID, Device.Resolution, Device.ColorMode));
            
            return Enabled = true;
        }

        void Device_BitmapReady(object sender, EventArgs e)
        {
            _camera.ImageSource = Device.BitmapSource;
        }

        public bool Stop()
        {
            Device.Stop();
            
            ConsoleService.WriteLine(string.Format("[Camera, {0}] Stopped.", _camera.GUID));
            
            return Enabled = false;
        }

        public void Destroy()
        {
            Device.Dispose();
            
            ConsoleService.WriteLine(string.Format("[Camera, {0}] Destroyed.", _camera.GUID));
        }

        #region Dependency Properties
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
            get { return (bool)GetValue(EnabledProperty); }
            set { SetValue(EnabledProperty, value); }
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
