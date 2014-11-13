using UniMove;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using System.Windows.Media;
using UniMoveStation.Helper;
using System.Windows.Media.Imaging;

namespace UniMoveStation.Model
{
    
    public class SingleCameraModel : ObservableObject
    {
        private bool _annotate;
        private int _id;
        private string _name;
        private bool _showImage;
        private bool _tracking;
        private BitmapSource _bitmapSource;
        private ImageSource _imageSource;
        private UniMoveTracker _tracker;
        private List<UniMoveController> _moves;

        public bool Annotate
        {
            get
            {
                return _annotate;
            }
            set
            {
                Set(() => Annotate, ref _annotate, value);
            }
        }

        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                Set(() => Id, ref _id, value);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public bool ShowImage
        {
            get
            {
                return _showImage;
            }
            set
            {
                Set(() => ShowImage, ref _showImage, value);
            }
        }

        public bool Tracking
        {
            get
            {
                return _tracking;
            }
            set
            {
                Set(() => Tracking, ref _tracking, value);
            }
        }

        public ImageSource ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                Set(() => ImageSource, ref _imageSource, value);
            }
        }

        public BitmapSource BitmapSource
        {
            get
            {
                return _bitmapSource;
            }
            set
            {
                Set(() => BitmapSource, ref _bitmapSource, value);
            }
        }
        
        public UniMoveTracker Tracker
        {
            get 
            {
                return _tracker;
            }
            set
            {
                Set(() => Tracker, ref _tracker, value);
            }
        }

        public List<UniMoveController> Controllers
        {
            get
            {
                return _moves;
            }
            set
            {
                Set(() => Controllers, ref _moves, value);
            }
        }
    }
}
