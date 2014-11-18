using io.thp.psmove;
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
        private PSMoveRemoteConfig _remoteConfig = PSMoveRemoteConfig.LocalAndRemote;

        public MotionControllerModel MotionController
        {
            get
            {
                return _motionController;
            }
            set
            {
                _motionController = value;
            }
        }

        public MotionControllerModel Start()
        {
            OnControllerDisconnected += MotionControllerService_OnControllerDisconnected;
            InitBackgroundWorker();
            return MotionController;
        }

        void MotionControllerService_OnControllerDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine(_motionController.Name + " disconnected");
        }

        public void Initialize(int id)
        {
            pinvoke.set_remote_config((int) _remoteConfig);

            MotionController = new MotionControllerModel();

            _motionController.ConnectStatus = Init(id);
            if(_motionController.ConnectStatus == PSMoveConnectStatus.OK)
            {
                _motionController.Id = id;
                _motionController.Color = UnityEngine.Color.white;
                _motionController.Serial = get_serial();
                _motionController.UpdateRate = UpdateRate;
                _motionController.Remote = is_remote() > 0;
                _motionController.ConnectionType = ConnectionType;
                _motionController.HostIp = get_moved_host();

                UpdateController();

                _motionController.Temperature = Temperature;
                _motionController.BatteryLevel = Battery;
                _motionController.RawAcceleration = RawAcceleration;
                _motionController.RawGyroscope = RawGyroscope;
                _motionController.Acceleration = Acceleration;
                _motionController.Gyroscope = Gyro;
            }
            else
            {

            }
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
            UpdateButtons();
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
            if(bw != null)
            {
                bw.CancelAsync();
                //wait until worker is finished
                bwResetEvent.WaitOne(-1);
            }
        }
        #endregion [ BackgroundWorker ]

        private void UpdateButtons()
        {
            _motionController.Circle = GetButton(PSMoveButton.Circle);
            _motionController.Cross = GetButton(PSMoveButton.Cross);
            _motionController.Triangle = GetButton(PSMoveButton.Triangle);
            _motionController.Square = GetButton(PSMoveButton.Square);
            _motionController.Move = GetButton(PSMoveButton.Move);
            _motionController.PS = GetButton(PSMoveButton.PS);
            _motionController.Start = GetButton(PSMoveButton.Start);
            _motionController.Select = GetButton(PSMoveButton.Select);
            _motionController.Trigger = (int) (Trigger * 255);
        }

        #region Imports

        #endregion
    }
}
