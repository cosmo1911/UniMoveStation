using System;
using System.Windows;
using System.Windows.Media.Imaging;
using UniMoveStation.Business.CLEyeMulticam;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Business.Service.Design
{
    public class DesignClEyeService : DependencyObject, ICameraService
    {
        private CameraModel _camera;

        public void Initialize(CameraModel camera)
        {
            _camera = camera;
            _camera.GUID = _camera.TrackerId + "1245678-9ABC-DEFG-HIJK-LMNOPQRSTUVW";
        }

        public bool Start()
        {
            if(_camera.TrackerId < -4 || _camera.TrackerId > -1) return false;

            string uri = String.Format(@"/UniMoveStation.Business;component/Resources/cam{0}_cl.png", -(_camera.TrackerId + 1));
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            bmp.EndInit();
            _camera.ImageSource = bmp;

            return Enabled = true;
        }

        public bool Stop() { return Enabled = false; }

        public void Destroy() { }

        public bool Enabled { get; set; }

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
