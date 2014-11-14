using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniMove;
using UniMoveStation.Model;

namespace UniMoveStation.Service
{
    class MotionControllerService : UniMoveController, IMotionControllerService
    {
        private MotionControllerModel _motionController;

        public void Start(MotionControllerModel motionController)
        {
            _motionController = motionController;
            Init(0);

            OnControllerDisconnected += MotionControllerService_OnControllerDisconnected;
            InitBackgroundWorker();
        }

        void MotionControllerService_OnControllerDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine(_motionController.Name + " disconnected");
        }

        public void Stop()
        {
            CancelBackgroundWorker();
        }

        public bool Enabled
        {
            get;
            set;
        }

        #region [ BackgroundWorker ]
        private BackgroundWorker bw;
        private AutoResetEvent bwResetEvent;

        public void InitBackgroundWorker()
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

            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            //update image 
            while (!worker.CancellationPending)
            {
                UpdateController();
                bw.ReportProgress(0);
                Thread.Sleep(new TimeSpan(0, 0, 0, 0, (int)(UpdateRate * 1000)));
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
            _motionController.Circle = GetButton(PSMoveButton.Circle);
            _motionController.Cross = GetButton(PSMoveButton.Cross);
            _motionController.Triangle = GetButton(PSMoveButton.Triangle);
            _motionController.Square = GetButton(PSMoveButton.Square);
            _motionController.Move = GetButton(PSMoveButton.Move);
            _motionController.PS = GetButton(PSMoveButton.PS);
            _motionController.Start = GetButton(PSMoveButton.Start);
            _motionController.Select = GetButton(PSMoveButton.Select);
            _motionController.Trigger = (int)(Trigger * 255);
            SetLED(_motionController.Color);
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

        public void CancelBackgroundWorker()
        {
            bw.CancelAsync();
            //wait until worker is finished
            bwResetEvent.WaitOne(-1);
        }
        #endregion [ BackgroundWorker ]
    }
}
