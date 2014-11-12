using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
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

        #region Constructor
        public TrackerService(SingleCameraModel camera)
        {
            _camera = camera;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        private void InitTracker(List<UniMoveController> moves)
        {
            //enable all UniMoveControllers for tracking
            _camera.Tracker = new UniMoveTracker(0);
            for (int i = 0; i < moves.Count; i++)
            {
                _camera.Tracker.EnableTracking(moves[i], colors[i]);
            }

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

        public bool Start()
        {
            _camera.Controllers = new List<UniMoveController>();
            UniMoveController controller = new UniMoveController();
            controller.Init(0);
            _camera.Controllers.Add(controller);
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
                System.Drawing.Bitmap bitmap = Utils.MIplImagePointerToBitmap(rgb32Image);
                _camera.ImageSource = Utils.loadBitmap(bitmap);
            }
        }
        #endregion
    } // TrackerService
} // Namespace
