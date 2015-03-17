using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Event;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Business.PsMove;
using UniMoveStation.Common.Utils;
using UnityEngine;

namespace UniMoveStation.Business.Service
{
    public class TrackerService : DependencyObject, ITrackerService
    {
        #region Member
        public IConsoleService ConsoleService { get; set; }
        private CameraModel _camera;
        public event EventHandler OnImageReady;

        private CancellationTokenSource _ctsUpdate;
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
            StartTracker(PSMoveTrackerExposure.Low);
            try
            {
                _updateTask = Task.Factory.StartNew(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    while (!_ctsUpdate.Token.IsCancellationRequested)
                    {
                        sw.Restart();
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
                        sw.Stop();
                        Thread.Sleep((int) (Math.Max((1000.0 / _camera.FPS) - sw.ElapsedMilliseconds, 0) + 0.5));
                    }
                }, _ctsUpdate.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
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

        public void Initialize(CameraModel camera)
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
            if (_camera.Handle != IntPtr.Zero)
            {
                if (!_camera.ShowImage) return;

                //display useful information
                if (_camera.Annotate) PsMoveApi.psmove_tracker_annotate(_camera.Handle);
                //retrieve and convert image frame
                Image<Bgr, Byte> img = GetImage();

                if (OnImageReady != null) OnImageReady(this, new OnImageReadyEventArgs(img));

                if (_camera.Debug)
                {
                    DrawCubeToImage(img);
                    // draw center of image for calibration
                    img.Draw(new Rectangle(315, 235, 10, 10), new Bgr(0, 255, 0), 1);
                }
                
                BitmapSource bitmapSource = BitmapHelper.ToBitmapSource(img);
                //_camera.ImageSource = (BitmapSource) Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(new Image<Bgr, byte>(rgb32Image.width, rgb32Image.height, rgb32Image.widthStep, rgb32Image.imageData)).GetAsFrozen();
                //display image
                //System.Drawing.Bitmap bitmap = MIplImagePointerToBitmap(rgb32Image);
                //BitmapSource bitmapSource = loadBitmap(bitmap);
                bitmapSource.Freeze();
                _camera.ImageSource = bitmapSource;
            }
        }

        public Image<Bgr, Byte> GetImage()
        {
            if (_camera.Handle == IntPtr.Zero) return null;

            IntPtr frame = PsMoveApi.psmove_tracker_get_frame(_camera.Handle);
            MIplImage rgb32Image = (MIplImage)Marshal.PtrToStructure(frame, typeof(MIplImage));
            return new Image<Bgr, byte>(rgb32Image.width, rgb32Image.height, rgb32Image.widthStep, rgb32Image.imageData);
        }
        #endregion

        #region Tracker
        public void UpdateTracker()
        {
            if (_camera.Handle != IntPtr.Zero)
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

        public bool StartTracker(PSMoveTrackerExposure exposure)
        {
            if (_camera.Handle == IntPtr.Zero)
            {
                _camera.Handle = PsMoveApi.psmove_tracker_new_with_camera(_camera.TrackerId);
                ConsoleService.WriteLine(string.Format("[Tracker, {0}] Started.", _camera.GUID));
                PsMoveApi.psmove_tracker_set_exposure(_camera.Handle, exposure);
                // full led intensity
                PsMoveApi.psmove_tracker_set_dimming(_camera.Handle, 1f);
                _camera.Fusion = PsMoveApi.new_PSMoveFusion(_camera.Handle, 1.0f, 1000f);
            }
            return _camera.Handle != IntPtr.Zero;
        }

        public void EnableTracking(MotionControllerModel mc)
        {
            if (mc.Design) return;

            if (_camera.Handle == IntPtr.Zero) StartTracker(PSMoveTrackerExposure.Low);

            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Calibrating Motion Controller ({1}).", _camera.GUID, mc.Serial));

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

            //Matrix4x4 proj = new Matrix4x4();

            //for (int row = 0; row < 4; row++)
            //{
            //    for (int col = 0; col < 4; col++)
            //    {
            //        proj[row, col] = PsMoveApi.PSMoveMatrix4x4_at(PsMoveApi.psmove_fusion_get_projection_matrix(_camera.Fusion), row * 4 + col);
            //    }
            //}

            //mc.ProjectionMatrix[_camera] = proj;

            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Tracker Status of {1} = {2}",
                _camera.GUID, mc.Name, Enum.GetName(typeof(PSMoveTrackerStatus), mc.TrackerStatus[_camera])));
            
        }

        public void DisableTracking(MotionControllerModel mc)
        {
            if (mc.Design) return;
            PsMoveApi.psmove_tracker_disable(_camera.Handle, mc.Handle);
            mc.Tracking[_camera] = false;
            mc.TrackerStatus[_camera] = PSMoveTrackerStatus.NotCalibrated;
            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Tracking of Motion Controller ({1}) disabled.", 
                _camera.GUID, mc.Serial));
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

        public void StopStracker()
        {
            if (_camera.Handle == IntPtr.Zero) return;
            PsMoveApi.delete_PSMoveTracker(_camera.Handle);
            _camera.Handle = IntPtr.Zero;
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
                Vector3 rawPosition = Vector3.zero;
                Vector3 fusionPosition = Vector3.zero;
                PSMoveTrackerStatus trackerStatus = mc.TrackerStatus[_camera];

                if(!mc.Design) trackerStatus = PsMoveApi.psmove_tracker_get_status(_camera.Handle, mc.Handle);

                if (trackerStatus == PSMoveTrackerStatus.Tracking)
                {
                    float rx = 0.0f, ry = 0.0f, rrad = 0.0f;
                    float fx = 0.0f, fy = 0.0f, fz = 0.0f;
                    PsMoveApi.psmove_tracker_get_position(_camera.Handle, mc.Handle, out rx, out ry, out rrad);
                    PsMoveApi.psmove_fusion_get_position(_camera.Fusion, mc.Handle, out fx, out fy, out fz);
                    rx = (int) (rx + 0.5);
                    ry = (int) (ry + 0.5);
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
                }
                else if (mc.Design)
                {
                    switch (_camera.Calibration.Index)
                    {
                        case 0:
                            rawPosition = new Vector3(129, 280, 8.970074f);
                            break;
                        case 1:
                            rawPosition = new Vector3(180, 293, 11.9714022f);
                            break;
                        case 2:
                            rawPosition = new Vector3(528, 286, 9.038924f);
                            break;
                        case 3:
                            rawPosition = new Vector3(389, 275, 6.530668f);
                            break;
                    }
                }
                mc.TrackerStatus[_camera] = trackerStatus;
                mc.RawPosition[_camera] = rawPosition;
                mc.FusionPosition[_camera] = fusionPosition;

                if (trackerStatus == PSMoveTrackerStatus.Tracking || mc.Design)
                {
                    // controller position -> rectangle in surrounding the sphere in image coordinates
                    PointF[] imgPts = CvHelper.GetImagePointsF(mc.RawPosition[_camera]);

                    ExtrinsicCameraParameters ex = CameraCalibration.FindExtrinsicCameraParams2(
                        _camera.Calibration.ObjectPoints2D,
                        imgPts,
                        _camera.Calibration.IntrinsicParameters);

                    Matrix<double> coordinatesInCameraSpace_homo = new Matrix<double>(new double[]
                    {
                        ex.TranslationVector[0, 0],
                        ex.TranslationVector[1, 0],
                        ex.TranslationVector[2, 0],
                        1
                    });
                    mc.CameraPosition[_camera] = new Vector3(
                        (float)coordinatesInCameraSpace_homo[0, 0], 
                        (float)coordinatesInCameraSpace_homo[1, 0], 
                        (float)coordinatesInCameraSpace_homo[2, 0]);
                    

                    ex.RotationVector[0, 0] += (Math.PI / 180) * (_camera.Calibration.RotX + _camera.Calibration.XAngle);
                    ex.RotationVector[1, 0] += (Math.PI / 180) * (_camera.Calibration.RotY + _camera.Calibration.YAngle);
                    ex.RotationVector[2, 0] += (Math.PI / 180) * (_camera.Calibration.RotZ + _camera.Calibration.ZAngle);

                    _camera.Calibration.ExtrinsicParameters[mc.Id] = ex;
                    Matrix<double> minusRotation = new Matrix<double>(3, 3);
                    minusRotation = CvHelper.Rotate(
                        -_camera.Calibration.RotX - _camera.Calibration.XAngle,
                        -_camera.Calibration.RotY - _camera.Calibration.YAngle,
                        -_camera.Calibration.RotZ - _camera.Calibration.ZAngle);

                    Matrix<double> R3x3_cameraToWorld = new Matrix<double>(3, 3);
                    R3x3_cameraToWorld = CvHelper.Rotate(
                        _camera.Calibration.RotX,
                        _camera.Calibration.RotY + _camera.Calibration.YAngle,
                        _camera.Calibration.RotZ);

                    Matrix<double> rotInv = new Matrix<double>(3, 3);
                    CvInvoke.cvInvert(ex.RotationVector.RotationMatrix.Ptr, rotInv, SOLVE_METHOD.CV_LU);

                    Matrix<double> test = CvHelper.ConvertToHomogenous(-1*R3x3_cameraToWorld);

                    _camera.Calibration.ObjectPointsProjected = CameraCalibration.ProjectPoints(
                        _camera.Calibration.ObjectPoints3D,
                        _camera.Calibration.ExtrinsicParameters[mc.Id],
                        _camera.Calibration.IntrinsicParameters);

                    Matrix<double> cameraPositionInWorldSpace4x4 = new Matrix<double>(new double[,]
                    {
                        {1, 0, 0, _camera.Calibration.TranslationToWorld[0, 0]},
                        {0, 1, 0, _camera.Calibration.TranslationToWorld[1, 0]},
                        {0, 0, 1, _camera.Calibration.TranslationToWorld[2, 0]},
                        {0, 0, 0, 1},
                    });

                    Matrix<double> Rt_homo = CvHelper.ConvertToHomogenous(R3x3_cameraToWorld);
                    Matrix<double> x_world_homo = CvHelper.ConvertToHomogenous(minusRotation) * coordinatesInCameraSpace_homo;
                    Rt_homo[0, 3] = x_world_homo[0, 0];
                    Rt_homo[1, 3] = x_world_homo[1, 0];
                    Rt_homo[2, 3] = x_world_homo[2, 0];
                    x_world_homo = cameraPositionInWorldSpace4x4 * x_world_homo;
                    mc.WorldPosition[_camera] = new Vector3((float)x_world_homo[0, 0], (float)x_world_homo[1, 0], (float)x_world_homo[2, 0]);

                }
            }
        } // ProcessData
        #endregion

        private void DrawCubeToImage(Image<Bgr, byte> img)
        {
            if (_camera.Calibration.ObjectPointsProjected == null
                || _camera.Calibration.ObjectPointsProjected.Length != 8) return;


            System.Drawing.Point[] points = Array.ConvertAll(
                _camera.Calibration.ObjectPointsProjected,
                CvHelper.PointFtoPoint);

            System.Drawing.Point[] cubeFront = new System.Drawing.Point[4] 
                {
                   points[0],
                   points[1],
                   points[2],
                   points[3]
                };

            System.Drawing.Point[] cubeBack = new System.Drawing.Point[4]
                {
                   points[4],
                   points[5],
                   points[6],
                   points[7]
                };

            System.Drawing.Point[] cubeLeft = new System.Drawing.Point[4]
                {
                   points[0],
                   points[3],
                   points[4],
                   points[7]
                };

            System.Drawing.Point[] cubeRight = new System.Drawing.Point[4]
                {
                   points[1],
                   points[2],
                   points[5],
                   points[6]
                };

            switch (_camera.Calibration.Index)
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
    } // TrackerService
} // Namespace
