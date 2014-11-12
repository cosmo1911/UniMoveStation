using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using UniMoveStation.Model;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NavigationViewModel : ViewModelBase
    {
        private CompositeCollection _cameras;
        private ObservableCollection<SingleCameraViewModel> _singleCameras;
        private RelayCommand<int> _selectControllerCommand;
        private RelayCommand<int> _selectCameraCommand;
        private int _mainSelectedIndex = 0;
        private int _controllerSelectedIndex = 0;
        private int _cameraSelectedIndex = 0;

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the NavigationViewModel class.
        /// </summary>
        [PreferredConstructor]
        public NavigationViewModel()
        {
            Cameras = new CompositeCollection();

            SingleCameras.Add(new SingleCameraViewModel(0));
            SingleCameras.Add(new SingleCameraViewModel(1337));
            SingleCameras.Add(new SingleCameraViewModel(1));
        }
        #endregion

        #region Properties
        /// <summary>
        /// The <see cref="MainSelectedIndex" /> property's name.
        /// </summary>
        public const string MainSelectedIndexPropertyName = "MainSelectedIndex";

        /// <summary>
        /// Sets and gets the MainSelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int MainSelectedIndex
        {
            get
            {
                return _mainSelectedIndex;
            }
            set
            {
                Set(MainSelectedIndexPropertyName, ref _mainSelectedIndex, value);
            }
        }

        /// <summary>
        /// The <see cref="ControllerSelectedIndex" /> property's name.
        /// </summary>
        public const string ControllerSelectedIndexPropertyName = "ControllerSelectedIndex";

        /// <summary>
        /// Sets and gets the ControllerSelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ControllerSelectedIndex
        {
            get
            {
                return _controllerSelectedIndex;
            }
            set
            {
                Set(ControllerSelectedIndexPropertyName, ref _controllerSelectedIndex, value);
            }
        }

        /// <summary>
        /// The <see cref="CameraSelectedIndex" /> property's name.
        /// </summary>
        public const string CameraSelectedIndexPropertyName = "CameraSelectedIndex";

        /// <summary>
        /// Sets and gets the CameraSelectedIndex property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int CameraSelectedIndex
        {
            get
            {
                return _cameraSelectedIndex;
            }
            set
            {
                Set(CameraSelectedIndexPropertyName, ref _cameraSelectedIndex, value);
            }
        }

        public CompositeCollection Cameras
        {
            get
            {
                CompositeCollection collection = new CompositeCollection();
                collection.Add(AllCameras);
                foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllInstances<SingleCameraViewModel>())
                {
                    collection.Add(scvm);
                }
                return collection;
            }
            private set
            {
                Set(() => Cameras, ref _cameras, value);
            }
        }

        public ObservableCollection<SingleCameraViewModel> SingleCameras
        {
            get
            {
                ObservableCollection<SingleCameraViewModel> collection = new ObservableCollection<SingleCameraViewModel>();
                foreach(SingleCameraViewModel scvm in SimpleIoc.Default.GetAllInstances<SingleCameraViewModel>())
                {
                    collection.Add(scvm);
                }
                return collection;
            }
            private set
            {
                Set(() => SingleCameras, ref _singleCameras, value);
            }
        }

        public AllCamerasViewModel AllCameras
        {
            get
            {
                return SimpleIoc.Default.GetInstance<AllCamerasViewModel>();
            }
        }
        #endregion

        #region Relay Commands
        /// <summary>
        /// Gets the SelectControllerCommand.
        /// </summary>
        public RelayCommand<int> SelectControllerCommand
        {
            get
            {
                return _selectControllerCommand
                    ?? (_selectControllerCommand = new RelayCommand<int>(DoSelectControllerCommand));
            }
        }

        /// <summary>
        /// Gets the SelectControllerCommand.
        /// </summary>
        public RelayCommand<int> SelectCameraCommand
        {
            get
            {
                return _selectCameraCommand
                    ?? (_selectCameraCommand = new RelayCommand<int>(DoSelectCameraCommand));
            }
        }

        private void DoSelectControllerCommand(int index)
        {
            MainSelectedIndex = 0;
            ControllerSelectedIndex = index + 1;
        }

        private void DoSelectCameraCommand(int index)
        {
            MainSelectedIndex = 1;
            CameraSelectedIndex = index + 1;
        }
        #endregion
    }
}