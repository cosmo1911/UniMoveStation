using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using UniMoveStation.Common;
using UnityEngine;

namespace UniMoveStation.Business.Model
{
    public class CamerasModel : ObservableObject
    {
        private ObservableCollection<CameraModel> _cameras;
        private ObservableCollection<MotionControllerModel> _controllers;
        private Vector3 _position;
        private string _name;
        private bool _annotate;
        private bool _bundleAdjusting;
        private bool _debug;
        private bool _tracking;
        private bool _showImage;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public bool Annotate
        {
            get { return _annotate; }
            set { Set(() => Annotate, ref _annotate, value); }
        }

        public bool BundleAdjusting
        {
            get { return _bundleAdjusting; }
            set { Set(() => BundleAdjusting, ref _bundleAdjusting, value); }
        }

        public bool Debug
        {
            get { return _debug; }
            set { Set(() => Debug, ref _debug, value); }
        }

        public bool Tracking
        {
            get { return _tracking; }
            set { Set(() => Tracking, ref _tracking, value); }
        }

        public bool ShowImage
        {
            get { return _showImage; }
            set { Set(() => ShowImage, ref _showImage, value); }
        }

        public ObservableCollection<CameraModel> Cameras
        {
            get { return _cameras ?? (_cameras = new ObservableCollection<CameraModel>()); }
            set { Set(() => Cameras, ref _cameras, value); }
        }

        public ObservableCollection<MotionControllerModel> Controllers
        {
            get { return _controllers ?? (_controllers = new ObservableCollection<MotionControllerModel>()); }
            set { Set(() => Controllers, ref _controllers, value); }
        }

        /// <summary>
        /// bundle adjusted position
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set { Set(() => Position, ref _position, value); }
        }


    }
}
