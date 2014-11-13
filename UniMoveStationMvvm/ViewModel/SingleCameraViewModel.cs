using GalaSoft.MvvmLight;
using CLEyeMulticam;
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

namespace UniMoveStation.ViewModel
{
    public class SingleCameraViewModel : ViewModelBase
    {
        #region Member
        public SingleCameraModel Camera
        {
            get;
            set;
        }

        public ITrackerService TrackerService
        {
            get;
            set;
        }

        public ICLEyeService CLEyeService
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        [PreferredConstructor]
        public SingleCameraViewModel(int trackerId)
        {
            Camera = new SingleCameraModel();

            TrackerService = new TrackerService(Camera);
            CLEyeService = new CLEyeService();

            Camera.Id = trackerId;
            Camera.Name = "Camera " + trackerId;

            Camera.Controllers = new List<UniMoveController>();

            ToggleCameraCommand = new RelayCommand<bool>(DoToggleCamera);
            ToggleAnnotateCommand = new RelayCommand<bool>(DoToggleAnnotate);
            ToggleTrackingCommand = new RelayCommand<bool>(DoToggleTracking);
            SimpleIoc ioc = (SimpleIoc)ServiceLocator.Current;
            ioc.Register(() => this, Camera.Name, true);

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

        /// <summary>
        /// starts or stops displaying the camera image from the CL Eye Camera
        /// </summary>
        private void DoToggleCamera(bool enabled)
        {
            if (enabled)
            {
                if(TrackerService.Enabled)
                {
                    CLEyeImageControlVisibility = Visibility.Hidden;
                    TrackerImageVisibility = Visibility.Visible;
                }
                else if(!CLEyeService.Enabled)
                {
                    CLEyeImageControlVisibility = Visibility.Visible;
                    TrackerImageVisibility = Visibility.Hidden;

                    CLEyeService.Start(Camera);
                }
            }
            else
            {
                if (CLEyeService.Enabled) Camera.ShowImage = CLEyeService.Stop();
                else if (TrackerService.Enabled) Camera.ShowImage = false;

                CLEyeImageControlVisibility = Visibility.Hidden;
                TrackerImageVisibility = Visibility.Hidden;
            }
        }

        public void DoToggleTracking(bool enabled)
        {
            if (enabled)
            {
                if (CLEyeService.Enabled)
                {
                    CLEyeService.Stop();
                    CLEyeImageControlVisibility = Visibility.Hidden;
                }
                Camera.Tracking = TrackerService.Start();
                TrackerImageVisibility = Visibility.Visible;
            }
            else
            {
                Camera.Tracking = TrackerService.Stop();
                TrackerImageVisibility = Visibility.Hidden;
                if (Camera.ShowImage)
                {
                    CLEyeImageControlVisibility = Visibility.Visible;
                    CLEyeService.Start(Camera);
                }
            }
        }
        #endregion

        #region Misc
        #endregion
    } // SingleCameraViewModel
} // Namespace