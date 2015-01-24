using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using UniMoveStation.Helper;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UniMoveStation.ViewModel.Flyout;
using MahApps.Metro.Controls;
using System;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.CvEnum;
using UnityEngine;
using Emgu.CV.Structure;
using System.Threading;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.View;
using System.Collections;

namespace UniMoveStation.ViewModel
{

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


        private RelayCommand _bundleAdjustCommand;
        private CancellationTokenSource _ctsBundle;
        private Task _bundleTask;

        private RelayCommand<MetroWindow> _fcpCommand;
        private CancellationTokenSource _ctsFcp;
        private Task _fcpTask;
        private bool _fcpFinished;

        private BaseMetroDialog _dialog;
        private MetroWindow _owningWindow;
        private RelayCommand _cancelCommand;
        private RelayCommand _startCommand;
        private RelayCommand _saveCommand;

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

        /// <summary>
        /// Gets the FindCorrespondingPointsCommand.
        /// </summary>
        public RelayCommand<MetroWindow> FindCorrespondingPointsCommand
        {
            get
            {
                return _fcpCommand
                    ?? (_fcpCommand = new RelayCommand<MetroWindow>(ShowFcpDialog));
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

        /// <summary>
        /// Gets the ButtonCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(DoCancel));
            }
        }

        /// <summary>
        /// Gets the StartCommand.
        /// </summary>
        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand
                    ?? (_startCommand = new RelayCommand(DoStart));
            }
        }

        /// <summary>
        /// Gets the SaveCommand.
        /// </summary>
        public RelayCommand SaveCommand
        {
            get
            {
                return _saveCommand
                    ?? (_saveCommand = new RelayCommand(
                        DoSave,
                        () => _fcpFinished));
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
                    ?? (_bundleAdjustCommand = new RelayCommand(BundleAdjust));
            }
        }

        public void DoFindCorrespondingPoints()
        {
            List<MotionControllerViewModel> mcvms = SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>().ToList<MotionControllerViewModel>();
            List<SingleCameraViewModel> scvms = SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>().ToList<SingleCameraViewModel>();
            IEnumerable<SingleCameraViewModel> orderedScvms = scvms.OrderBy(view => view.Camera.Calibration.Position);

            foreach (SingleCameraViewModel scvm in orderedScvms)
            {
                foreach (MotionControllerViewModel mcvm in mcvms)
                {
                    Vector3 v3 = mcvm.MotionController.RawPosition[scvm.Camera];
                    if (v3.x == 0 && v3.y == 0)
                    {
                        if(scvm.Camera.Calibration.Position != 0)
                        {
                            foreach (SingleCameraViewModel scvm2 in orderedScvms)
                            {
                                int diff = scvm2.Camera.Calibration.CorrespondingPoints.Count - scvm.Camera.Calibration.CorrespondingPoints.Count;
                                for (int i = diff - 1; i > -1; i--)
                                {
                                    scvm2.Camera.Calibration.CorrespondingPoints.RemoveAt(scvm.Camera.Calibration.CorrespondingPoints.Count + i);
                                    Console.WriteLine("removing point " + (scvm.Camera.Calibration.CorrespondingPoints.Count + i));
                                }
                            }
                        }
                        return;
                    }
                    else if (scvm.Camera.Calibration.CorrespondingPoints.Count < 100)
                    {
                        scvm.Camera.Calibration.CorrespondingPoints.Add(new PointF(v3.x, v3.y));
                        Console.WriteLine("saving point " + scvm.Camera.Calibration.CorrespondingPoints.Count);
                    }
                    else if (scvm.Camera.Calibration.CorrespondingPoints.Count >= 100)
                    {
                        Console.WriteLine("finished fcp");
                        _fcpFinished = true;
                        CancelFcpTask();
                    }
                }
            }
        }
        #endregion

        public void BundleAdjust()
        {
            // N = cams
            // M = points
            //public static void BundleAdjust(MCvPoint3D64f[M] points,              // Positions of points in global coordinate system (input and output), values will be modified by bundle adjustment
            //                                MCvPoint2D64f[M][N] imagePoints,      // Projections of 3d points for every camera
            //                                int[M][N] visibility,                 // Visibility of 3d points for every camera
            //                                Matrix<double>[N] cameraMatrix,       // Intrinsic matrices of all cameras (input and output), values will be modified by bundle adjustment
            //                                Matrix<double>[N] R,                  // rotation matrices of all cameras (input and output), values will be modified by bundle adjustment
            //                                Matrix<double>[N] T,                  // translation vector of all cameras (input and output), values will be modified by bundle adjustment
            //                                Matrix<double>[N] distCoefficients,   // distortion coefficients of all cameras (input and output), values will be modified by bundle adjustment
            //                                MCvTermCriteria termCrit)             // Termination criteria, a reasonable value will be (30, 1.0e-12)

            List<MotionControllerViewModel> mcvms = SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>().ToList<MotionControllerViewModel>();
            List<SingleCameraViewModel> scvms = SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>().ToList<SingleCameraViewModel>();
            IEnumerable<SingleCameraViewModel> orderedScvms = scvms.OrderBy(view => view.Camera.Calibration.Position);

            int cameraCount = CLEyeMulticam.CLEyeCameraDevice.CameraCount;
            MCvPoint3D64f[] points = new MCvPoint3D64f[mcvms.Count];
            MCvPoint2D64f[][] imagePoints = new MCvPoint2D64f[cameraCount][];
            int[][] visibility = new int[cameraCount][];
            Matrix<double>[] cameraMatrix = new Matrix<double>[cameraCount];
            Matrix<double>[] R = new Matrix<double>[cameraCount];
            Matrix<double>[] T = new Matrix<double>[cameraCount];
            Matrix<double>[] distCoefficients = new Matrix<double>[cameraCount];
            MCvTermCriteria termCrit = new MCvTermCriteria(30, 1e-12);

            if (mcvms.Count == 0) return;

            foreach (SingleCameraViewModel scvm in orderedScvms)
            {
                imagePoints[scvm.Camera.Calibration.Position] = new MCvPoint2D64f[mcvms.Count];
                visibility[scvm.Camera.Calibration.Position] = new int[mcvms.Count];
                cameraMatrix[scvm.Camera.Calibration.Position] = scvm.Camera.Calibration.IntrinsicParameters.IntrinsicMatrix;
                R[scvm.Camera.Calibration.Position] = scvm.Camera.Calibration.RotationMatrix;
                T[scvm.Camera.Calibration.Position] = scvm.Camera.Calibration.TranslationVector;
                distCoefficients[scvm.Camera.Calibration.Position] = scvm.Camera.Calibration.IntrinsicParameters.DistortionCoeffs;

                foreach (MotionControllerViewModel mcvm in mcvms)
                {

                    double x = mcvm.MotionController.RawPosition[scvm.Camera].x;
                    double y = mcvm.MotionController.RawPosition[scvm.Camera].y;
                    double z = mcvm.MotionController.RawPosition[scvm.Camera].z;
                    if (x == 0 && y == 0 && z == 0) return;

                    MCvPoint3D32f[] objPoints = new MCvPoint3D32f[] { new MCvPoint3D32f(50, 0, 100) };
                    System.Drawing.PointF[] imgPoints = new System.Drawing.PointF[] { new System.Drawing.PointF((float)x, (float)y) };
                    ExtrinsicCameraParameters ex = Emgu.CV.CameraCalibration.FindExtrinsicCameraParams2(objPoints, imgPoints, scvm.Camera.Calibration.IntrinsicParameters);


                    imagePoints[scvm.Camera.Calibration.Position][mcvm.MotionController.Id] = new MCvPoint2D64f(x, y);

                    if (x == 0 && y == 0) visibility[scvm.Camera.Calibration.Position][mcvm.MotionController.Id] = 0;
                    else
                    {
                        visibility[scvm.Camera.Calibration.Position][mcvm.MotionController.Id] = 1;

                        // estimate (x, y, z) using two neighbouring / orthogonal cameras
                        if (scvm.Camera.Calibration.Position == 0)
                        {
                            points[mcvm.MotionController.Id] = new MCvPoint3D64f(0, 0, z);
                        }
                        else if (visibility[scvm.Camera.Calibration.Position - 1][mcvm.MotionController.Id] == 0)
                        {
                            points[mcvm.MotionController.Id] = new MCvPoint3D64f(0, 0, z);
                        }
                        else if (visibility[scvm.Camera.Calibration.Position - 1][mcvm.MotionController.Id] == 1)
                        {
                            if (points[mcvm.MotionController.Id].x == 0) points[mcvm.MotionController.Id].x = z;
                        }
                    }
                } // foreach controller
            } // foreach camera

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            Console.WriteLine("Input: ({0}, {1}, {2})", points[0].x, points[0].y, points[0].z);
            Emgu.CV.LevMarqSparse.BundleAdjust(points, imagePoints, visibility, cameraMatrix, R, T, distCoefficients, termCrit);
            Console.WriteLine("Output: ({0}, {1}, {2})\n", points[0].x, points[0].y, points[0].z);
            //sw.Stop();
            //ConsoleService.WriteLine("bundle adjust: " + sw.Elapsed.ToString());

            foreach (SingleCameraViewModel scvm in scvms)
            {
                scvm.Camera.Calibration.IntrinsicParameters.IntrinsicMatrix = cameraMatrix[scvm.Camera.Calibration.Position];
                scvm.Camera.Calibration.RotationMatrix = R[scvm.Camera.Calibration.Position];
                scvm.Camera.Calibration.TranslationVector = T[scvm.Camera.Calibration.Position];
                scvm.Camera.Calibration.IntrinsicParameters.DistortionCoeffs = distCoefficients[scvm.Camera.Calibration.Position];
                scvm.Camera.Calibration.Point = new Vector3((float)points[0].x, (float)points[0].y, (float)points[0].z);
            }
        }

        void FindFundamentalMatrix(IList<PointF> points1, IList<PointF> points2)
        {

            IntPtr points1Ptr = CreatePointListPointer(points1);
            IntPtr points2Ptr = CreatePointListPointer(points2);
            IntPtr statusPtr = CvInvoke.cvCreateMat(1, 100, MAT_DEPTH.CV_8U);
            IntPtr fundamentalMatrixPtr = CvInvoke.cvCreateMat(3, 3, MAT_DEPTH.CV_32F);
            Emgu.CV.CvInvoke.cvFindFundamentalMat(points1Ptr, points2Ptr, fundamentalMatrixPtr, CV_FM.CV_FM_RANSAC_ONLY, 3, 0.99, statusPtr);
            Matrix<double> fundamentalMatrix = new Matrix<double>(3, 3, fundamentalMatrixPtr);

        }

        public IntPtr CreatePointListPointer(IList<PointF> points)
        {
            IntPtr result = CvInvoke.cvCreateMat(points.Count, 2, MAT_DEPTH.CV_32F);

            for (int i = 0; i < points.Count; i++)
            {
                double currentX = points[i].X;
                double currentY = points[i].Y;
                CvInvoke.cvSet2D(result, i, 0, new MCvScalar(currentX));
                CvInvoke.cvSet2D(result, i, 1, new MCvScalar(currentY));
            }

            return result;
        }

        private async void StartBundleAdjustTask()
        {
            if (_bundleTask != null && _bundleTask.Status == TaskStatus.Running)
            {
                CancelBundleTask();
                return;
            }

            _ctsBundle = new CancellationTokenSource();
            CancellationToken token = _ctsBundle.Token;
            try
            {
                _bundleTask = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        BundleAdjust();
                    }
                });
                await _bundleTask;
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

        private void CancelBundleTask()
        {
            if (_ctsBundle != null)
            {
                _ctsBundle.Cancel();
                _bundleTask.Wait();
            }
        }

        private async void StartFcpTask()
        {
            if (_fcpTask != null && _fcpTask.Status == TaskStatus.Running)
            {
                CancelFcpTask();
                return;
            }

            _ctsFcp = new CancellationTokenSource();
            CancellationToken token = _ctsFcp.Token;
            try
            {
                _fcpTask = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        DoFindCorrespondingPoints();
                        Thread.Sleep(300);
                    }
                });
                await _fcpTask;
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

        private void CancelFcpTask()
        {
            if (_ctsFcp != null)
            {
                _ctsFcp.Cancel();
                _fcpTask.Wait();
            }
        }

        private async void ShowFcpDialog(MetroWindow window)
        {
            _dialog = new FcpView(window);
            _dialog.DataContext = this;
            _owningWindow = window;

            await _owningWindow.ShowMetroDialogAsync(_dialog);
        }

        public async void DoCancel()
        {
            if (_ctsFcp != null)
            {
                _ctsFcp.Cancel();
            }
            Console.WriteLine("canceling fcp");
            await _dialog.RequestCloseAsync();
        }

        public void DoStart()
        {
            StartFcpTask();
            foreach (SingleCameraViewModel scvm in Cameras)
            {
                scvm.Camera.Calibration.CorrespondingPoints = new List<PointF>();
            }
            Console.WriteLine("starting fcp");
        }

        public void DoSave()
        {
            foreach(SingleCameraViewModel scvm in Cameras)
            {
                ViewModelLocator.Instance.Settings.DoSaveCalibration(scvm.Camera);
            }
            Console.WriteLine("saving fcp");
            DoCancel();
        }
    } // CamerasViewModel
} // namespace