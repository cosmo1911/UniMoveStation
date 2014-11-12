using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using CLEyeMulticam;

using UnityEngine;

using UniMove;
using UniMoveStation.Utilities;

namespace UniMoveStation
{
    public partial class CameraControlBox : UserControl
    {
        /// <summary>
        /// BackgroundWorker updating the image while tracking
        /// </summary>
        public BackgroundWorker bw;

        /// <summary>
        /// indicates that the BackgroudWorker was cancelled successfully
        /// </summary>
        private AutoResetEvent bwResetEvent;

        /// <summary>
        ///  Control displaying an Image from CL Eye Camera while not tracking
        /// </summary>
        private CLEyeImageControl image;

        /// <summary>
        /// is the image of the CL Eye Camera being displayed?
        /// </summary>
        private bool initializedCLEye = false;

        /// <summary>
        /// index of the assigned tracker
        /// </summary>
        private int trackerId = 0;

        /// <summary>
        /// UniMoveTracker assigned to this control
        /// </summary>
        public UniMoveTracker tracker;

        /// <summary>
        /// UniMoveControllers being tracked by the assigned tracker
        /// </summary>
        public List<UniMoveController> moves;

        /// <summary>
        /// is tracker allowed to track?
        /// </summary>
        public bool enabledForTracking = false;

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

        #region [ Contructor ]
        /// <summary>
        /// default constructor (just for designer)
        /// </summary>
        public CameraControlBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackerId">index of the camera</param>
        public CameraControlBox(int trackerId)
        {
            InitializeComponent();
            this.trackerId = trackerId;
            this.Text = "Camera " + trackerId;
            //hide pictureBox overlapping imageHost
            pictureBox.Hide();
            moves = new List<UniMoveController>();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        public void initTracker(List<UniMoveController> moves)
        {
            //stop CLEye Camera if enabled
            if (initializedCLEye)
            {
                toggleCLEye();
            }

            //disable controls
            checkBox_enableTracking.Enabled = false;
            button_initCamera.Enabled = false;

            //enable all UniMoveControllers for tracking
            tracker = new UniMoveTracker();
            for (int i = 0; i < moves.Count; i++)
            {
                tracker.EnableTracking(moves[i], trackerId, colors[i]);
            }

            //start BackgroundWorker updating the image
            initBackgroundWorker();
        }

        #region [ Private Methods ]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vector"></param>
        private void setCoordinates(Vector3 vector)
        {
            textBox_X.Text = vector.x.ToString();
            textBox_Y.Text = vector.y.ToString();
            textBox_R.Text = vector.z.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        private void setImage(CLEyeImageControl image)
        {
            pictureBox.Hide();
            imageHost.Show();

            this.image = image;
            imageHost.Child = image;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        private void setImage(MIplImage image)
        {
            imageHost.Hide();
            pictureBox.Show();

            Bitmap bitmap = Utils.MIplImagePointerToBitmap(image);
            //rescale bitmap to fit pictureBox
            bitmap = new Bitmap(bitmap, pictureBox.Size);
            pictureBox.Image = bitmap;
        }

        /// <summary>
        /// starts or stops displaying the camera image from the CL Eye Camera
        /// </summary>
        private void toggleCLEye()
        {
            if (!initializedCLEye)
            {
                image = new CLEyeImageControl(trackerId);
                image.Start(trackerId);
                setImage(image);

                initializedCLEye = true;
                button_initCamera.Text = "Stop Camera";
            }
            else
            {
                image.Stop();
                setImage(null);
                button_initCamera.Text = "Init Camera";
                initializedCLEye = false;
            }
        }

        /// <summary>
        /// retrieves the tracker image and displays it
        /// </summary>
        private void updateImage()
        {
            if (tracker != null)
            {
                //display useful information
                if (checkBox_annotate.Checked)
                {
                    tracker.annotate();
                }
                //retrieve and convert image frame
                IntPtr frame = tracker.get_frame();
                MIplImage rgb32Image = new MIplImage();
                rgb32Image = (MIplImage)Marshal.PtrToStructure(frame, typeof(MIplImage));
                //display image
                setImage(rgb32Image);
            }
        }
        #endregion

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
            //update ui
            updateImage();
            setCoordinates(tracker.controllers[0].m_position);
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

            MainWindow.console.AppendText(string.Format("Camera {0} stopped tracking.\n", trackerId));
        }
        #endregion

        #region [ Control Clicks ]
        void button_initCamera_Click(object sender, EventArgs e)
        {
            toggleCLEye();
        }

        void checkBox_enableTracking_Click(object sender, EventArgs e)
        {
            enabledForTracking = !enabledForTracking;
        }

        void checkBox_annotate_Click(object sender, System.EventArgs e)
        {

        }
        #endregion

        #region [ Misc ]
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
        #endregion
    } // CameraControl
} // namespace