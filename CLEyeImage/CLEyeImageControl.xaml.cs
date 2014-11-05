using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CLEyeMulticam
{
    public partial class CLEyeImageControl : UserControl, IDisposable
    {
        public CLEyeImageControl()
        {
            InitializeComponent();
        }

        public CLEyeImageControl(int UUID)
        {
            InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            cameraImage.Dispose();
        }

        public void stop()
        {
            if (cameraImage.Device != null)
            {
                cameraImage.Visibility = System.Windows.Visibility.Hidden;
                cameraImage.Device.Dispose();
            }
        }

        public void start(int UUID)
        {
            if(cameraImage.Device != null)
            {
                cameraImage.Visibility = System.Windows.Visibility.Visible;
                cameraImage.Device.Resolution = CLEyeCameraResolution.CLEYE_QVGA;
                //cameraImage.Device.AutoExposure = true;
                //cameraImage.Device.AutoGain = true;
                //cameraImage.Device.AutoWhiteBalance = true;
                cameraImage.Device.ColorMode = CLEyeCameraColorMode.CLEYE_COLOR_PROCESSED;
                cameraImage.Device.Framerate = 10;
                cameraImage.Device.Create(CLEyeCameraDevice.CameraUUID(UUID));
                cameraImage.Device.Start();
            }
        }
    } //CLEyeImageControl
} //namespace
