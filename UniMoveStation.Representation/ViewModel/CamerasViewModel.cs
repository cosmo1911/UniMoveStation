using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using UniMoveStation.Business.CLEyeMulticam;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Representation.MessengerMessage;
using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.Representation.ViewModel
{

    public class CamerasViewModel : ViewModelBase
    {
        private MultipleViewsService _multipleViewsService;
        private CamerasModel _camerasModel;
        private ObservableCollection<CameraViewModel> _cameraViewModels;

        private RelayCommand<bool> _toggleCameraCommand;
        private RelayCommand<bool> _toggleTrackingCommand;
        private RelayCommand<bool> _toggleAnnotateCommand;
        private RelayCommand<bool> _toggleDebugCommand;
        private RelayCommand<bool> _toggleBundleAdjustCommand;
        private RelayCommand _findFundamentalMatricesCommand;

        private RelayCommand<MetroWindow> _fcpCommand;
        private RelayCommand _cancelCommand;
        private RelayCommand _startCommand;
        private RelayCommand _saveCommand;

        public CamerasModel CamerasModel
        {
            get { return _camerasModel ?? (_camerasModel = new CamerasModel()); }
            set { Set(() => CamerasModel, ref _camerasModel, value); }
        }

        public ObservableCollection<CameraViewModel> CameraViewModels { 
            get { return _cameraViewModels ?? (_cameraViewModels = new ObservableCollection<CameraViewModel>()); }
            set { Set(() => CameraViewModels, ref _cameraViewModels, value); }
        }

        public MultipleViewsService MultipleViewsService
        {
            get { return _multipleViewsService; }
            set { Set(() => MultipleViewsService, ref _multipleViewsService, value); }
        }

        public CameraPositioningCalibrationService PositioningService { get; set; }

        /// <summary>
        /// Initializes a new instance of the AllCamerasViewModel class.
        /// </summary>
        public CamerasViewModel()
        {
            CamerasModel.Name = "all";
            CamerasModel.Cameras = new ObservableCollection<CameraModel>();
            CamerasModel.Controllers = new ObservableCollection<MotionControllerModel>();
            Refresh();
            MultipleViewsService = new MultipleViewsService();
            MultipleViewsService.Initialize(_camerasModel, CameraViewModels.Select(camera => camera.TrackerService).ToList());

            Messenger.Default.Register<AddCameraMessage>(this, message =>
            {
                CameraViewModel cvm = SimpleIoc.Default.GetInstance<CameraViewModel>(message.Camera.GUID);
                CameraViewModels.Add(cvm);
                CamerasModel.Cameras.Add(cvm.Camera);
                MultipleViewsService.TrackerServices.Add(cvm.TrackerService);
            });

            Messenger.Default.Register<RemoveCameraMessage>(this, message =>
            {
                if(SimpleIoc.Default.ContainsCreated<CameraViewModel>(message.Camera.GUID))
                {
                    CameraViewModel cvm = SimpleIoc.Default.GetInstance<CameraViewModel>(message.Camera.GUID);
                    CameraViewModels.Remove(cvm);
                    CamerasModel.Cameras.Remove(cvm.Camera);
                    MultipleViewsService.TrackerServices.Remove(cvm.TrackerService);
                }
            });

            Messenger.Default.Register<AddMotionControllerMessage>(this,
            message =>
            {
                CamerasModel.Controllers.Add(message.MotionController);
            });

            Messenger.Default.Register<RemoveMotionControllerMessage>(this,
            message =>
            {
                CamerasModel.Controllers.Remove(message.MotionController);
            });

            if(IsInDesignMode)
            {
                CamerasModel.Cameras.Add(new CameraViewModel().Camera);
                CamerasModel.Cameras.Add(new CameraViewModel().Camera);
                CamerasModel.Cameras.Add(new CameraViewModel().Camera);
                CamerasModel.Cameras.Add(new CameraViewModel().Camera);
                CamerasModel.Controllers.Add(new MotionControllerModel());
                CamerasModel.Controllers.Add(new MotionControllerModel());
            }
            else
            {
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().Settings.LoadCamerasOnStartUp)
                {
                    AddAvailableCameras();
                }
                if(SimpleIoc.Default.GetInstance<SettingsViewModel>().Settings.Debug)
                {
                    new CameraViewModel();
                    new CameraViewModel();
                    new CameraViewModel();
                    new CameraViewModel();
                }
            }
        }

        public void AddAvailableCameras()
        {
            int count = CLEyeCameraDevice.CameraCount;

            for(int i = 0; i < count; i++)
            {
                CameraModel camera = new CameraModel {TrackerId = i};
                IConsoleService consoleService = new ConsoleService();
                new CameraViewModel(
                    camera, 
                    new TrackerService(consoleService), 
                    new ClEyeService(consoleService), 
                    consoleService, 
                    new HelixCameraVisualizationService(),
                    new CameraCalibrationService(SimpleIoc.Default.GetInstance<ISettingsService>()));

            }
            Refresh();
        }

        public void Refresh()
        {
            CamerasModel.Cameras.Clear();
            foreach (CameraViewModel cameraViewModel in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
            {
                CamerasModel.Cameras.Add(cameraViewModel.Camera);
            }
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        #region Commands
        /// <summary>
        /// Gets the ToggleAnnotateCommand.
        /// </summary>
        public RelayCommand<bool> ToggleAnnotateCommand
        {
            get
            {
                return _toggleAnnotateCommand
                    ?? (_toggleAnnotateCommand = new RelayCommand<bool>(DoToggleAnnotate));
            }
        }

        /// <summary>
        /// Gets the ToggleDebugCommand.
        /// </summary>
        public RelayCommand<bool> ToggleDebugCommand
        {
            get
            {
                return _toggleDebugCommand
                    ?? (_toggleDebugCommand = new RelayCommand<bool>(DoToggleDebug));
            }
        }

        /// <summary>
        /// Gets the ToggleCameraCommand.
        /// </summary>
        public RelayCommand<bool> ToggleCameraCommand
        {
            get
            {
                return _toggleCameraCommand
                    ?? (_toggleCameraCommand = new RelayCommand<bool>(DoToggleCamera));
            }
        }

        /// <summary>
        /// Gets the ToggleTrackingCommand.
        /// </summary>
        public RelayCommand<bool> ToggleTrackingCommand
        {
            get
            {
                return _toggleTrackingCommand
                    ?? (_toggleTrackingCommand = new RelayCommand<bool>(DoToggleTracking));
            }
        }

        ///// <summary>
        ///// Gets the FindCorrespondingPointsCommand.
        ///// </summary>
        //public RelayCommand<MetroWindow> FindCorrespondingPointsCommand
        //{
        //    get
        //    {
        //        return _fcpCommand
        //            ?? (_fcpCommand = new RelayCommand<MetroWindow>(ShowFcpDialog));
        //    }
        //}

        ///// <summary>
        ///// Gets the CancelCommand.
        ///// </summary>
        //public RelayCommand CancelCommand
        //{
        //    get
        //    {
        //        return _cancelCommand
        //            ?? (_cancelCommand = new RelayCommand(DoCancelFcp));
        //    }
        //}

        ///// <summary>
        ///// Gets the StartCommand.
        ///// </summary>
        //public RelayCommand StartCommand
        //{
        //    get
        //    {
        //        return _startCommand
        //            ?? (_startCommand = new RelayCommand(DoStartFcp));
        //    }
        //}

        ///// <summary>
        ///// Gets the SaveCommand.
        ///// </summary>
        //public RelayCommand SaveCommand
        //{
        //    get
        //    {
        //        return _saveCommand
        //            ?? (_saveCommand = new RelayCommand(
        //                DoSaveFcp,
        //                () => _fcpFinished));
        //    }
        //}        
        
        ///// <summary>
        ///// Gets the FindFundamentalMatricesCommand.
        ///// </summary>
        //public RelayCommand FindFundamentalMatricesCommand
        //{
        //    get
        //    {
        //        return _findFundamentalMatricesCommand
        //            ?? (_findFundamentalMatricesCommand = new RelayCommand(DoFindFundamentalMatrices));
        //    }
        //}

        /// <summary>
        /// Gets the ToggleBundleAdjustCommand.
        /// </summary>
        public RelayCommand<bool> ToggleBundleAdjustCommand
        {
            get
            {
                return _toggleBundleAdjustCommand
                    ?? (_toggleBundleAdjustCommand = new RelayCommand<bool>(DoToggleBundleAdjust));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleAnnotate(bool annotate)
        {
            foreach (CameraModel camera in CamerasModel.Cameras)
            {
                SimpleIoc.Default.GetInstance<CameraViewModel>(camera.GUID).DoToggleAnnotate(annotate);
            }
            CamerasModel.Annotate = annotate;
        }

        public void DoToggleDebug(bool debug)
        {
            foreach (CameraModel camera in CamerasModel.Cameras)
            {
                SimpleIoc.Default.GetInstance<CameraViewModel>(camera.GUID).DoToggleDebug(debug);
            }
            CamerasModel.Debug = debug;
        }

        public void DoToggleCamera(bool enabled)
        {
            foreach (CameraModel camera in CamerasModel.Cameras)
            {
                SimpleIoc.Default.GetInstance<CameraViewModel>(camera.GUID).DoToggleCamera(enabled);
            }
            CamerasModel.ShowImage = enabled;
        }

        public void DoToggleTracking(bool enabled)
        {
            foreach (CameraModel camera in CamerasModel.Cameras)
            {
                SimpleIoc.Default.GetInstance<CameraViewModel>(camera.GUID).DoToggleTracking(enabled);
            }
            CamerasModel.Tracking = enabled;
        }
        #endregion

        private void DoToggleBundleAdjust(bool enable)
        {
            if (enable)
            {
                MultipleViewsService.StartBundleAdjustTask();
            }
            else
            {
                MultipleViewsService.CancelBundleTask();
            }
            _camerasModel.BundleAdjusting = enable;
        }
    } // CamerasViewModel
} // namespace