using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UniMoveStation.Utilities;

namespace UniMoveStation.Design
{
    public class DesignTrackerService : ITrackerService
    {
        #region Member
        private SingleCameraModel _camera;
        #endregion

        #region Interface Implementation
        public bool Enabled
        {
            get { return true; }
            set { Console.WriteLine(value); }
        }

        public void Initialize(SingleCameraModel camera) { _camera = camera; }

        public bool Start() { return Enabled = true; }

        public bool Stop() { return Enabled = false; }

        public void Destroy() { }

        public void UpdateImage() { }

        public void AddMotionController(MotionControllerModel mc) 
        {
            _camera.Controllers.Add(mc);
        }

        public void RemoveMotionController(MotionControllerModel mc) 
        {
            _camera.Controllers.Remove(mc);
        }
        #endregion
        
    } // TrackerService
} // Namespace
