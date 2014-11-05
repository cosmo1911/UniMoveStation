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
            this.Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

            init(UUID);
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Dispose();
        }

        public void Dispose()
        {
            cameraImage.Dispose();
        }

        private void init(int UUID)
        {
            cameraImage.Device.Create(CLEyeCameraDevice.CameraUUID(UUID));
            cameraImage.Device.Start();
        }
    }
}
