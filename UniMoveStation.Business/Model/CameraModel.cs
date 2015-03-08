using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace UniMoveStation.Business.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CameraModel : ObservableObject
    {
        private bool _annotate;
        private int _trackerId;
        private string _name;
        private string _guid;
        private bool _showImage;
        private bool _tracking;
        private bool _debug;
        private bool _visualization;
        private int _fps;
        private BitmapSource _imageSource;
        private IntPtr _handle;
        private IntPtr _fusion;
        private ObservableCollection<MotionControllerModel> _controllers;
        private CameraCalibrationModel _calibration;

#if DEBUG
        private static int COUNTER = -1;
        public CameraModel()
        {
            TrackerId = COUNTER--;
            Name = "Design " + TrackerId;
            GUID = TrackerId + "1245678-9ABC-DEFG-HIJK-LMNOPQRSTUVW";
            Calibration = new CameraCalibrationModel();
            FPS = 60;
        }
#endif

        public CameraCalibrationModel Calibration
        {
            get { return _calibration; }
            set { Set(() => Calibration, ref _calibration, value); }
        }

        [JsonProperty]
        public string GUID
        {
            get { return _guid; }
            set { Set(() => GUID, ref _guid, value); }
        }

        [JsonProperty]
        public int FPS
        {
            get { return _fps; }
            set { Set(() => FPS, ref _fps, value); }
        }

        [JsonProperty]
        public bool Visualization
        {
            get { return _visualization; }
            set { Set(() => Visualization, ref _visualization, value); }
        }

        [JsonProperty]
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
            get { return _controllers ?? (_controllers = new ObservableCollection<MotionControllerModel>()); }
            set { Set(() => Controllers, ref _controllers, value); }
        }

        [JsonProperty]
        public string Type
        {
            get { return "CameraModel"; }
        }
    } // CameraModel
} // namespace
