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
            AvailableCameras = new ObservableCollection<SingleCameraModel>();
        }

        #region Properties
        public ObservableCollection<SingleCameraModel> AvailableCameras
        {
            get { return _availableCameras; }
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
            NewCamera = new SingleCameraModel();
            IsOpen = false;
        }

        public void DoCreateCommand()
        {
            if (NewCamera != null)
            {
                SingleCameraViewModel scvw;
                if (!NewCamera.GUID.Contains("9ABC-DEFG-HIJK-LMNOPQRSTUVW"))
                {
                    IConsoleService consoleService = new ConsoleService();
                    scvw = new SingleCameraViewModel(NewCamera, new TrackerService(consoleService), new CLEyeService(consoleService), consoleService);
                }
                else 
                {
                    scvw = new SingleCameraViewModel();
                }
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

        public void DoRefresh()
        {
            ObservableCollection<SingleCameraModel> existingCameras = new ObservableCollection<SingleCameraModel>();
            AvailableCameras.Clear();
            NewCamera = new SingleCameraModel()
            {
                Name = null
            };
            NewCamerasDetected = false;

            ICameraService cameraService = new CLEyeService(new ConsoleService());
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
                    if(existingCameras.Count > 0)
                    {
                        bool duplicate = false;
                        foreach (SingleCameraModel sc in existingCameras)
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