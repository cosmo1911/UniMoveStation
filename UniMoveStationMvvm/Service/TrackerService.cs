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
using UniMoveStation.Utilities;

namespace UniMoveStation.Service
{
    public class TrackerService : ITrackerService
    {
        #region Member
        /// <summary>
        /// BackgroundWorker updating the image while tracking
        /// </summary>
        private BackgroundWorker _bw;

        /// <summary>
        /// indicates that the BackgroudWorker was cancelled successfully
        /// </summary>
        private AutoResetEvent _bwResetEvent;

        private SingleCameraModel _camera;

        /// <summary>
        /// Color pool for controllers being tracked
        /// </summary>
        private List<UnityEngine.Color> colors = new List<UnityEngine.Color>()
        {
            UnityEngine.Color.blue,
            UnityEngine.Color.red,
            UnityEngine.Color.green,
            UnityEngine.Color.yellow,
            UnityEngine.Color.white
        };
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        private void InitTracker(List<UniMoveController> moves)
        {
            _camera.Tracker = new UniMoveTracker(_camera.TrackerId);
            _camera.Controllers = moves;
            //start BackgroundWorker updating the image
            InitBackgroundWorker();
        }


        #region [ BackgroundWorker ]
        private void InitBackgroundWorker()
        {
            //init worker
            _bw = new BackgroundWorker();
            _bw.WorkerSupportsCancellation = true;
            _bw.WorkerReportsProgress = true;

            //add event handlers
            _bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            _bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            _bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            _bwResetEvent = new AutoResetEvent(false);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //enable all UniMoveControllers for tracking
            
            for (int i = 0; i < _camera.Controllers.Count; i++)
            {
                _camera.Tracker.EnableTracking(_camera.Controllers[i], colors[i]);
            }

            //update image 
            while (!worker.CancellationPending)
            {
                _camera.Tracker.UpdateTracker();
                _bw.ReportProgress(0);
            }

            e.Cancel = true;
            //remove event handlers
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged -= new ProgressChangedEventHandler(bw_ProgressChanged);

            //signal completion
            _bwResetEvent.Set();
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateImage();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //remove event handlers
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged -= new ProgressChangedEventHandler(bw_ProgressChanged);
            //signal completion
            _bwResetEvent.Set();
        }

        private void CancelBackgroundWorker()
        {
            _bw.CancelAsync();
            //wait until worker is finished
            _bwResetEvent.WaitOne(-1);
            _bwResetEvent = new AutoResetEvent(false);

            //console.AppendText(string.Format("Camera {0} stopped tracking.\n", trackerId));
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
            _camera.Controllers = new List<UniMoveController>();
            UniMoveController controller = new UniMoveController();
            // TODO: replace with proper id reference
            controller.Init(0);
            _camera.Controllers.Add(controller);
        }
        public bool Start()
        {
            InitTracker(_camera.Controllers);
            _bw.RunWorkerAsync();

            return Enabled = true;
        }

        public bool Stop()
        {
            CancelBackgroundWorker();
            _camera.Tracker.DisableTracking();

            return Enabled = false;
        }

        public void AddMotionController(UniMoveController motionController)
        {
            _camera.Controllers.Add(motionController);
            _camera.Tracker.EnableTracking(motionController, UnityEngine.Color.blue);
        }

        public void RemoveMotionController(UniMoveController motionController)
        {
            foreach(UniMove.UniMoveTracker.TrackedController controller in _camera.Tracker.controllers)
            {
                if (controller.move == motionController)
                {
                    _camera.Tracker.controllers.Remove(controller);
                    break;
                }
            }
            _camera.Controllers.Remove(motionController);
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
