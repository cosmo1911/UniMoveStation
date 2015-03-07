using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service;
using UniMoveStation.Business.Service.Design;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Representation.MessengerMessage;
using UniMoveStation.Representation.ViewModel.Flyout;

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

            CameraService.Initialize(Camera);
            TrackerService.Initialize(Camera);
            VisualizationService.Initialize(Camera);

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

            SimpleIoc.Default.Register(() => this, Camera.GUID, true);
            Messenger.Default.Send(new AddCameraMessage(Camera));
            SimpleIoc.Default.GetInstance<SettingsViewModel>().SettingsService.LoadCalibration(Camera);
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

        public void DoCalibrateCamera()
        {
            DoToggleCamera(false);
            DoToggleTracking(false);

            // TODO move to view
            //CameraCalibrationService ccs = new CameraCalibrationService(Camera);
            //ccs.ShowDialog(window);
            
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