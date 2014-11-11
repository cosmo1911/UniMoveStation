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
using UniMoveStation.Utilities;

namespace UniMoveStation.Model
{
    public class TrackerService : ITrackerService
    {

        public TrackerService(int id)
        {
            tracker = new UniMoveTracker(id);
        }

        /// <summary>
        /// BackgroundWorker updating the image while tracking
        /// </summary>
        public BackgroundWorker bw;

        /// <summary>
        /// indicates that the BackgroudWorker was cancelled successfully
        /// </summary>
        private AutoResetEvent bwResetEvent;

        public UniMoveTracker tracker;

        public List<UniMoveController> moves;

        public bool Annotate
        {
            get;
            set;
        }

        public ImageSource ImageSource
        {
            get;
            set;
        }



        public void StartTracking()
        {
            initBackgroundWorker();
        }

        public void StopTracking()
        {
            cancelBackgroundWorker();
        }

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

        #region [ BackgroundWorker ]
        private void initBackgroundWorker()
        {
            //init worker
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;

            //add event handlers
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bwResetEvent = new AutoResetEvent(false);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //update image 
            while (!worker.CancellationPending)
            {
                tracker.UpdateTracker();
                bw.ReportProgress(0);
            }

            e.Cancel = true;
            //remove event handlers
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged -= new ProgressChangedEventHandler(bw_ProgressChanged);

            //signal completion
            bwResetEvent.Set();
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //updateImage();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //remove event handlers
            worker.DoWork -= new DoWorkEventHandler(bw_DoWork);
            worker.ProgressChanged -= new ProgressChangedEventHandler(bw_ProgressChanged);
            //signal completion
            bwResetEvent.Set();
        }

        public void cancelBackgroundWorker()
        {
            bw.CancelAsync();
            //wait until worker is finished
            bwResetEvent.WaitOne(-1);
            bwResetEvent = new AutoResetEvent(false);

            //console.AppendText(string.Format("Camera {0} stopped tracking.\n", trackerId));
        }
        #endregion

        public void AddMotionController(UniMoveController motionController)
        {
            moves.Add(motionController);
            tracker.EnableTracking(motionController, UnityEngine.Color.blue);
        }

        public void RemoveMotionController(UniMoveController motionController)
        {
            foreach(UniMove.UniMoveTracker.TrackedController controller in tracker.controllers)
            {
                if (controller.move == motionController)
                {
                    tracker.controllers.Remove(controller);
                    break;
                }
            }
            moves.Remove(motionController);
        }

        public void UpdateImage()
        {
            if (tracker.getCPtr().Handle != IntPtr.Zero)
            {
                //display useful information
                if (Annotate)
                {
                    tracker.annotate();
                }
                //retrieve and convert image frame
                IntPtr frame = tracker.get_frame();
                MIplImage rgb32Image = new MIplImage();
                rgb32Image = (MIplImage) Marshal.PtrToStructure(frame, typeof(MIplImage));
                //display image
                System.Drawing.Bitmap bitmap = Utils.MIplImagePointerToBitmap(rgb32Image);
                ImageSource = Utils.loadBitmap(bitmap);
            }
        }


        public void ToggleCamera(SingleCameraModel.CameraState state)
        {
            throw new NotImplementedException();
        }
    }
}
