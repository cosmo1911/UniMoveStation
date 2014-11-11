using GalaSoft.MvvmLight;
using UniMoveStation.Model;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class AllCamerasViewModel : ViewModelBase
    {
        private AllCamerasModel _allCameras;
        private string _name;

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

        public AllCamerasModel AllCameras
        {
            get
            {
                return _allCameras;
            }
            private set
            {
                Set(() => AllCameras, ref _allCameras, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the AllCamerasViewModel class.
        /// </summary>
        public AllCamerasViewModel()
        {
            Name = "All";
            AllCameras = new AllCamerasModel();
        }
    }
}