using System;
using System.Windows;
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

        public bool Start() { return true; }

        public bool Stop() { return false; }

        public void Destroy() { }

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
