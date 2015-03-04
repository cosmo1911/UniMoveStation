using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using UniMoveStation.Model;
using UniMoveStation.SharpMove;
using UniMoveStation.Utilities;
using UniMoveStation.ViewModel;
using UnityEngine;

namespace UniMoveStation.Service
{
    public class TrackerService : DependencyObject, ITrackerService
    {
        #region Member
        private IConsoleService ConsoleService
        {
            get;
            set;
        }

        private CancellationTokenSource _ctsUpdate;
        private SingleCameraModel _camera;
        private Task _updateTask;
        #endregion

        #region Constructor
        public TrackerService(IConsoleService consoleService)
        {
            ConsoleService = consoleService;
        }
        #endregion

        #region Update Task
        private async void StartUpdateTask()
        {
            if (_updateTask != null && _updateTask.Status == TaskStatus.Running) return;

            _ctsUpdate = new CancellationTokenSource();
            CancellationToken token = _ctsUpdate.Token;
            StartTracker();
            try
            {
                _updateTask = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        foreach (MotionControllerModel mc in _camera.Controllers)
                        {
                            if(mc.Tracking[_camera])
                            {
                                if(mc.TrackerStatus[_camera] == PSMoveTrackerStatus.NotCalibrated)
                                {
                                    EnableTracking(mc);
                                }
                            }
                            else
                            {
                                if(mc.TrackerStatus[_camera] != PSMoveTrackerStatus.NotCalibrated)
                                {
                                    DisableTracking(mc);
                                }
                            }
                        }
                        UpdateTracker();
                        UpdateImage();
                    }
                });
                await _updateTask;
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Stop();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Stop();
            }
        }

        private void CancelUpdateTask()
        {
            if(_ctsUpdate != null)
            {
                _ctsUpdate.Cancel();
                _updateTask.Wait();
                PsMoveApi.delete_PSMoveTracker(_camera.Handle);
                _camera.Handle = IntPtr.Zero;
            }
        }
        #endregion

        #region Interface Implementation
        public bool Enabled
        {
            get;
            set;
        }

        public void Initialize(SingleCameraModel camera)
        {
            _camera = camera;
        }

        public bool Start()
        {
            StartUpdateTask();

            return Enabled = true;
        }

        public bool Stop()
        {
            CancelUpdateTask();
            return Enabled = false;
        }

        public void AddMotionController(MotionControllerModel mc)
        {
            if(!_camera.Controllers.Contains(mc))
            {
                _camera.Controllers.Add(mc);
            }
        }

        public void RemoveMotionController(MotionControllerModel mc)
        {       
            if(_camera.Controllers.Contains(mc))
            {
                if(mc.Tracking[_camera])
                {
                    DisableTracking(mc);
                }
                _camera.Controllers.Remove(mc);
            }
        }

        public void UpdateImage()
        {
            if (_camera.ShowImage && _camera.Handle != IntPtr.Zero)
            {
                //display useful information
                if (_camera.Annotate) PsMoveApi.psmove_tracker_annotate(_camera.Handle);
                //retrieve and convert image frame
                IntPtr frame = PsMoveApi.psmove_tracker_get_frame(_camera.Handle);
                MIplImage rgb32Image = (MIplImage) Marshal.PtrToStructure(frame, typeof(MIplImage));
                Image<Bgr, Byte> img = new Image<Bgr, byte>(rgb32Image.width, rgb32Image.height, rgb32Image.widthStep, rgb32Image.imageData);


                if (_camera.Debug) DrawCubeToImage(img);

                if(true)
                {
                    // draw center of image for calibration
                    //img.Draw(new Rectangle(315, 235, 10, 10), new Bgr(0, 255, 0), 1);

                    List<MotionControllerViewModel> mcvms = SimpleIoc.Default.GetAllCreatedInstances<MotionControllerViewModel>().ToList<MotionControllerViewModel>();
                    List<SingleCameraViewModel> scvms = SimpleIoc.Default.GetAllCreatedInstances<SingleCameraViewModel>().ToList<SingleCameraViewModel>();
                    IEnumerable<SingleCameraViewModel> orderedScvms = scvms.OrderBy((view) => view.Camera.Calibration.Position);

                    foreach (SingleCameraViewModel scvm in orderedScvms)
                    {

                        if (scvm.Camera.Calibration.ObjectPointsProjected == null || _camera.Calibration.ObjectPointsProjected == null) continue;
                        else if (scvm.Camera.Calibration.Position == _camera.Calibration.Position) continue;
                        else if (scvm.Camera.Calibration.Position > 1) continue;

                        IntPtr points1Ptr = Utils.CreatePointListPointer(scvm.Camera.Calibration.ObjectPointsProjected);
                        IntPtr points2Ptr = Utils.CreatePointListPointer(_camera.Calibration.ObjectPointsProjected);

                        Matrix<double> fundamentalMatrix = new Matrix<double>(3, 3);

                        IntPtr fundamentalMatrixPtr = CvInvoke.cvCreateMat(3, 3, MAT_DEPTH.CV_32F);
                        Emgu.CV.CvInvoke.cvFindFundamentalMat(points1Ptr, points2Ptr, fundamentalMatrix.Ptr, CV_FM.CV_FM_RANSAC, 3, 0.99, IntPtr.Zero);

                        Matrix<double> lines1 = new Matrix<double>(8, 3);
                        Emgu.CV.CvInvoke.cvComputeCorrespondEpilines(points2Ptr, 2, fundamentalMatrix.Ptr, lines1.Ptr);

                        Matrix<double> lines2 = new Matrix<double>(8, 3);
                        Emgu.CV.CvInvoke.cvComputeCorrespondEpilines(points1Ptr, 1, fundamentalMatrix.Ptr, lines2.Ptr);

                        for (int i = 0; i < scvm.Camera.Calibration.ObjectPointsProjected.Length; i++)
                        {
                            {
                                System.Drawing.Point[] points = new System.Drawing.Point[2]
                                {
                                    new System.Drawing.Point(0, (int) -(lines2[i, 2]/lines2[i, 1])),
                                    new System.Drawing.Point(img.Cols, (int) (-(lines2[i, 2] + lines2[i, 0] * img.Cols) / lines2[i, 1]))
                                };
                                img.DrawPolyline(points, false, new Bgr(255, 255, 0), 1);
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
                
                BitmapSource bitmapSource = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(img);
                //_camera.ImageSource = (BitmapSource) Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(new Image<Bgr, byte>(rgb32Image.width, rgb32Image.height, rgb32Image.widthStep, rgb32Image.imageData)).GetAsFrozen();
                //display image
                //System.Drawing.Bitmap bitmap = MIplImagePointerToBitmap(rgb32Image);
                //BitmapSource bitmapSource = loadBitmap(bitmap);
                bitmapSource.Freeze();
                _camera.ImageSource = bitmapSource;
            }
        }
        #endregion

        #region Tracker
        public void UpdateTracker()
        {
            if (_camera.Handle != IntPtr.Zero && !_ctsUpdate.IsCancellationRequested)
            {
                try
                {
                    PsMoveApi.psmove_tracker_update_image(_camera.Handle);
                    foreach (MotionControllerModel mc in _camera.Controllers)
                    {
                        if (mc.Tracking[_camera])
                        {
                            PsMoveApi.psmove_tracker_update(_camera.Handle, mc.Handle);
                            ProcessData(mc);
                        }
                    }
                }
                catch(AccessViolationException e)
                {
                    Console.WriteLine(e.StackTrace);
                    Stop();
                }
            }
        }

        public bool StartTracker()
        {
            if (_camera.Handle == IntPtr.Zero)
            {
                _camera.Handle = PsMoveApi.psmove_tracker_new_with_camera(_camera.TrackerId);
                ConsoleService.WriteLine(string.Format("[Tracker, {0}] Started.", _camera.GUID));
                PsMoveApi.psmove_tracker_set_dimming(_camera.Handle, 1f);
                _camera.Fusion = PsMoveApi.new_PSMoveFusion(_camera.Handle, 1.0f, 1000f);
            }
            return _camera.Handle != IntPtr.Zero;
        }

        public void EnableTracking(MotionControllerModel mc)
        {
            if (_camera.Handle == IntPtr.Zero) StartTracker();

            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Calibrating Motion Controller ({0}).", _camera.GUID, mc.Serial));

            byte r = (byte)((mc.Color.r * 255) + 0.5f);
            byte g = (byte)((mc.Color.g * 255) + 0.5f);
            byte b = (byte)((mc.Color.b * 255) + 0.5f);

            mc.TrackerStatus[_camera] = PsMoveApi.psmove_tracker_enable_with_color(_camera.Handle, mc.Handle, r, g, b);
            
            if (mc.TrackerStatus[_camera] == PSMoveTrackerStatus.Tracking 
                || mc.TrackerStatus[_camera] == PSMoveTrackerStatus.Calibrated)
            {
                PsMoveApi.psmove_tracker_update_image(_camera.Handle);
                PsMoveApi.psmove_tracker_update(_camera.Handle, mc.Handle);
                mc.TrackerStatus[_camera] = PsMoveApi.psmove_tracker_get_status(_camera.Handle, mc.Handle);
                PsMoveApi.psmove_enable_orientation(mc.Handle, PSMoveBool.True);
                PsMoveApi.psmove_reset_orientation(mc.Handle);
            }

            Matrix4x4 proj = new Matrix4x4();

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    proj[row, col] = PsMoveApi.PSMoveMatrix4x4_at(PsMoveApi.psmove_fusion_get_projection_matrix(_camera.Fusion), row * 4 + col);
                }
            }

            mc.ProjectionMatrix[_camera] = proj;

            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Tracker Status of {0} = {2}", 
                _camera.GUID, mc.Name, Enum.GetName(typeof(PSMoveTrackerStatus), mc.TrackerStatus[_camera])));
        }

        public void DisableTracking(MotionControllerModel motionController)
        {
            PsMoveApi.psmove_tracker_disable(_camera.Handle, motionController.Handle);
            motionController.Tracking[_camera] = false;
            motionController.TrackerStatus[_camera] = PSMoveTrackerStatus.NotCalibrated;
            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Tracking of Motion Controller ({1}) disabled.", 
                _camera.GUID, motionController.Serial));
        }

        public void DisableTracking()
        {
            if (_camera.Handle != IntPtr.Zero)
            {
                foreach (MotionControllerModel mc in _camera.Controllers)
                {
                    DisableTracking(mc);
                }
            }
        }

        public void Destroy()
        {
            if (_camera.Handle != IntPtr.Zero)
            {
                DisableTracking();
                CancelUpdateTask();
                ConsoleService.WriteLine(string.Format("[Tracker, {0}] Tracker destroyed.", _camera.GUID));
            }
            if (_camera.Fusion != IntPtr.Zero)
            {
                PsMoveApi.psmove_fusion_free(_camera.Fusion);
                _camera.Fusion = IntPtr.Zero;
                ConsoleService.WriteLine(string.Format("[Tracker, {0}] Fusion destroyed.", _camera.GUID));
            }
        }

        public void ProcessData(MotionControllerModel mc)
        {
            if (_camera.Handle != IntPtr.Zero)
            {
                PSMoveTrackerStatus trackerStatus = mc.TrackerStatus[_camera];
                if(mc.Id >= 0) trackerStatus = PsMoveApi.psmove_tracker_get_status(_camera.Handle, mc.Handle);
                Vector3 rawPosition = Vector3.zero;
                Vector3 fusionPosition = Vector3.zero;
                Matrix4x4 model = new Matrix4x4();

                if (trackerStatus == PSMoveTrackerStatus.Tracking)
                {
                    float rx = 0.0f, ry = 0.0f, rrad = 0.0f;
                    float fx = 0.0f, fy = 0.0f, fz = 0.0f;
                    PsMoveApi.psmove_tracker_get_position(_camera.Handle, mc.Handle, out rx, out ry, out rrad);
                    PsMoveApi.psmove_fusion_get_position(_camera.Fusion, mc.Handle, out fx, out fy, out fz);
                    rx = (float)((int) (rx + 0.5));
                    ry = (float)((int) (ry + 0.5));
                    //Console.WriteLine(rx + " " + ry + " " + rrad);

                    rawPosition = new Vector3(rx, ry, rrad);
                    fusionPosition = new Vector3(fx, fy, fz);
                    //#if YISUP
                    //vec.x = -vec.x;
                    //vec.y = -vec.y;
                    //vec.z = -vec.z;
                    //#endif
                    //tc.m_positionScalePos = Vector3.Max(vec, tc.m_positionScalePos);
                    //tc.m_positionScaleNeg = Vector3.Min(vec, tc.m_positionScaleNeg);

                    //Vector3 extents = tc.m_positionScalePos - tc.m_positionScaleNeg;

                    //vec = vec - tc.m_positionScaleNeg;
                    //vec.x = vec.x/extents.x;
                    //vec.y = vec.y/extents.y;
                    //vec.z = vec.z/extents.z;
                    //vec = vec*2.0f - Vector3.one;

                    //for (int i = mc.m_positionHistory.Length - 1; i > 0; --i)
                    //{
                    //    mc.m_positionHistory[i] = mc.m_positionHistory[i - 1];
                    //}
                    //mc.m_positionHistory[0] = vec;

                    //vec = m_positionHistory[0]*0.3f + m_positionHistory[1]*0.5f + m_positionHistory[2]*0.1f + m_positionHistory[3]*0.05f + m_positionHistory[4]*0.05f;

                    //mc.m_position = vec;

                    //for (int row = 0; row < 4; row++)
                    //{
                    //    for (int col = 0; col < 4; col++)
                    //    {
                    //        model[row, col] = PsMoveApi.PSMoveMatrix4x4_at(PsMoveApi.psmove_fusion_get_modelview_matrix(_camera.Fusion, mc.Handle), row * 4 + col);
                    //    }
                    //}

                    PointF[] imgPts = Utils.GetImagePointsF(mc.RawPosition[_camera]);

                    ExtrinsicCameraParameters ex = Emgu.CV.CameraCalibration.FindExtrinsicCameraParams2(
                        _camera.Calibration.ObjectPoints2D,
                        imgPts,
                        _camera.Calibration.IntrinsicParameters);

                    ex.RotationVector[0, 0] += (Math.PI / 180) * _camera.Calibration.RotX;
                    ex.RotationVector[1, 0] += (Math.PI / 180) * (_camera.Calibration.RotY + _camera.Calibration.YAngle);
                    ex.RotationVector[2, 0] += (Math.PI / 180) * _camera.Calibration.RotZ;

                    _camera.Calibration.ExtrinsicParameters[mc.Id] = ex;
                    Matrix<double> R3x3_cameraToWorld = new Matrix<double>(3, 3);
                    R3x3_cameraToWorld = rotate(-_camera.Calibration.RotX, -_camera.Calibration.RotY - _camera.Calibration.YAngle, -_camera.Calibration.RotZ);

                    Matrix<double> test3x1 = new Matrix<double>(3, 1);
                    test3x1[0, 0] += (Math.PI / 180) * _camera.Calibration.RotX;
                    test3x1[1, 0] += (Math.PI / 180) * (_camera.Calibration.RotY + _camera.Calibration.YAngle);
                    test3x1[2, 0] += (Math.PI / 180) * _camera.Calibration.RotZ;

                    Matrix<double> test3x3 = new Matrix<double>(3, 3);
                    CvInvoke.cvRodrigues2(test3x1, test3x3, IntPtr.Zero);

                    //IntPtr dstPtr = CvInvoke.cvCreateMat(3, 3, MAT_DEPTH.CV_64F);
                    //Emgu.CV.CvInvoke.cvInvert(ex.RotationVector.RotationMatrix.Ptr, dstPtr, SOLVE_METHOD.CV_LU);

                    //Matrix<double> rotInv = new Matrix<double>(3, 3, dstPtr);

                    _camera.Calibration.ObjectPointsProjected = Emgu.CV.CameraCalibration.ProjectPoints(
                        _camera.Calibration.ObjectPoints3D,
                        _camera.Calibration.ExtrinsicParameters[mc.Id],
                        _camera.Calibration.IntrinsicParameters);

                    Matrix<double> cameraPositionInWorldSpace4x4 = Utils.Get3DTranslationMatrix(_camera.Calibration.TranslationVector);

                    Matrix<double> coordinatesInCameraSpace_homo = new Matrix<double>(4, 1);
                    coordinatesInCameraSpace_homo[0, 0] = ex.TranslationVector[0, 0];
                    coordinatesInCameraSpace_homo[1, 0] = ex.TranslationVector[1, 0];
                    coordinatesInCameraSpace_homo[2, 0] = ex.TranslationVector[2, 0];
                    coordinatesInCameraSpace_homo[3, 0] = 1;

                    mc.CameraPosition[_camera] = new Vector3((float) coordinatesInCameraSpace_homo[0, 0], (float) coordinatesInCameraSpace_homo[1, 0], (float) coordinatesInCameraSpace_homo[2, 0]);
                    Matrix<double> Rt_homo = Utils.ConvertToHomogenous(-1 * R3x3_cameraToWorld);
                    Matrix<double> x_world_homo = Utils.ConvertToHomogenous(R3x3_cameraToWorld) * coordinatesInCameraSpace_homo;
                    Rt_homo[0, 3] = x_world_homo[0, 0];
                    Rt_homo[1, 3] = x_world_homo[1, 0];
                    Rt_homo[2, 3] = x_world_homo[2, 0];
                    x_world_homo = cameraPositionInWorldSpace4x4 * x_world_homo;
                    mc.WorldPosition[_camera] = new Vector3((float)x_world_homo[0, 0], (float)x_world_homo[1, 0], (float)x_world_homo[2, 0]);

                }
                mc.TrackerStatus[_camera] = trackerStatus;
                mc.RawPosition[_camera] = rawPosition;
                mc.FusionPosition[_camera] = fusionPosition;
                mc.ModelViewMatrix[_camera] = model;
            }
        } // ProcessData



        private void DrawCubeToImage(Image<Bgr, byte> img)
        {
            if (_camera.Calibration.ObjectPointsProjected == null
                || _camera.Calibration.ObjectPointsProjected.Length != 8) return;


            System.Drawing.Point[] points = Array.ConvertAll<PointF, System.Drawing.Point>(
                _camera.Calibration.ObjectPointsProjected,
                Utils.PointFtoPoint);

            System.Drawing.Point[] cubeFront = new System.Drawing.Point[4] 
                {
                   points[0],
                   points[1],
                   points[2],
                   points[3],
                };

            System.Drawing.Point[] cubeBack = new System.Drawing.Point[4]
                {
                   points[4],
                   points[5],
                   points[6],
                   points[7],
                };

            System.Drawing.Point[] cubeLeft = new System.Drawing.Point[4]
                {
                   points[0],
                   points[3],
                   points[4],
                   points[7],
                };

            System.Drawing.Point[] cubeRight = new System.Drawing.Point[4]
                {
                   points[1],
                   points[2],
                   points[5],
                   points[6],
                };

            switch (_camera.Calibration.Position)
            {
                case 0:
                    img.DrawPolyline(cubeBack, true, new Bgr(0, 255, 0), 2);
                    img.DrawPolyline(cubeLeft, true, new Bgr(0, 0, 255), 2);
                    img.DrawPolyline(cubeRight, true, new Bgr(255, 255, 0), 2);
                    img.DrawPolyline(cubeFront, true, new Bgr(255, 0, 0), 2);
                    break;
                case 1:
                    img.DrawPolyline(cubeLeft, true, new Bgr(0, 0, 255), 2);
                    img.DrawPolyline(cubeBack, true, new Bgr(0, 255, 0), 2);
                    img.DrawPolyline(cubeFront, true, new Bgr(255, 0, 0), 2);
                    img.DrawPolyline(cubeRight, true, new Bgr(255, 255, 0), 2);
                    break;
                case 2:
                    img.DrawPolyline(cubeFront, true, new Bgr(255, 0, 0), 2);
                    img.DrawPolyline(cubeRight, true, new Bgr(255, 255, 0), 2);
                    img.DrawPolyline(cubeLeft, true, new Bgr(0, 0, 255), 2);
                    img.DrawPolyline(cubeBack, true, new Bgr(0, 255, 0), 2);
                    break;
                case 3:
                    img.DrawPolyline(cubeRight, true, new Bgr(255, 255, 0), 2);
                    img.DrawPolyline(cubeBack, true, new Bgr(0, 255, 0), 2);
                    img.DrawPolyline(cubeFront, true, new Bgr(255, 0, 0), 2);
                    img.DrawPolyline(cubeLeft, true, new Bgr(0, 0, 255), 2);
                    break;
            }
        }

        /** this conversion uses NASA standard aeroplane conventions as described on page:
        *   http://www.euclideanspace.com/maths/geometry/rotations/euler/index.htm
        *   Coordinate System: right hand
        *   Positive angle: right hand
        *   Order of euler angles: heading first, then attitude, then bank
        *   matrix row column ordering:
        *   [m00 m01 m02]
        *   [m10 m11 m12]
        *   [m20 m21 m22]*/
        public Matrix<double> rotate(double x, double y, double z)
        {
            double heading = (Math.PI / 180) * y;
            double attitude = (Math.PI / 180) * z;
            double bank = (Math.PI / 180) * x;

            // Assuming the angles are in radians.
            double ch = Math.Cos(heading);
            double sh = Math.Sin(heading);
            double ca = Math.Cos(attitude);
            double sa = Math.Sin(attitude);
            double cb = Math.Cos(bank);
            double sb = Math.Sin(bank);

            Matrix<double> r = new Matrix<double>(3, 3);
            r[0, 0] = ch * ca;
            r[0, 1] = sh * sb - ch * sa * cb;
            r[0, 2] = ch * sa * sb + sh * cb;
            r[1, 0] = sa;
            r[1, 1] = ca * cb;
            r[1, 2] = -ca * sb;
            r[2, 0] = -sh * ca;
            r[2, 1] = sh * sa * cb + ch * sb;
            r[2, 2] = -sh * sa * sb + ch * cb;

            return r;
        }
        #endregion
    } // TrackerService
} // Namespace
