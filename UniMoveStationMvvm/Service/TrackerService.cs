using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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
                DisableTracking();
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

        public void AddMotionController(MotionControllerModel motionController)
        {
            //_tracker.Controllers.Add(motionController.Serial, motionController);
            // TODO enable tracking
        }

        public void RemoveMotionController(MotionControllerModel motionController)
        {         
            //_tracker.Controllers.Remove(motionController.Serial);
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
                MIplImage rgb32Image = new MIplImage();
                rgb32Image = (MIplImage)Marshal.PtrToStructure(frame, typeof(MIplImage));
                //display image
                System.Drawing.Bitmap bitmap = MIplImagePointerToBitmap(rgb32Image);
                BitmapSource bitmapSource = loadBitmap(bitmap);
                bitmapSource.Freeze();
                _camera.ImageSource = bitmapSource;
            }
        }
        #endregion

        #region Tracker
        public void UpdateTracker()
        {
            if (_camera.Handle != IntPtr.Zero)
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
                //dimming = 1f;
                //			fusion = psmove_fusion_new(tracker,0.001f,1.0f);
            }
            return _camera.Handle != IntPtr.Zero;
        }

        public void EnableTracking(MotionControllerModel mc)
        {
            if (_camera.Handle == IntPtr.Zero)
            {
                StartTracker();
            }

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
            }

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
                DestroyTracker();
            }
        }

        public void DestroyTracker()
        {
            if (_camera.Handle != IntPtr.Zero)
            {
                PsMoveApi.delete_PSMoveTracker(_camera.Handle);
                _camera.Handle = IntPtr.Zero;
                ConsoleService.WriteLine(string.Format("[Tracker, {0}] Destroyed.", _camera.GUID));
            }
            //if (fusion != IntPtr.Zero)
            //{
            //    psmove_fusion_free(fusion);
            //    fusion = IntPtr.Zero;
            //    Console.WriteLine("fusion destroyed");
            //}
        }

        public void ProcessData()
        {
            if (_camera.Handle != IntPtr.Zero)
            {
                foreach (MotionControllerModel mc in _camera.Controllers)
                {
                    PSMoveTrackerStatus trackerStatus = mc.TrackerStatus[_camera];
                    trackerStatus = PsMoveApi.psmove_tracker_get_status(_camera.Handle, mc.Handle);
                    Vector3 position = Vector3.zero;
                    if (trackerStatus == PSMoveTrackerStatus.Tracking)
                    {
                        float rx = 0.0f, ry = 0.0f, rrad = 0.0f;
                        PsMoveApi.psmove_tracker_get_position(_camera.Handle, mc.Handle, out rx, out ry, out rrad);

                        //Console.WriteLine(rx + " " + ry + " " + rrad);

                        float rz = PsMoveApi.psmove_tracker_distance_from_radius(_camera.Handle, rrad);
                        position = new Vector3(rx, ry, rz);
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
                    }
                    mc.TrackerStatus[_camera] = trackerStatus;
                    mc.Position[_camera] = position;
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
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
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
