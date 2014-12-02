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

namespace UniMoveStation.Model
{
    
    public class SingleCameraModel : ObservableObject
    {
        private bool _annotate = false;
        private int _trackerId = -1;
        private string _name = "name";
        private string _guid = "";
        private bool _showImage = false;
        private bool _tracking = false;
        private BitmapSource _bitmapSource;
        private ImageSource _imageSource;
        private IntPtr _handle;
        private ObservableCollection<MotionControllerModel> _controllers;

#if DEBUG
        private static int COUNTER = -1;
        public SingleCameraModel()
        {
            TrackerId = ++COUNTER;
            Name = "Design " + TrackerId;
            GUID = TrackerId + "1245678-9ABC-DEFG-HIJK-LMNOPQRSTUVW";
        }
#endif
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

        public int TrackerId
        {
            get { return _trackerId; }
            set { Set(() => TrackerId, ref _trackerId, value); }
        }

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
        /// OpenCv / PsMoveApi Image
        /// </summary>
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set { Set(() => ImageSource, ref _imageSource, value); }
        }

        /// <summary>
        /// CL Eye Image
        /// </summary>
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { Set(() => BitmapSource, ref _bitmapSource, value); }
        }
        
        public IntPtr Handle
        {
            get { return _handle; }
            set { Set(() => Handle, ref _handle, value); }
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
    }
}
