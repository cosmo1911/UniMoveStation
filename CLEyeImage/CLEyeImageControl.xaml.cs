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

        public void Stop()
        {
            if (cameraImage.Device != null)
            {
                cameraImage.Visibility = System.Windows.Visibility.Hidden;
                cameraImage.Device.Dispose();
            }
        }

        public void Start(int UUID)
        {
            if(cameraImage.Device != null)
            {
                
                cameraImage.Visibility = System.Windows.Visibility.Visible;
                cameraImage.Device.Resolution = CLEyeCameraResolution.CLEYE_VGA;
                cameraImage.Device.ColorMode = CLEyeCameraColorMode.CLEYE_COLOR_RAW;
                //CLEYE_QVGA - 15, 30, 60, 75, 100, 125
                //CLEYE_VGA - 15, 30, 40, 50, 60, 75
                cameraImage.Device.Framerate = 60;
                cameraImage.Device.Create(CLEyeCameraDevice.CameraUUID(UUID));
                cameraImage.Device.AutoExposure = true;
                cameraImage.Device.AutoGain = true;
                cameraImage.Device.AutoWhiteBalance = true;
                //cameraImage.Device.Gain = 0;
                //cameraImage.Device.Exposure = 0;
                //cameraImage.Device.WhiteBalanceBlue = 0;
                //cameraImage.Device.WhiteBalanceRed = 0;
                //cameraImage.Device.WhiteBalanceGreen = 0;
                cameraImage.Device.Start();
            }
        }
    } //CLEyeImageControl
} //namespace
