using System;
using UniMoveStation.Model;
using UniMoveStation.Service;

namespace UniMoveStation.Design
{
    public class DesignTrackerService : ITrackerService
    {
        #region Member
        private CameraModel _camera;
        #endregion

        #region Interface Implementation
        public bool Enabled
        {
            get { return true; }
            set { Console.WriteLine(value); }
        }

        public void Initialize(CameraModel camera) { _camera = camera; }

        public bool Start() { return Enabled = true; }

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
        
    } // TrackerService
} // Namespace
