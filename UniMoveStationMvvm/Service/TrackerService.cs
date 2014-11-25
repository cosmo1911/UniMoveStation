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
using UniMove;
using UniMoveStation.Model;
using UniMoveStation.SharpMove;
using UniMoveStation.Utilities;

namespace UniMoveStation.Service
{
    public class TrackerService : ITrackerService
    {
        #region Member
        private SingleCameraModel _camera;
        private CancellationTokenSource _cts;

        private async void StartTask()
        {
            _cts = new CancellationTokenSource();
            CancellationToken token = _cts.Token;
            try
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < _camera.Controllers.Count; i++)
                    {
                        // TODO enable tracking?
                        //_camera.Tracker.EnableTracking(_camera.Controllers[i], colors[i]);
                    }

                    //update image 
                    while (!token.IsCancellationRequested)
                    {
                        _camera.Tracker.UpdateTracker();
                        UpdateImage();
                    }
                });
            }
            catch(OperationCanceledException)
            {

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void CancelTask()
        {
            if(_cts != null)
            {
                _cts.Cancel();
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
            _camera.Tracker = new UniMoveTracker(_camera.TrackerId);
            _camera.Controllers = new Dictionary<string, SharpMotionController>();
            SharpMotionController controller = new SharpMotionController();
        }
        public bool Start()
        {
            StartTask();

            return Enabled = true;
        }

        public bool Stop()
        {
            CancelTask();
            _camera.Tracker.DisableTracking();

            return Enabled = false;
        }

        public void AddMotionController(SharpMotionController motionController)
        {
            _camera.Controllers.Add(motionController.Serial, motionController);
            // TODO enable tracking
        }

        public void RemoveMotionController(SharpMotionController motionController)
        {         
            _camera.Controllers.Remove(motionController.Serial);
        }

        public void UpdateImage()
        {
            if (_camera.ShowImage && _camera.Tracker.getCPtr().Handle != IntPtr.Zero)
            {
                //display useful information
                if (_camera.Annotate)
                {
                    _camera.Tracker.annotate();
                }
                //retrieve and convert image frame
                IntPtr frame = _camera.Tracker.get_frame();
                MIplImage rgb32Image = new MIplImage();
                rgb32Image = (MIplImage) Marshal.PtrToStructure(frame, typeof(MIplImage));
                //display image
                System.Drawing.Bitmap bitmap = MIplImagePointerToBitmap(rgb32Image);
                _camera.ImageSource = loadBitmap(bitmap);
            }
        }
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
