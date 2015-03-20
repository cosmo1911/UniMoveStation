using System;
using System.Windows.Media.Imaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;

namespace UniMoveStation.Business.Service.Design
{
    public class DesignTrackerService : ITrackerService
    {
        #region Member
        private CameraModel _camera;
        #endregion

        #region Interface Implementation
        public bool Enabled { get; set; }

        public void Initialize(CameraModel camera) { _camera = camera; }

        public bool Start()
        {
            if (_camera.TrackerId < -4 || _camera.TrackerId > -1) return false;

            string uri;
            if (_camera.Annotate)
            {
                uri = String.Format(@"/UniMoveStation.Business;component/Resources/cam{0}_tracker_annotate.png", -(_camera.TrackerId + 1));
            }
            else
            {
                uri = String.Format(@"/UniMoveStation.Business;component/Resources/cam{0}_tracker.png", -(_camera.TrackerId + 1));
            }
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute);
            bmp.EndInit();
            _camera.ImageSource = bmp;

            return Enabled = true;
        }

        public bool Stop() { return Enabled = false; }

        public void Destroy() { }

        public void UpdateImage() { }

        public void AddMotionController(MotionControllerModel mc) 
        {
            if (!_camera.Controllers.Contains(mc))
            {
                _camera.Controllers.Add(mc);
            }
        }

        public void RemoveMotionController(MotionControllerModel mc) 
        {
            _camera.Controllers.Remove(mc);
        }
        #endregion

        public event EventHandler OnImageReady;
    } // TrackerService
} // Namespace
