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
namespace UniMoveStation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class SingleCameraViewModel : ViewModelBase
    {
        private TrackerService _trackerService;

        public string Name
        {
            get;
            set;
        }

        public SingleCameraModel SingleCamera
        {
            get;
            set;
        }

        public TrackerService TrackerService
        {
            get;
            private set;
        }

        public Visibility LabelCameraBackgroundVisibility
        {
            get;
            set;
        }

        public Visibility CLEyeImageControlVisibility
        {
            get;
            set;
        }

        public Visibility TrackerImageVisibility
        {
            get;
            set;
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
        /// Gets the AnnotateCommand.
        /// </summary>
        public RelayCommand<bool> AnnotateCommand
        {
            get;
            private set;
        }

        public void DoAnnotate(bool annotate)
        {
            TrackerService.Annotate = annotate;
        }


        #region [ Contructor ]
        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        [PreferredConstructor]
        public SingleCameraViewModel(int trackerId)
        {
            //Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            TrackerService = new TrackerService(trackerId);
            SingleCamera = new SingleCameraModel();

            SingleCamera.Tracker = TrackerService.tracker;
            Name = "Camera " + trackerId;

            SingleCamera.Moves = new List<UniMoveController>();

            ToggleCameraCommand = new RelayCommand<bool>(DoToggleCamera);
            AnnotateCommand = new RelayCommand<bool>(DoAnnotate);
            EnableTrackingCommand = new RelayCommand<bool>(DoEnableTracking);
            SimpleIoc ioc = (SimpleIoc)ServiceLocator.Current;
            ioc.Register(() => this, Name, true);
        }

        #endregion

        public void toggleTracker(bool enable)
        {
            if (enable)
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        public void initTracker(List<UniMoveController> moves)
        {
            //stop CLEye Camera if enabled
            DoToggleCamera(false);
            //enable all UniMoveControllers for tracking
            //_trackerService.SetTracker(camera.Tracker);
            TrackerService.tracker.StartTracker(SingleCamera.Tracker.id);
            for (int i = 0; i < moves.Count; i++)
            {
                //if (checkBoxList_moves.checkBoxListBoxItems[i].IsChecked)
                //{
                //    camera.Tracker.EnableTracking(moves[i], camera.Tracker.id, colors[i]);
                //}
                //else
                //{
                //    camera.Tracker.controllers.Add(new UniMoveTracker.TrackedController(moves[i]));
                //}
            }

            //start BackgroundWorker updating the image
            //initBackgroundWorker();
        }

        #region [ Private Methods ]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        private void setCoordinates(Vector3 vector)
        {
            //textBox_X.Text = vector.x.ToString();
            //textBox_Y.Text = vector.y.ToString();
            //textBox_R.Text = vector.z.ToString();
        }



        /// <summary>
        /// starts or stops displaying the camera image from the CL Eye Camera
        /// </summary>
        private void DoToggleCamera(bool enable)
        {
            if (enable)
            {
                LabelCameraBackgroundVisibility = Visibility.Hidden;

                //clEyeImageControl.start(camera.Tracker.id);

                //initializedCLEye = true;
                //button_initCamera.Content = "Stop Camera";
            }
            else
            {
                //clEyeImageControl.stop();

                LabelCameraBackgroundVisibility = System.Windows.Visibility.Visible;

                //button_initCamera.Content = "Init Camera";
                //initializedCLEye = false;
            }
        }


        #endregion

        #region [ Misc ]
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            DoToggleCamera(false);
            if (TrackerService.bw != null)
            {
                TrackerService.cancelBackgroundWorker();
            }

            if (SingleCamera.Tracker != null)
            {
                SingleCamera.Tracker.DisableTracking();
            }
        }
        #endregion

        /// <summary>
        /// Gets the EnableTracking.
        /// </summary>
        public RelayCommand<bool> EnableTrackingCommand
        {
            get;
            private set;
        }

        public void DoEnableTracking(bool enabled)
        {
            if (enabled)
            {
                TrackerService.StartTracking();
            }
            else
            {
                TrackerService.StopTracking();
            }
            if (enabled)
            {
                List<UniMoveController> moves = new List<UniMoveController>();
                //moves.AddRange(checkBoxList_moves.moves);
                initTracker(moves);
                //bw.RunWorkerAsync();
            }
            else
            {

            }
        }
    }
}