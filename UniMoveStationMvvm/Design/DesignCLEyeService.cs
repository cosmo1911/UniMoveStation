using CLEyeMulticam;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UniMoveStation.Model;
using UniMoveStation.Service;

namespace UniMoveStation.Design
{
    public class DesignCLEyeService : DependencyObject, ICameraService
    {
        private SingleCameraModel _camera;

        public void Initialize(SingleCameraModel camera)
        {
            _camera = camera;
            _camera.GUID = _camera.TrackerId + "1245678-9ABC-DEFG-HIJK-LMNOPQRSTUVW";
        }

        public bool Start() { return true; }

        public bool Stop() { return false; }

        public bool Enabled
        {
            get { return true; }
            set { Console.WriteLine(value); }
        }

        public CLEyeCameraDevice Device
        {
            get;
            set;
        }

        public int GetConnectedCount()
        {
            return 99;
        }
    }
}
