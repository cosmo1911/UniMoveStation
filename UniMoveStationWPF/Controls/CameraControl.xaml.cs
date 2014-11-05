using CLEyeMulticam;
using Emgu.CV.Structure;
using UniMoveStation.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using UniMove;
using UnityEngine;

namespace UniMoveStation
{
    /// <summary>
    /// Interaction logic for CameraControl.xaml
    /// </summary>
    public partial class CameraControl : UserControl
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

        public Observable<string> ImageLabelTitleContent { get; set; }

        #region [ Contructor ]
        /// <summary>
        /// default constructor (just for designer)
        /// </summary>
        public CameraControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackerId">index of the camera</param>
        public CameraControl(int trackerId)
        {
            InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;
            //SizeChanged += CameraControl_SizeChanged;
            //resizeTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 1500), IsEnabled = false };
            //resizeTimer.Tick += resizeTimer_Tick;

            this.trackerId = trackerId;

            ImageLabelTitleContent = new Observable<string>(null)
            {
                Value = "Camera " + trackerId
            };
            DataContext = this;

            moves = new List<UniMoveController>();

    //        IObservable<SizeChangedEventArgs> ObservableSizeChanges = 
    //            Observable.FromEventPattern<SizeChangedEventArgs>(this, "SizeChanged")
    //.Select(x => x.EventArgs)
    //.Throttle(TimeSpan.FromMilliseconds(500));

    //        IDisposable SizeChangedSubscription = ObservableSizeChanges
    //            .ObserveOn(SynchronizationContext.Current)
    //            .Subscribe(x =>
    //            {
    //                Size_Changed(x);
    //            });
        }
        #endregion

        public void toggleTracker(bool enable)
        {
            if (enable)
            {

            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="moves"></param>
        public void initTracker(List<UniMoveController> moves)
        {
            //stop CLEye Camera if enabled
            toggleCLEyeCamera(false);

            //enable all UniMoveControllers for tracking
            tracker = new UniMoveTracker();
            tracker.StartTracker(trackerId);
            for (int i = 0; i < moves.Count; i++)
            {
                if (checkBoxList_moves.checkBoxListBoxItems[i].IsChecked)
                {
                    tracker.EnableTracking(moves[i], trackerId, colors[i]);
                }
                else
                {
                    tracker.controllers.Add(new UniMoveTracker.TrackedController(moves[i]));
                }
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
            //textBox_X.Text = vector.x.ToString();
            //textBox_Y.Text = vector.y.ToString();
            //textBox_R.Text = vector.z.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        private void setImage(MIplImage image)
        {
            clEyeImageControl.Visibility = System.Windows.Visibility.Hidden;
            imageControl.Visibility = System.Windows.Visibility.Visible;

            System.Drawing.Bitmap bitmap = Utils.MIplImagePointerToBitmap(image);
            //rescale bitmap?
            //bitmap = new System.Drawing.Bitmap(bitmap, new System.Drawing.Size((int)label_cameraBackground.ActualWidth, (int)label_cameraBackground.ActualHeight));
            imageControl.Source = Utils.loadBitmap(bitmap);
        }

        /// <summary>
        /// starts or stops displaying the camera image from the CL Eye Camera
        /// </summary>
        private void toggleCLEyeCamera(bool enable)
        {
            if (enable)
            {
                label_cameraBackground.Visibility = System.Windows.Visibility.Hidden;

                clEyeImageControl.start(trackerId);

                initializedCLEye = true;
                button_initCamera.Content = "Stop Camera";
            }
            else
            {
                clEyeImageControl.stop();

                label_cameraBackground.Visibility = System.Windows.Visibility.Visible;

                button_initCamera.Content = "Init Camera";
                initializedCLEye = false;
            }
        }

        /// <summary>
        /// retrieves the tracker image and displays it
        /// </summary>
        private void updateImage()
        {
            if (tracker.getCPtr().Handle != IntPtr.Zero)
            {
                //display useful information
                if ((bool)checkBox_annotate.IsChecked)
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
                Thread.Sleep(100);
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
            updateImage();
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

        #region [ Control Clicks ]
        void button_initCamera_Click(object sender, EventArgs e)
        {
            toggleCLEyeCamera(!initializedCLEye);
        }

        void checkBox_annotate_Click(object sender, System.EventArgs e)
        {

        }
        #endregion

        #region [ Misc ]
        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            toggleCLEyeCamera(false);
            clEyeImageControl.Dispose();
            if (bw != null)
            {
                cancelBackgroundWorker();
            }

            if (tracker != null)
            {
                tracker.DisableTracking();
            }
        }
        #endregion

        private void checkBox_enableTracking_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            enabledForTracking = !enabledForTracking;

            if (enabledForTracking)
            {
                List<UniMoveController> moves = new List<UniMoveController>();
                moves.AddRange(checkBoxList_moves.moves);
                initTracker(moves);
                bw.RunWorkerAsync();
            }
            else
            {

            }
        }
    }
}
