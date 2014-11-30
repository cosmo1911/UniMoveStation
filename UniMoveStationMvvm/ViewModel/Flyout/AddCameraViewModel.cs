using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UniMoveStation.SharpMove;

namespace UniMoveStation.ViewModel.Flyout
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class AddCameraViewModel : FlyoutBaseViewModel
    {
        private SingleCameraModel _newCamera;
        private bool _newCamerasDetected;
        private RelayCommand _cancelCommand;
        private RelayCommand _createCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand<object> _selectItemCommand;
        private ObservableCollection<SingleCameraModel> _availableCameras;

        /// <summary>
        /// Initializes a new instance of the AddCameraViewModel class.
        /// </summary>
        public AddCameraViewModel()
        {
            Position = Position.Right;
            Header = "Add Camera";
        }

        #region Properties
        public ObservableCollection<SingleCameraModel> AvailableCameras
        {
            get
            {
                if (_availableCameras == null)
                {
                    AvailableCameras = new ObservableCollection<SingleCameraModel>();
                }
                return _availableCameras;
            }
            set { Set(() => AvailableCameras, ref _availableCameras, value); }
        }
        public SingleCameraModel NewCamera
        {
            get { return _newCamera; }
            private set { Set(() => NewCamera, ref _newCamera, value); }
        }

        public bool NewCamerasDetected
        {
            get { return _newCamerasDetected; }
            set { Set(() => NewCamerasDetected, ref _newCamerasDetected, value); }
        }
        #endregion

        

        #region Commands
        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(DoCancelCommand));
            }
        }

        /// <summary>
        /// Gets the CreateCommand.
        /// </summary>
        public RelayCommand CreateCommand
        {
            get
            {
                return _createCommand
                    ?? (_createCommand = new RelayCommand(DoCreateCommand));
            }
        }

        /// <summary>
        /// Gets the RefreshCommand.
        /// </summary>
        public RelayCommand RefreshCommand
        {
            get
            {
                return _refreshCommand
                    ?? (_refreshCommand = new RelayCommand(DoRefreshCommand));
            }
        }

        /// <summary>
        /// Gets the SelectItemCommand.
        /// </summary>
        public RelayCommand<object> SelectItemCommand
        {
            get
            {
                return _selectItemCommand
                    ?? (_selectItemCommand = new RelayCommand<object>(DoSelectItemCommand));
            }
        }
        #endregion

        #region Command Executions
        public void DoCancelCommand()
        {
            NewCamera = new SingleCameraModel();
            IsOpen = false;
        }

        public void DoCreateCommand()
        {
            if (NewCamera != null)
            {
                SingleCameraViewModel scvw;
                if(!NewCamera.GUID.Contains("1245678"))
                {
                    scvw = new SingleCameraViewModel(NewCamera, new TrackerService(), new CLEyeService());
                }
                else 
                {
                    scvw = new SingleCameraViewModel();
                }

                ViewModelLocator.Instance.Navigation.CameraTabs.Add(scvw);
                IsOpen = false;
            }
        }

        public void DoSelectItemCommand(object item)
        {
            if (item != null)
            {
                string tmp = NewCamera.Name;
                NewCamera = (SingleCameraModel) item;
                NewCamera.Name = tmp;
            }
        }

        public void DoRefreshCommand()
        {
            ObservableCollection<SingleCameraModel> existingCameras = new ObservableCollection<SingleCameraModel>();
            AvailableCameras = new ObservableCollection<SingleCameraModel>();
            NewCamera = new SingleCameraModel();
            NewCamera.Name = null;
            NewCamerasDetected = false;

            ICameraService cameraService = new CLEyeService();
            int connectedCount = cameraService.GetConnectedCount();
            if (connectedCount > 0)
            {
                foreach (SingleCameraViewModel scvw in SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>())
                {
                    existingCameras.Add(scvw.Camera);
                }
                for (int i = 0; i < connectedCount; i++)
                {
                    SingleCameraModel tmp = new SingleCameraModel();
                    tmp.TrackerId = i;
                    cameraService.Initialize(tmp);
                    foreach (SingleCameraModel sc in existingCameras)
                    {
                        if (!tmp.GUID.Equals(sc.GUID))
                        {
                            if (existingCameras.IndexOf(sc) == existingCameras.Count - 1)
                            {
                                AvailableCameras.Add(tmp);
                            }
                        }
                    }
                    cameraService.Device.Dispose();
                }

                if (AvailableCameras.Count > 0)
                {
                    NewCamerasDetected = true;
                }
            }
        }
        #endregion
    }
}