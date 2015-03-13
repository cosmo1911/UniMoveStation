using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Event;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common.Utils;
using UnityEngine;

namespace UniMoveStation.Business.Service
{
    public class MultipleViewsService
    {
        private CamerasModel _cameras { get; set; }
        public List<ITrackerService> TrackerServices { get; set; }
        private CancellationTokenSource _ctsBundle;
        private Task _bundleTask;

        private CancellationTokenSource _ctsFcp;
        private Task _fcpTask;
        private bool _fcpFinished;

        public MultipleViewsService()
        {

        }

        public void Initialize(CamerasModel cameras, List<ITrackerService> trackerServices)
        {
            _cameras = cameras;
            TrackerServices = trackerServices;

            foreach (CameraModel cameraModel in _cameras.Cameras)
            {
                TrackerServices[_cameras.Cameras.IndexOf(cameraModel)].OnImageReady += TrackerService_OnImageReady;
            }
        }


        private void TrackerService_OnImageReady(object sender, EventArgs e)
        {
            OnImageReadyEventArgs args = (OnImageReadyEventArgs) e;

            IEnumerable<CameraModel> orderedCameras = _cameras.Cameras.OrderBy(camera => camera.Calibration.Index);

            foreach (CameraModel cameraModel1 in orderedCameras)
            {
                foreach (CameraModel cameraModel2 in orderedCameras)
                {
                    if (cameraModel1.Calibration.ObjectPointsProjected == null ||
                        cameraModel2.Calibration.ObjectPointsProjected == null) continue;
                    if (cameraModel1.Calibration.Index == cameraModel2.Calibration.Index) continue;
                    if (cameraModel1.Calibration.Index > 1) continue;

                    IntPtr points1Ptr = CvHelper.CreatePointListPointer(cameraModel1.Calibration.ObjectPointsProjected);
                    IntPtr points2Ptr = CvHelper.CreatePointListPointer(cameraModel2.Calibration.ObjectPointsProjected);

                    Matrix<double> fundamentalMatrix = new Matrix<double>(3, 3);

                    IntPtr fundamentalMatrixPtr = CvInvoke.cvCreateMat(3, 3, MAT_DEPTH.CV_32F);
                    CvInvoke.cvFindFundamentalMat(points1Ptr, points2Ptr, fundamentalMatrix.Ptr, CV_FM.CV_FM_RANSAC, 3,
                        0.99, IntPtr.Zero);

                    Matrix<double> lines1 = new Matrix<double>(8, 3);
                    CvInvoke.cvComputeCorrespondEpilines(points2Ptr, 2, fundamentalMatrix.Ptr, lines1.Ptr);

                    Matrix<double> lines2 = new Matrix<double>(8, 3);
                    CvInvoke.cvComputeCorrespondEpilines(points1Ptr, 1, fundamentalMatrix.Ptr, lines2.Ptr);

                    for (int i = 0; i < cameraModel1.Calibration.ObjectPointsProjected.Length; i++)
                    {
                        {
                            Point[] points = new Point[2]
                            {
                                new Point(0, (int) -(lines2[i, 2]/lines2[i, 1])),
                                new Point(args.Image.Cols,
                                    (int) (-(lines2[i, 2] + lines2[i, 0]*args.Image.Cols)/lines2[i, 1]))
                            };
                            args.Image.DrawPolyline(points, false, new Bgr(255, 255, 0), 1);
                        }

                        //{
                        //    System.Drawing.Point[] points = new System.Drawing.Point[2]
                        //    {
                        //        new System.Drawing.Point(0, (int) -(lines1[i, 2]/lines1[i, 1])),
                        //        new System.Drawing.Point(img.Cols, (int) (-(lines1[i, 2] + lines1[i, 0] * img.Cols) / lines1[i, 1]))
                        //    };
                        //    img.DrawPolyline(points, false, new Bgr(255, 0, 255), 1);
                        //}
                    }
                }
            }
        }

        public void DoFindCorrespondingPoints()
        {
            //List<MotionControllerViewModel> mcvms = SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>().ToList<MotionControllerViewModel>();
            //List<CameraViewModel> scvms = SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>().ToList<CameraViewModel>();
            //IEnumerable<CameraViewModel> orderedScvms = scvms.OrderBy(view => view.Camera.Calibration.Index);

            //foreach (CameraViewModel scvm in orderedScvms)
            //{
            //    foreach (MotionControllerViewModel mcvm in mcvms)
            //    {
            //        Vector3 v3 = mcvm.MotionController.RawPosition[scvm.Camera];
            //        // tracking lost
            //        if (v3.x == 0 && v3.y == 0)
            //        {
            //            // remove already obtained points of the current loop
            //            if (scvm.Camera.Calibration.Index != 0)
            //            {
            //                foreach (CameraViewModel scvm2 in orderedScvms)
            //                {
            //                    int diff = scvm2.Camera.Calibration.CorrespondingPoints.Count - scvm.Camera.Calibration.CorrespondingPoints.Count;
            //                    for (int i = diff - 1; i > -1; i--)
            //                    {
            //                        scvm2.Camera.Calibration.CorrespondingPoints.RemoveAt(scvm.Camera.Calibration.CorrespondingPoints.Count + i);
            //                        Console.WriteLine("removing point " + (scvm.Camera.Calibration.CorrespondingPoints.Count + i));
            //                    }
            //                }
            //            }
            //            return;
            //        }
            //        if (scvm.Camera.Calibration.CorrespondingPoints.Count < 100)
            //        {
            //            scvm.Camera.Calibration.CorrespondingPoints.Add(new PointF(v3.x, v3.y));
            //            Console.WriteLine("saving point " + scvm.Camera.Calibration.CorrespondingPoints.Count);
            //        }
            //        else if (scvm.Camera.Calibration.CorrespondingPoints.Count >= 100)
            //        {
            //            Console.WriteLine("finished fcp");
            //            _fcpFinished = true;
            //            CancelFcpTask();
            //        }
            //    }
            //}
        }

        public void DoBundleAdjust()
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

            if (_cameras.Cameras.Count == 0) return;

            IEnumerable<CameraModel> orderedCameras = _cameras.Cameras.OrderBy(camera => camera.Calibration.Index);
            ObservableCollection<MotionControllerModel> controllers = _cameras.Cameras[0].Controllers;

            if (controllers.Count == 0) return;

            const float radiusCm = (int)((14.0 / Math.PI) * 100) / 200f;
            int cameraCount = _cameras.Cameras.Count;
            int pointCount = 8;
            MCvPoint3D64f[] objectPoints = new MCvPoint3D64f[controllers.Count * pointCount];
            MCvPoint2D64f[][] imagePoints = new MCvPoint2D64f[cameraCount][];
            int[][] visibility = new int[cameraCount][];
            Matrix<double>[] cameraMatrix = new Matrix<double>[cameraCount];
            Matrix<double>[] R = new Matrix<double>[cameraCount];
            Matrix<double>[] T = new Matrix<double>[cameraCount];
            Matrix<double>[] distCoefficients = new Matrix<double>[cameraCount];
            MCvTermCriteria termCrit = new MCvTermCriteria(30, 1.0e-12);

            int visible = 0;

            foreach (CameraModel camera in orderedCameras)
            {
                visibility[camera.Calibration.Index] = new int[controllers.Count * pointCount];
                cameraMatrix[camera.Calibration.Index] = camera.Calibration.IntrinsicParameters.IntrinsicMatrix.Clone();
                distCoefficients[camera.Calibration.Index] = camera.Calibration.IntrinsicParameters.DistortionCoeffs.Clone();

                foreach (MotionControllerModel controller in controllers)
                {
                    float x = controller.RawPosition[camera].x;
                    float y = controller.RawPosition[camera].y;

                    //if (x == 0 && y == 0) return;

                    if (x == 0 && y == 0)
                    {
                        for (int i = 0; i < pointCount; i++)
                        {
                            visibility[camera.Calibration.Index][i + controller.Id * pointCount] = 0;
                        }
                    }
                    else
                    {
                        visible++;
                        for (int i = controller.Id * pointCount; i < pointCount * controllers.Count; i++)
                        {
                            visibility[camera.Calibration.Index][i] = 1; 
                        }

                        double distance = Math.Abs(objectPoints[0].z) -
                                             Math.Abs(controller.WorldPosition[camera].z - radiusCm);

                        if (objectPoints[0].x == 0 || distance > 0)
                        {
                            float wx = controller.WorldPosition[camera].x;
                            float wy = controller.WorldPosition[camera].y;
                            float wz = controller.WorldPosition[camera].z;


                            objectPoints[0] = new MCvPoint3D32f(wx - radiusCm, wy - radiusCm, wz - radiusCm);
                            objectPoints[1] = new MCvPoint3D32f(wx + radiusCm, wy - radiusCm, wz - radiusCm);
                            objectPoints[2] = new MCvPoint3D32f(wx + radiusCm, wy + radiusCm, wz - radiusCm);
                            objectPoints[3] = new MCvPoint3D32f(wx - radiusCm, wy + radiusCm, wz - radiusCm);

                            objectPoints[4] = new MCvPoint3D32f(wx - radiusCm, wy + radiusCm, wz + radiusCm);
                            objectPoints[5] = new MCvPoint3D32f(wx + radiusCm, wy + radiusCm, wz + radiusCm);
                            objectPoints[6] = new MCvPoint3D32f(wx + radiusCm, wy - radiusCm, wz + radiusCm);
                            objectPoints[7] = new MCvPoint3D32f(wx - radiusCm, wy - radiusCm, wz + radiusCm);
                        }

                        //imagePoints[scvm.Camera.Calibration.Index] = Utils.GetImagePoints(mcvm.MotionController.RawPosition[scvm.Camera]);
                        imagePoints[camera.Calibration.Index] = Array.ConvertAll(camera.Calibration.ObjectPointsProjected, CvHelper.PointFtoPoint2D);

                        R[camera.Calibration.Index] = camera.Calibration.RotationToWorld;
                        T[camera.Calibration.Index] = camera.Calibration.TranslationToWorld;

                    }
                } // foreach controller
            } // foreach camera

            // average object points
            //for (int i = 0; i < objectPoints.Length; i++)
            //{
            //    objectPoints[i].x /= visible;
            //    objectPoints[i].y /= visible;
            //    objectPoints[i].z /= visible;
            //}

            // check for calucation error
            for (int i = 0; i < objectPoints.Length; i++)
            {
                if (objectPoints[i].x.ToString().Equals("NaN")) return;
                else if (objectPoints[i].y.ToString().Equals("NaN")) return;
                else if (objectPoints[i].z.ToString().Equals("NaN")) return;
            }

            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            Console.WriteLine("Input: ({0}, {1}, {2})", objectPoints[0].x, objectPoints[0].y, objectPoints[0].z);
            LevMarqSparse.BundleAdjust(objectPoints, imagePoints, visibility, cameraMatrix, R, T, distCoefficients, termCrit);
            Console.WriteLine("Output: ({0}, {1}, {2})\n", objectPoints[0].x, objectPoints[0].y, objectPoints[0].z);
            //sw.Stop();
            //ConsoleService.WriteLine("bundle adjust: " + sw.Elapsed.ToString());

            // check for calucation error
            for (int i = 0; i < objectPoints.Length; i++)
            {
                if (objectPoints[i].x.ToString().Equals("NaN")) return;
                else if (objectPoints[i].y.ToString().Equals("NaN")) return;
                else if (objectPoints[i].z.ToString().Equals("NaN")) return;
            }

            // save changed matrices
            foreach (CameraModel camera in orderedCameras)
            {
                if (visibility[camera.Calibration.Index][0] == 1)
                {
                    camera.Calibration.IntrinsicParameters.IntrinsicMatrix = cameraMatrix[camera.Calibration.Index];
                    camera.Calibration.RotationToWorld = R[camera.Calibration.Index];
                    camera.Calibration.TranslationToWorld = T[camera.Calibration.Index];
                    camera.Calibration.IntrinsicParameters.DistortionCoeffs = distCoefficients[camera.Calibration.Index];
                }
            }

            //float posx = 0, posy = 0, posz = 0; 
            //for (int i = 0; i < objectPoints.Length; i++)
            //{
            //    posx += (float)objectPoints[0].x / objectPoints.Length;
            //    posy += (float)objectPoints[0].y / objectPoints.Length;
            //    posz += (float)objectPoints[0].z / objectPoints.Length;
            //}

            _cameras.Position = new Vector3((float)objectPoints[0].x, (float)objectPoints[0].y, (float)objectPoints[0].z);
        }

        void DoFindFundamentalMatrices()
        {
            //List<MotionControllerViewModel> mcvms = SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>().ToList<MotionControllerViewModel>();
            //List<CameraViewModel> scvms = SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>().ToList<CameraViewModel>();
            //IEnumerable<CameraViewModel> orderedScvms = scvms.OrderBy(view => view.Camera.Calibration.Index);

            //foreach (CameraViewModel scvm in orderedScvms)
            //{
            //    foreach (CameraViewModel scvm2 in orderedScvms)
            //    {
            //        if (scvm.Camera.Calibration.Index == scvm2.Camera.Calibration.Index) continue;
            //        Matrix<double> fundamentalMatrix = FindFundamentalMatrix(scvm.Camera, scvm2.Camera);
            //        //FindHomographyMatrix(scvm.Camera, scvm.Camera);
            //    }
            //}
        }

        Matrix<double> FindFundamentalMatrix(CameraModel cam1, CameraModel cam2)
        {
            IntPtr points1Ptr = CvHelper.CreatePointListPointer(CvHelper.NormalizePoints(cam1.Calibration.ObjectPointsProjected, cam1.Calibration.IntrinsicParameters));
            IntPtr points2Ptr = CvHelper.CreatePointListPointer(CvHelper.NormalizePoints(cam2.Calibration.ObjectPointsProjected, cam1.Calibration.IntrinsicParameters));

            Matrix<double> fundamentalMatrix = new Matrix<double>(3, 3);

            IntPtr fundamentalMatrixPtr = CvInvoke.cvCreateMat(3, 3, MAT_DEPTH.CV_32F);
            CvInvoke.cvFindFundamentalMat(points1Ptr, points2Ptr, fundamentalMatrix.Ptr, CV_FM.CV_FM_RANSAC, 3, 0.99, IntPtr.Zero);

            return fundamentalMatrix;
        }

        void StereoCalibrate()
        {
            MCvPoint3D32f[][] objectPoints;
            PointF[][] imagePoints1;
            PointF[][] imagePoints2;
            MCvTermCriteria termCriteria;

            IntrinsicCameraParameters icp;
            ExtrinsicCameraParameters ecp;
            Matrix<double> fundamentalMatrix = new Matrix<double>(3, 3);
            Matrix<double> essentialMatrix = new Matrix<double>(3, 3);

            //Emgu.CV.CameraCalibration.StereoCalibrate()

        }

        void FindHomographyMatrix(CameraModel cam1, CameraModel cam2)
        {
            PointF[] src = new PointF[4] {
                cam1.Calibration.ObjectPointsProjected[0],
                cam1.Calibration.ObjectPointsProjected[1],
                cam1.Calibration.ObjectPointsProjected[2],
                cam1.Calibration.ObjectPointsProjected[3]
            };

            PointF[] dst = new PointF[4] {
                cam2.Calibration.ObjectPointsProjected[0],
                cam2.Calibration.ObjectPointsProjected[1],
                cam2.Calibration.ObjectPointsProjected[2],
                cam2.Calibration.ObjectPointsProjected[3]
            };

            HomographyMatrix h = CameraCalibration.FindHomography(src, dst, HOMOGRAPHY_METHOD.RANSAC, 3);

            PointF test = cam1.Calibration.ObjectPointsProjected[0];
        }

        public async void StartBundleAdjustTask()
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
                        DoBundleAdjust();
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

        public void CancelBundleTask()
        {
            if (_ctsBundle == null) return;

            _ctsBundle.Cancel();
            _bundleTask.Wait();
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
            if (_ctsFcp != null && _fcpTask.Status != TaskStatus.RanToCompletion)
            {
                _ctsFcp.Cancel();
                _fcpTask.Wait();
            }
        }

        private void ShowFcpDialog()
        {
            // TODO move to view
            //_dialog = new FcpView(window);
            //_dialog.DataContext = this;
            //_owningWindow = window;

            //await _owningWindow.ShowMetroDialogAsync(_dialog);
        }

        public void DoCancelFcp()
        {
            if (_ctsFcp != null)
            {
                _ctsFcp.Cancel();
            }
            Console.WriteLine("canceling fcp");
            //await _dialog.RequestCloseAsync();
        }

        public void DoStartFcp()
        {
            //StartFcpTask();
            //foreach (CameraViewModel cameraViewModel in _cameras)
            //{
            //    cameraViewModel.Camera.Calibration.CorrespondingPoints = new List<PointF>();
            //}
            //Console.WriteLine("starting fcp");
        }

        public void DoSaveFcp()
        {
            //foreach (CameraViewModel cameraModel in _cameras)
            //{
            //    // TODO ioc
            //    //ViewModelLocator.Instance.Settings.DoSaveCalibration(cameraModel);
            //}
            //Console.WriteLine("saving fcp");
            //DoCancelFcp();
        }
    }
}
