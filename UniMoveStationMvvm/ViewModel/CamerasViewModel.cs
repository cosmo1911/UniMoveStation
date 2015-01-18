using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using UniMoveStation.Helper;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class CamerasViewModel : ViewModelBase
    {
        private string _name;
        private bool _annotate;
        private bool _tracking;
        private bool _showImage;

        private RelayCommand<bool> _toggleCameraCommand;
        private RelayCommand<bool> _toggleTrackingCommand;
        private RelayCommand<bool> _toggleAnnotateCommand;
        private RelayCommand<ListBox> _applySelectionCommand;
        private RelayCommand<ListBox> _cancelSelectionCommand;

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public bool Annotate
        {
            get { return _annotate; }
            set { Set(() => Annotate, ref _annotate, value); }
        }

        public bool Tracking
        {
            get { return _tracking; }
            set { Set(() => Tracking, ref _tracking, value); }
        }

        public bool ShowImage
        {
            get { return _showImage; }
            set { Set(() => ShowImage, ref _showImage, value); }
        }

        public ObservableCollection<SingleCameraViewModel> Cameras
        {
            get;
            private set;
        }

        public ObservableCollection<MotionControllerModel> Controllers
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the AllCamerasViewModel class.
        /// </summary>
        public CamerasViewModel()
        {

            Name = "all";
            Cameras = new ObservableCollection<SingleCameraViewModel>();
            Controllers = new ObservableCollection<MotionControllerModel>();
            Refresh();

            Messenger.Default.Register<AddCameraMessage>(this, message =>
            {
                Cameras.Add(SimpleIoc.Default.GetInstance<SingleCameraViewModel>(message.Camera.GUID));
            });

            Messenger.Default.Register<RemoveCameraMessage>(this, message =>
            {
                if(SimpleIoc.Default.ContainsCreated<SingleCameraViewModel>(message.Camera.GUID))
                {
                    Cameras.Remove(SimpleIoc.Default.GetInstance<SingleCameraViewModel>(message.Camera.GUID));
                }
            });

            Messenger.Default.Register<AddMotionControllerMessage>(this,
                message =>
                {
                    Controllers.Add(message.MotionController);
                });

            Messenger.Default.Register<RemoveMotionControllerMessage>(this,
                message =>
                {
                    Controllers.Remove(message.MotionController);
                });

            if(IsInDesignMode)
            {
                Cameras.Add(new SingleCameraViewModel());
                Cameras.Add(new SingleCameraViewModel());
                Cameras.Add(new SingleCameraViewModel());
                Controllers.Add(new MotionControllerModel());
                Controllers.Add(new MotionControllerModel());
                Controllers.Add(new MotionControllerModel());
            }
            else
            {
                if (SimpleIoc.Default.GetInstance<SettingsViewModel>().Settings.LoadCamerasOnStartUp)
                {
                    AddAvailableCameras();
                }
            }
        }

        public void AddAvailableCameras()
        {
            int count = CLEyeMulticam.CLEyeCameraDevice.CameraCount;

            for(int i = 0; i < count; i++)
            {
                SingleCameraModel camera = new SingleCameraModel();
                IConsoleService consoleService = new ConsoleService();
                ITrackerService trackerService = new TrackerService(consoleService);
                ICameraService cameraService = new CLEyeService(consoleService);
                camera.TrackerId = i;
                camera.Name = "Camera " + i;
                cameraService.Initialize(camera);
                new SingleCameraViewModel(camera, trackerService, cameraService, consoleService);
            }
            Refresh();
        }

        public void Refresh()
        {
            Cameras.Clear();
            foreach (SingleCameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>())
            {
                Cameras.Add(scvm);
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
        /// Gets the ApplySelectionCommand.
        /// </summary>
        public RelayCommand<ListBox> ApplySelectionCommand
        {
            get
            {
                return _applySelectionCommand
                    ?? (_applySelectionCommand = new RelayCommand<ListBox>(
                        DoApplySelection,
                        (box) => Cameras.Count > 0 && Cameras[0].Camera.Controllers.Count > 0));
            }
        }

        /// <summary>
        /// Gets the CancelSelectionCommand.
        /// </summary>
        public RelayCommand<ListBox> CancelSelectionCommand
        {
            get
            {
                return _cancelSelectionCommand
                    ?? (_cancelSelectionCommand = new RelayCommand<ListBox>(
                        DoCancelSelection,
                        (box) => Cameras.Count > 0 && Cameras[0].Camera.Controllers.Count > 0));
            }
        }
        #endregion

        #region Command Exeuctions
        public void DoToggleAnnotate(bool annotate)
        {
            foreach(SingleCameraViewModel scvm in Cameras)
            {
                scvm.DoToggleAnnotate(annotate);
            }
            Annotate = annotate;
        }

        public void DoToggleCamera(bool enabled)
        {
            foreach (SingleCameraViewModel scvm in Cameras)
            {
                scvm.DoToggleCamera(enabled);
            }
            ShowImage = enabled;
        }

        public void DoToggleTracking(bool enabled)
        {
            foreach (SingleCameraViewModel scvm in Cameras)
            {
                scvm.DoToggleTracking(enabled);
            }
            Tracking = enabled;
        }

        public void DoApplySelection(ListBox listBox)
        {
            foreach (SingleCameraViewModel scvm in Cameras)
            {
                scvm.DoApplySelection(listBox);
            }
        } // DoApplySelection

        public void DoCancelSelection(ListBox listBox)
        {
            foreach (SingleCameraViewModel scvm in Cameras)
            {
                scvm.DoCancelSelection(listBox);
            }
        }
        #endregion
    } // CamerasViewModel
} // namespace