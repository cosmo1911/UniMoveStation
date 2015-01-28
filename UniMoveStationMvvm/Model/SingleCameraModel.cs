using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Windows.Media;
using UniMoveStation.Helper;
using System.Windows.Media.Imaging;
using UniMoveStation.SharpMove;
using System;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.ViewModel;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Emgu.CV;

namespace UniMoveStation.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SingleCameraModel : ObservableObject
    {
        private bool _annotate = false;
        private int _trackerId = -1;
        private string _name = "name";
        private string _guid = "";
        private bool _showImage = false;
        private bool _tracking = false;
        private bool _debug = false;
        private BitmapSource _imageSource;
        private IntPtr _handle;
        private IntPtr _fusion;
        private ObservableCollection<MotionControllerModel> _controllers;

#if DEBUG
        private static int COUNTER = 0;
        public SingleCameraModel()
        {
            TrackerId = --COUNTER;
            Name = "Design " + TrackerId;
            GUID = TrackerId + "1245678-9ABC-DEFG-HIJK-LMNOPQRSTUVW";
            Calibration = new CameraCalibrationModel();
        }
#endif
        
        public CameraCalibrationModel Calibration
        {
            get;
            set;
        }

        [JsonProperty]
        public string GUID
        {
            get { return _guid; }
            set { Set(() => GUID, ref _guid, value); }
        }

        public bool Annotate
        {
            get { return _annotate; }
            set { Set(() => Annotate, ref _annotate, value); }
        }

        public bool Debug
        {
            get { return _debug; }
            set { Set(() => Debug, ref _debug, value); }
        }

        public int TrackerId
        {
            get { return _trackerId; }
            set { Set(() => TrackerId, ref _trackerId, value); }
        }

        [JsonProperty]
        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public bool ShowImage
        {
            get { return _showImage; }
            set { Set(() => ShowImage, ref _showImage, value); }
        }

        public bool Tracking
        {
            get { return _tracking; }
            set { Set(() => Tracking, ref _tracking, value); }
        }

        /// <summary>
        /// Camera Image
        /// </summary>
        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set { Set(() => ImageSource, ref _imageSource, value); }
        }

        public IntPtr Handle
        {
            get { return _handle; }
            set { Set(() => Handle, ref _handle, value); }
        }

        public IntPtr Fusion
        {
            get { return _fusion; }
            set { Set(() => Fusion, ref _fusion, value); }
        }

        public ObservableCollection<MotionControllerModel> Controllers
        {
            get 
            { 
                if(_controllers == null)
                {
                    _controllers = new ObservableCollection<MotionControllerModel>();
                    foreach(MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
                    {
                        _controllers.Add(mcvw.MotionController);
                    }
                }
                return _controllers; 
            }
            set { Set(() => Controllers, ref _controllers, value); }
        }

        [JsonProperty]
        public string Type
        {
            get { return "SingleCameraModel"; }
        }
    } // SingleCameraModel
} // namespace
