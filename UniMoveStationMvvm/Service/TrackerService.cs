using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
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
                if (_camera.Annotate)
                {
                    PsMoveApi.psmove_tracker_annotate(_camera.Handle);
                }
                //retrieve and convert image frame
                IntPtr frame = PsMoveApi.psmove_tracker_get_frame(_camera.Handle);
                MIplImage rgb32Image = (MIplImage) Marshal.PtrToStructure(frame, typeof(MIplImage));
                Image<Bgr, Byte> img = new Image<Bgr, byte>(rgb32Image.width, rgb32Image.height, rgb32Image.widthStep, rgb32Image.imageData);

                // draw center of image for calibration
                //img.Draw(new Rectangle(315, 235, 10, 10), new Bgr(0, 255, 0), 1);


                
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
                PsMoveApi.psmove_tracker_update_image(_camera.Handle);
                foreach (MotionControllerModel mc in _camera.Controllers)
                {
                    if (mc.Tracking[_camera])
                    {
                        PsMoveApi.psmove_tracker_update(_camera.Handle, mc.Handle);
                        ProcessData();
                    }
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

            ConsoleService.WriteLine(string.Format("[Tracker, {0}] Tracker Status of Motion Controller ({0}) = {1}", 
                _camera.GUID, mc.Serial, Enum.GetName(typeof(PSMoveTrackerStatus), mc.TrackerStatus[_camera])));
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

        public void ProcessData()
        {
            if (_camera.Handle != IntPtr.Zero)
            {
                foreach (MotionControllerModel mc in _camera.Controllers)
                {
                    PSMoveTrackerStatus trackerStatus = mc.TrackerStatus[_camera];
                    trackerStatus = PsMoveApi.psmove_tracker_get_status(_camera.Handle, mc.Handle);
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

                        float rz = PsMoveApi.psmove_tracker_distance_from_radius(_camera.Handle, rrad);
                        rawPosition = new Vector3(rx, ry, rz);
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

                        for (int row = 0; row < 4; row++)
                        {
                            for (int col = 0; col < 4; col++)
                            {
                                model[row, col] = PsMoveApi.PSMoveMatrix4x4_at(PsMoveApi.psmove_fusion_get_modelview_matrix(_camera.Fusion, mc.Handle), row * 4 + col);
                            }
                        }

                        
                    }
                    mc.TrackerStatus[_camera] = trackerStatus;
                    mc.RawPosition[_camera] = rawPosition;
                    mc.FusionPosition[_camera] = fusionPosition;
                    mc.ModelViewMatrix[_camera] = model;
                }
            }
        } // ProcessData
        #endregion
        #region Tools
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// http://stackoverflow.com/a/1118557
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ip,
                    IntPtr.Zero, 
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                DeleteObject(ip);
            }
            
            return bs;
        }

        /// <summary>
        /// http://www.cnblogs.com/xrwang/archive/2010/01/26/TheInteractionOfOpenCv-EmguCvANDDotNet.html
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public static Bitmap MIplImagePointerToBitmap(MIplImage image)
        {
            
            PixelFormat pixelFormat; // pixel format
            string unsupportedDepth = "Unsupported pixel bit depth IPL_DEPTH";
            string unsupportedChannels = "The number of channels is not supported (only 1,2,4 channels)";
            switch (image.nChannels)
            {
                case 1:
                    switch (image.depth)
                    {
                        case IPL_DEPTH.IPL_DEPTH_8U:
                            pixelFormat = PixelFormat.Format8bppIndexed;
                            break;
                        case IPL_DEPTH.IPL_DEPTH_16U:
                            pixelFormat = PixelFormat.Format16bppGrayScale;
                            break;
                        default:
                            throw new NotImplementedException(unsupportedDepth);
                    }
                    break;
                case 3:
                    switch (image.depth)
                    {
                        case IPL_DEPTH.IPL_DEPTH_8U:
                            pixelFormat = PixelFormat.Format24bppRgb;
                            break;
                        case IPL_DEPTH.IPL_DEPTH_16U:
                            pixelFormat = PixelFormat.Format48bppRgb;
                            break;
                        default:
                            throw new NotImplementedException(unsupportedDepth);
                    }
                    break;
                case 4:
                    switch (image.depth)
                    {
                        case IPL_DEPTH.IPL_DEPTH_8U:
                            pixelFormat = PixelFormat.Format32bppArgb;
                            break;
                        case IPL_DEPTH.IPL_DEPTH_16U:
                            pixelFormat = PixelFormat.Format64bppArgb;
                            break;
                        default:
                            throw new NotImplementedException(unsupportedDepth);
                    }
                    break;
                default:
                    throw new NotImplementedException(unsupportedChannels);

            }
            Bitmap bitmap = new Bitmap(image.width, image.height, image.widthStep, pixelFormat, image.imageData);
            // For grayscale images, but also to modify the color palette
            if (pixelFormat == PixelFormat.Format8bppIndexed)
                SetColorPaletteOfGrayscaleBitmap(bitmap);
            return bitmap;
        }

        public static void SetColorPaletteOfGrayscaleBitmap(Bitmap bitmap)
        {
            PixelFormat pixelFormat = bitmap.PixelFormat;
            if (pixelFormat == PixelFormat.Format8bppIndexed)
            {
                ColorPalette palette = bitmap.Palette;
                for (int i = 0; i < palette.Entries.Length; i++)
                    palette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                bitmap.Palette = palette;
            }
        }
        #endregion
    } // TrackerService
} // Namespace
