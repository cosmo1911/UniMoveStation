using GalaSoft.MvvmLight;
using Emgu.CV.Structure;
using UniMoveStation.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UniMove;
using UnityEngine;
using GalaSoft.MvvmLight.CommandWpf;
using UniMoveStation.Model;
using System.Windows.Media;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using UniMoveStation.Service;
using UniMoveStation.View;
using UniMoveStation.Design;

namespace UniMoveStation.ViewModel
{
    public class SingleCameraViewModel : ViewModelBase
    {
        #region Member
        private static int COUNTER = -1;
        public SingleCameraModel Camera
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
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        [PreferredConstructor]
        public SingleCameraViewModel(SingleCameraModel camera, ITrackerService trackerService, ICameraService cameraService)
        {
            TrackerService = trackerService;
            CameraService = cameraService;
            Camera = camera;

            CameraService.Initialize(Camera);
            TrackerService.Initialize(Camera);

            ToggleCameraCommand = new RelayCommand<bool>(DoToggleCamera);
            ToggleAnnotateCommand = new RelayCommand<bool>(DoToggleAnnotate);
            ToggleTrackingCommand = new RelayCommand<bool>(DoToggleTracking);

            SimpleIoc ioc = (SimpleIoc)ServiceLocator.Current;
            ioc.Register(() => this, Camera.GUID, true);
        }

        /// <summary>
        /// for design time purposes only
        /// </summary>
        public SingleCameraViewModel() : this(new SingleCameraModel(), new DesignTrackerService(),  new DesignCLEyeService())
        {
#if DEBUG
            if (IsInDesignMode)
            {
                Camera.TrackerId = ++COUNTER;
                Camera.Name = "Design " + Camera.TrackerId;
                CameraService.Initialize(Camera);
            }
#endif
        }
        #endregion

        #region Properties
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

        #region Relay Commands
        /// <summary>
        /// Gets the AnnotateCommand.
        /// </summary>
        public RelayCommand<bool> ToggleAnnotateCommand
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public RelayCommand<bool> ToggleCameraCommand
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the EnableTracking.
        /// </summary>
        public RelayCommand<bool> ToggleTrackingCommand
        {
            get;
            private set;
        }
        #endregion

        #region Command Executions
        public void DoToggleAnnotate(bool annotate)
        {
            Camera.Annotate = annotate;
        }

        private void DoToggleCamera(bool enabled)
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
        }
        #endregion

        #region Misc
        #endregion
    } // SingleCameraViewModel
} // Namespace