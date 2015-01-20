using GalaSoft.MvvmLight;
using Emgu.CV.Structure;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UnityEngine;
using GalaSoft.MvvmLight.CommandWpf;
using UniMoveStation.Model;
using System.Windows.Media;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using UniMoveStation.Service;
using UniMoveStation.View;
using UniMoveStation.Design;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Helper;
using System.ComponentModel;
using MahApps.Metro.Controls;
using UniMoveStation.ViewModel.Flyout;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace UniMoveStation.ViewModel
{
    public class SingleCameraViewModel : ViewModelBase
    {
        #region Member
        private RelayCommand<MetroWindow> _calibrateCameraCommand;
        private RelayCommand<bool> _toggleCameraCommand;
        private RelayCommand<bool> _toggleTrackingCommand;
        private RelayCommand<bool> _toggleAnnotateCommand;
        private RelayCommand<ListBox> _applySelectionCommand;
        private RelayCommand<ListBox> _cancelSelectionCommand;
        private RelayCommand _bundleAdjustCommand;
        private CancellationTokenSource _ctsUpdate;
        private Task _updateTask;

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

        public IConsoleService ConsoleService
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
        public SingleCameraViewModel(
            SingleCameraModel camera, 
            ITrackerService trackerService, 
            ICameraService cameraService, 
            IConsoleService consoleService)
        {
            TrackerService = trackerService;
            CameraService = cameraService;
            ConsoleService = consoleService;
            Camera = camera;

            CameraService.Initialize(Camera);
            TrackerService.Initialize(Camera);

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
            Messenger.Default.Send<AddCameraMessage>(new AddCameraMessage(Camera));
            SimpleIoc.Default.GetInstance<SettingsViewModel>().LoadCalibration(Camera);
        }

        /// <summary>
        /// for design time purposes only
        /// </summary>
        public SingleCameraViewModel() : this(
            new SingleCameraModel(), 
            new DesignTrackerService(),  
            new DesignCLEyeService(), 
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
        /// Gets the CalibrateCameraCommand.
        /// </summary>
        public RelayCommand<MetroWindow> CalibrateCameraCommand
        {
            get
            {
                return _calibrateCameraCommand
                    ?? (_calibrateCameraCommand = new RelayCommand<MetroWindow>(
                        DoCalibrateCamera, 
                        (window) => Camera.TrackerId == 0));
            }
        }

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
                        (box) => Camera.Controllers.Count > 0));
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
                        (box) => Camera.Controllers.Count > 0));
            }
        }

        /// <summary>
        /// Gets the BundleAdjustCommand.
        /// </summary>
        public RelayCommand BundleAdjustCommand
        {
            get
            {
                return _bundleAdjustCommand
                    ?? (_bundleAdjustCommand = new RelayCommand(StartUpdateTask));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleAnnotate(bool annotate)
        {
            Camera.Annotate = annotate;
            ConsoleService.WriteLine("Annotate: " + annotate);
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

        public void DoApplySelection(ListBox listBox)
        {
            int index = -1;
            foreach(MotionControllerModel mc in listBox.Items)
            {
                index++;
                ListBoxItem listBoxItem = (ListBoxItem) listBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox) dataTemplate.FindName("CheckBox", contentPresenter);
                bool isChecked = (bool) checkBox.IsChecked;
                if (isChecked)
                {
                    mc.Tracking[Camera] = true;
                }
                else
                {
                    mc.Tracking[Camera] = false;
                }
                ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, isChecked));
            }
        } // DoApplySelection

        public void DoCancelSelection(ListBox listBox)
        {
            int index = -1;
            foreach (MotionControllerModel mc in listBox.Items)
            {
                index++;
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                checkBox.IsChecked = mc.Tracking[Camera];
                ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
            }
        }

        public void DoCalibrateCamera(MetroWindow window)
        {
            DoToggleCamera(false);
            DoToggleTracking(false);

            CameraCalibrationService ccs = new CameraCalibrationService(Camera);
            ccs.ShowDialog(window);
            
        }
        #endregion

        #region Misc
        public override void Cleanup()
        {
            TrackerService.Destroy();
            CameraService.Destroy();
            Messenger.Default.Send<RemoveCameraMessage>(new RemoveCameraMessage(Camera));
            SimpleIoc.Default.Unregister<SingleCameraViewModel>(Camera.GUID);
            base.Cleanup();
        }

        /// <summary>
        /// http://msdn.microsoft.com/en-us/library/bb613579.aspx
        /// </summary>
        /// <typeparam name="childItem"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        public void BundleAdjust()
        {
            // N = cams
            // M = points
            //public static void BundleAdjust(MCvPoint3D64f[M] points,               // Positions of points in global coordinate system (input and output), values will be modified by bundle adjustment
            //                                MCvPoint2D64f[M][N] imagePoints,        // Projections of 3d points for every camera
            //                                int[M][N] visibility,                   // Visibility of 3d points for every camera
            //                                Matrix<double>[N] cameraMatrix,        // Intrinsic matrices of all cameras (input and output), values will be modified by bundle adjustment
            //                                Matrix<double>[N] R,                   // rotation matrices of all cameras (input and output), values will be modified by bundle adjustment
            //                                Matrix<double>[N] T,                   // translation vector of all cameras (input and output), values will be modified by bundle adjustment
            //                                Matrix<double>[N] distCoeffcients,     // distortion coefficients of all cameras (input and output), values will be modified by bundle adjustment
            //                                MCvTermCriteria termCrit)             // Termination criteria, a reasonable value will be (30, 1.0e-12)

            List<MotionControllerViewModel> mcvms = SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>().ToList<MotionControllerViewModel>();
            List<SingleCameraViewModel> scvms = SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>().ToList<SingleCameraViewModel>();

            int cameraCount = CLEyeMulticam.CLEyeCameraDevice.CameraCount;
            MCvPoint3D64f[] points = new MCvPoint3D64f[mcvms.Count];
            MCvPoint2D64f[][] imagePoints = new MCvPoint2D64f[cameraCount][];
            int[][] visibility = new int[cameraCount][];
            Matrix<double>[] cameraMatrix = new Matrix<double>[cameraCount];
            Matrix<double>[] R = new Matrix<double>[cameraCount];
            Matrix<double>[] T = new Matrix<double>[cameraCount];
            Matrix<double>[] distCoefficients = new Matrix<double>[cameraCount];
            MCvTermCriteria termCrit = new MCvTermCriteria(30, 0.000000000001);

            if (mcvms.Count == 0) return;

            foreach (SingleCameraViewModel scvm in scvms)
            {
                //switch (scvm.Camera.TrackerId)
                //{
                //    case 0:
                //        scvm.Camera.Calibration.TranslationVector = GetTranslationVector(0, 0, 0);
                //        scvm.Camera.Calibration.RotationMatrix = GetYRotationMatrix(0);
                //        break;
                //    case 1:
                //        scvm.Camera.Calibration.TranslationVector = GetTranslationVector(200, 0, 0);
                //        scvm.Camera.Calibration.RotationMatrix = GetYRotationMatrix(90);
                //        break;
                //    case 2:
                //        scvm.Camera.Calibration.TranslationVector = GetTranslationVector(200, 0, 270);
                //        scvm.Camera.Calibration.RotationMatrix = GetYRotationMatrix(180);
                //        break;
                //    case 3:
                //        scvm.Camera.Calibration.TranslationVector = GetTranslationVector(0, 0, 270);
                //        scvm.Camera.Calibration.RotationMatrix = GetYRotationMatrix(270);
                //        break;
                //}
                //SimpleIoc.Default.GetInstance<SettingsViewModel>().DoSaveCalibration(scvm.Camera);
                imagePoints[scvm.Camera.TrackerId] = new MCvPoint2D64f[mcvms.Count];
                visibility[scvm.Camera.TrackerId] = new int[mcvms.Count];
                cameraMatrix[scvm.Camera.TrackerId] = scvm.Camera.Calibration.IntrinsicParameters.IntrinsicMatrix;
                R[scvm.Camera.TrackerId] = scvm.Camera.Calibration.RotationMatrix;
                T[scvm.Camera.TrackerId] = scvm.Camera.Calibration.TranslationVector;
                distCoefficients[scvm.Camera.TrackerId] = scvm.Camera.Calibration.IntrinsicParameters.DistortionCoeffs;

                foreach (MotionControllerViewModel mcvm in mcvms)
                {

                    double x = mcvm.MotionController.RawPosition[scvm.Camera].x;
                    double y = mcvm.MotionController.RawPosition[scvm.Camera].y;
                    double z = mcvm.MotionController.RawPosition[scvm.Camera].z;
                    imagePoints[scvm.Camera.TrackerId][mcvm.MotionController.Id] = new MCvPoint2D64f(x, y);

                    if (x == 0 && y == 0) visibility[scvm.Camera.TrackerId][mcvm.MotionController.Id] = 0;
                    else
                    {
                        visibility[scvm.Camera.TrackerId][mcvm.MotionController.Id] = 1;

                        if (scvm.Camera.TrackerId == 0)
                        {
                            points[mcvm.MotionController.Id] = new MCvPoint3D64f(0, 0, z);
                        }
                        else if(visibility[scvm.Camera.TrackerId - 1][mcvm.MotionController.Id] == 0)
                        {
                            points[mcvm.MotionController.Id] = new MCvPoint3D64f(0, 0, z);
                        }
                        else if (visibility[scvm.Camera.TrackerId - 1][mcvm.MotionController.Id] == 1)
                        {
                            if (points[mcvm.MotionController.Id].x == 0) points[mcvm.MotionController.Id].x = z;
                        }
                    }
                }
            }
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            Emgu.CV.LevMarqSparse.BundleAdjust(points, imagePoints, visibility, cameraMatrix, R, T, distCoefficients, termCrit);
            //sw.Stop();
            //ConsoleService.WriteLine("bundle adjust: " + sw.Elapsed.ToString());

            foreach (SingleCameraViewModel scvm in scvms)
            {
                scvm.Camera.Calibration.IntrinsicParameters.IntrinsicMatrix = cameraMatrix[scvm.Camera.TrackerId];
                scvm.Camera.Calibration.RotationMatrix = R[scvm.Camera.TrackerId];
                scvm.Camera.Calibration.TranslationVector = T[scvm.Camera.TrackerId];
                scvm.Camera.Calibration.IntrinsicParameters.DistortionCoeffs = distCoefficients[scvm.Camera.TrackerId];
                scvm.Camera.Calibration.Point = new Vector3((float)points[0].x, (float)points[0].y, (float) points[0].z);
            }
        }

        Matrix<double> GetTranslationVector(double x, double y, double z)
        {
            double[,] data = new double[3, 1];

            data[0, 0] = x;
            data[1, 0] = y;
            data[2, 0] = z;

            return new Matrix<double>(data);
        }

        Matrix<double> GetYRotationMatrix(float angle)
        {
            Matrix<double> rotationMatrix = new Matrix<double>(3, 3);

            rotationMatrix[0, 0] = Mathf.Cos(angle);
            rotationMatrix[0, 1] = 0;
            rotationMatrix[0, 2] = Mathf.Sin(angle);

            rotationMatrix[1, 0] = 0;
            rotationMatrix[1, 1] = 1;
            rotationMatrix[1, 2] = 0;

            rotationMatrix[2, 0] = -Mathf.Sin(angle);
            rotationMatrix[2, 1] = 0;
            rotationMatrix[2, 2] = Mathf.Cos(angle);

            return rotationMatrix;
        }

        private async void StartUpdateTask()
        {
            if (_updateTask != null && _updateTask.Status == TaskStatus.Running)
            {
                CancelUpdateTask();
                return;
            }

            _ctsUpdate = new CancellationTokenSource();
            CancellationToken token = _ctsUpdate.Token;
            try
            {
                _updateTask = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        BundleAdjust();
                    }
                });
                await _updateTask;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void CancelUpdateTask()
        {
            if (_ctsUpdate != null)
            {
                _ctsUpdate.Cancel();
                _updateTask.Wait();
            }
        }

        #endregion
    } // SingleCameraViewModel
} // Namespace