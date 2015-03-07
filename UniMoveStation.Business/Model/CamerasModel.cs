using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace UniMoveStation.Business.Model
{
    public class CamerasModel : ObservableObject
    {
        private ObservableCollection<CameraModel> _cameras;

        public ObservableCollection<CameraModel> Cameras
        {
            get { return _cameras ?? (_cameras = new ObservableCollection<CameraModel>()); }
            set { Set(() => Cameras, ref _cameras, value); }
        }
    }
}
