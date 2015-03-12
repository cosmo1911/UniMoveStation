using System.Windows;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Design;
using UniMoveStation.Business.Service.Event;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common;
using UniMoveStation.Representation.MessengerMessage;
using UniMoveStation.Representation.ViewModel.Flyout;
using System.Drawing;

namespace UniMoveStation.Representation.ViewModel
{
    public class CameraViewModel : ViewModelBase
    {
        #region Member
        private RelayCommand<bool> _toggleCameraCommand;
        private RelayCommand<bool> _toggleTrackingCommand;
        private RelayCommand<bool> _toggleAnnotateCommand;
        private RelayCommand<bool> _toggleVisualizationCommand;
        private RelayCommand<bool> _toggleDebugCommand;
        private RelayCommand _cancelCalibrationCommand;
        private RelayCommand _startCalibrationCommand;
        private RelayCommand _saveCalibrationCommand;

        public CameraModel Camera
        {
            get;
            private set;
        }

        public ITrackerService TrackerService
        {
            get;
            private set;
        }

        public ICameraService CameraService
        {
            get;
            private set;
        }

        public IConsoleService ConsoleService
        {
            get;
            private set;
        }

        public CameraCalibrationService CalibrationService
        {
            get; 
            private set;
        }

        public HelixCameraVisualizationService VisualizationService
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        [PreferredConstructor]
        public CameraViewModel(
            CameraModel camera, 
            ITrackerService trackerService, 
            ICameraService cameraService, 
            IConsoleService consoleService)
        {
            Camera = camera;
            TrackerService = trackerService;
            CameraService = cameraService;
            ConsoleService = consoleService;
            VisualizationService = new HelixCameraVisualizationService();
            CalibrationService = new CameraCalibrationService(SimpleIoc.Default.GetInstance<ISettingsService>());

            CameraService.Initialize(Camera);
            TrackerService.Initialize(Camera);
            VisualizationService.Initialize(Camera);
            CalibrationService.Initialize(Camera);

            Messenger.Default.Register<AddMotionControllerMessage>(this,
                message =>
                {
                    TrackerService.AddMotionController(message.MotionController);
                });

            Messenger.Default.Register<RemoveMotionControllerMessage>(this,
                message =>
                {
                    TrackerService.RemoveMotionController(message.MotionController);
                });

            // add existing controllers
            foreach (MotionControllerViewModel mcvw in SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>())
            {
                TrackerService.AddMotionController(mcvw.MotionController);
            }


            SimpleIoc.Default.Register(() => this, Camera.GUID, true);
            Messenger.Default.Send(new AddCameraMessage(Camera));
            // try loading previously saved calibration of this camera
            Camera.Calibration = SimpleIoc.Default.GetInstance<SettingsViewModel>().SettingsService.LoadCalibration(Camera.GUID);
        }

        /// <summary>
        /// for design time purposes only
        /// </summary>
        public CameraViewModel() : this(
            new CameraModel(), 
            new DesignTrackerService(),  
            new DesignClEyeService(), 
            new ConsoleService())
        {
            Camera.Name = "Design " + Camera.TrackerId;

#if DEBUG
            if (IsInDesignMode)
            {
                Camera.Controllers.Add((new MotionControllerModel()));
                Camera.Controllers.Add((new MotionControllerModel()));
                Camera.Controllers.Add((new MotionControllerModel()));
            }
#endif
        }
        #endregion

        #region Dependency Properties
        /// <summary>
        /// The <see cref="CLEyeImageControlVisibility" /> property's name.
        /// </summary>
        public const string CLEyeImageControlVisibilityPropertyName = "CLEyeImageControlVisibility";

        private Visibility _clEyeImageControlVisibility = Visibility.Hidden;

        /// <summary>
        /// Sets and gets the CLEyeImageControlVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility CLEyeImageControlVisibility
        {
            get
            {
                return _clEyeImageControlVisibility;
            }

            set
            {
                if (_clEyeImageControlVisibility == value)
                {
                    return;
                }

                _clEyeImageControlVisibility = value;
                RaisePropertyChanged(() => CLEyeImageControlVisibility);
            }
        }

        /// <summary>
        /// The <see cref="TrackerImageVisibility" /> property's name.
        /// </summary>
        public const string TrackerImageVisibilityPropertyName = "TrackerImageVisibility";

        private Visibility _trackerImageVisibility = Visibility.Hidden;

        /// <summary>
        /// Sets and gets the TrackerImageVisibility property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Visibility TrackerImageVisibility
        {
            get
            {
                return _trackerImageVisibility;
            }

            set
            {
                if (_trackerImageVisibility == value)
                {
                    return;
                }

                _trackerImageVisibility = value;
                RaisePropertyChanged(() => TrackerImageVisibility);
            }
        }
        #endregion

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
        /// Gets the ToggleAnnotateCommand.
        /// </summary>
        public RelayCommand<bool> ToggleVisualizationCommand
        {
            get
            {
                return _toggleVisualizationCommand
                    ?? (_toggleVisualizationCommand = new RelayCommand<bool>(DoToggleVisualization));
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


        /// <summary>
        /// Gets the ButtonCommand.
        /// </summary>
        public RelayCommand CancelCalibrationCommand
        {
            get
            {
                return _cancelCalibrationCommand
                    ?? (_cancelCalibrationCommand = new RelayCommand(DoCancelCalibration));
            }
        }

        /// <summary>
        /// Gets the StartCalibrationCommand.
        /// </summary>
        public RelayCommand StartCalibrationCommand
        {
            get
            {
                return _startCalibrationCommand
                    ?? (_startCalibrationCommand = new RelayCommand(
                        DoStartCalibration,
                        () => Camera.Calibration.StartFlag == false && Camera.TrackerId == 0));
            }
        }

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCalibrationCommand
                    ?? (_saveCalibrationCommand = new RelayCommand(
                        DoSaveCalibration,
                        () => Camera.Calibration.CurrentMode == CameraCalibrationMode.Calibrated));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleAnnotate(bool annotate)
        {
            Camera.Annotate = annotate;
            ConsoleService.WriteLine("Annotate: " + annotate);
        }

        public void DoToggleDebug(bool debug)
        {
            Camera.Debug = debug;
            ConsoleService.WriteLine("Debug: " + debug);
        }

        public void DoToggleCamera(bool enabled)
        {
            if (enabled)
            {
                if(TrackerService.Enabled)
                {
                    Camera.ShowImage = true;
                }
                else if(!CameraService.Enabled)
                {
                    Camera.ShowImage = CameraService.Start();
                }
            }
            else
            {
                if (CameraService.Enabled) Camera.ShowImage = CameraService.Stop();
                else if (TrackerService.Enabled) Camera.ShowImage = false;
            }
            ConsoleService.WriteLine("Show Image: " + Camera.ShowImage);
        }

        public void DoToggleTracking(bool enabled)
        {
            if (enabled)
            {
                if (CameraService.Enabled)
                {
                    CameraService.Stop();
                }
                Camera.Tracking = TrackerService.Start();
            }
            else
            {
                Camera.Tracking = TrackerService.Stop();
                if (Camera.ShowImage)
                {
                    CameraService.Start();
                }
            }
            ConsoleService.WriteLine("Tracking: " + enabled);
        }

        public void DoToggleVisualization(bool enabled)
        {
            if (enabled) VisualizationService.Start();
            else VisualizationService.Stop();
        }

        
        public void DoCancelCalibration()
        {
            CalibrationService.CancelCalibration();
        }

        public void DoStartCalibration()
        {
            CalibrationService.StartCalibration();
        }

        public void DoSaveCalibration()
        {
            CalibrationService.SaveCalibration();
        }
        #endregion

        #region Misc
        public override void Cleanup()
        {
            VisualizationService.Stop();
            TrackerService.Destroy();
            CameraService.Destroy();
            Messenger.Default.Send(new RemoveCameraMessage(Camera));
            SimpleIoc.Default.Unregister<CameraViewModel>(Camera.GUID);
            base.Cleanup();
        }      
        #endregion
    } // CameraViewModel
} // Namespace