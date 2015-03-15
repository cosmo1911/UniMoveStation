using System.Collections.ObjectModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;
using MahApps.Metro.Controls;

namespace UniMoveStation.Representation.ViewModel.Flyout
{
    public class AddCameraViewModel : FlyoutBaseViewModel
    {
        private CameraModel _newCamera;
        private bool _newCamerasDetected;
        private RelayCommand _cancelCommand;
        private RelayCommand _createCommand;
        private RelayCommand _refreshCommand;
        private RelayCommand<object> _selectItemCommand;
        private ObservableCollection<CameraModel> _availableCameras;

        /// <summary>
        /// Initializes a new instance of the AddCameraViewModel class.
        /// </summary>
        public AddCameraViewModel()
        {
            Position = Position.Right;
            Header = "Add Camera";
            AvailableCameras = new ObservableCollection<CameraModel>();
        }

        #region Properties
        public ObservableCollection<CameraModel> AvailableCameras
        {
            get { return _availableCameras; }
            set { Set(() => AvailableCameras, ref _availableCameras, value); }
        }
        public CameraModel NewCamera
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
                    ?? (_refreshCommand = new RelayCommand(DoRefresh));
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
            NewCamera = null;
            IsOpen = false;
        }

        public void DoCreateCommand()
        {
            if (NewCamera != null)
            {
                if (!NewCamera.Design)
                {
                    IConsoleService consoleService = new ConsoleService();
                    new CameraViewModel(
                        NewCamera, 
                        new TrackerService(consoleService), 
                        new ClEyeService(consoleService), 
                        consoleService,
                        new HelixCameraVisualizationService());
                }
                // add debug design
                else 
                {
                    new CameraViewModel();
                }
                IsOpen = false;
            }
        }

        public void DoSelectItemCommand(object item)
        {
            if (item != null)
            {
                string tmp = NewCamera.Name;
                NewCamera = (CameraModel) item;
                NewCamera.Name = tmp;
                NewCamera.Debug = false;
            }
        }

        public void DoRefresh()
        {
            ObservableCollection<CameraModel> existingCameras = new ObservableCollection<CameraModel>();
            AvailableCameras.Clear();
            NewCamera = new CameraModel
            {
                Name = null,
                Design = true
            };
            NewCamerasDetected = false;

            ICameraService cameraService = new ClEyeService(new ConsoleService());
            int connectedCount = cameraService.GetConnectedCount();
            if (connectedCount > 0)
            {
                foreach (CameraViewModel cameraViewModel in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                {
                    existingCameras.Add(cameraViewModel.Camera);
                }
                
                for (int i = 0; i < connectedCount; i++)
                {
                    CameraModel tmp = new CameraModel();
                    tmp.TrackerId = i;
                    cameraService.Initialize(tmp);
                    if(existingCameras.Count > 0)
                    {
                        bool duplicate = false;
                        foreach (CameraModel sc in existingCameras)
                        {
                            if (tmp.GUID.Equals(sc.GUID))
                            {
                                duplicate = true;
                                break;
                            }
                        }
                        if (!duplicate) AvailableCameras.Add(tmp);
                    }
                    else
                    {
                        AvailableCameras.Add(tmp);
                    }
                    
                    cameraService.Destroy();
                }

                if (AvailableCameras.Count > 0)
                {
                    NewCamerasDetected = true;
                }
            }
        }
        #endregion
    } // AddCameraViewModel
} // namespace