using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

using Emgu.CV;
using Emgu.CV.Structure;


using CLEyeMulticam;
using UniMoveStation.IO;

using UniMove;

namespace UniMoveStation
{
    public partial class MainWindow : Form
    {
        private int numCameras = 4;
        private List<CameraControlBox> cameraControls;
        private TCPServerNito server;

        public static TextBox console;

        private List<UniMoveController> moves;
        private List<UniMoveTracker> trackers;

        public MainWindow()
        {
            InitializeComponent();

            console = textBox_output;
            cameraControls = new List<CameraControlBox>();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            button_initTracking.Enabled = false;

            //init cameras
            int cameraCount = CLEyeCameraDevice.CameraCount;

            if (cameraCount == 0)
            {
                console.AppendText("Could not find any PS3Eye cameras!" + Environment.NewLine);
            }
            else
            {
                console.AppendText(string.Format("Found {0} CLEyeCamera devices" + Environment.NewLine, cameraCount));
            }

            //init camera boxes
            cameraPanel.ColumnCount = (int)((numCameras / 2f) + 0.5f);
            cameraPanel.RowCount = 2;

            for (int i = 0; i < numCameras; i++)
            {
                cameraControls.Add(new CameraControlBox(i));
                //two cameras controls per row
                int column = i / 2;
                int row = i % 2;
                cameraPanel.Controls.Add(cameraControls[i], column, row);

                //disable excess CameraControls
                if (i >= cameraCount)
                {
                    cameraControls[i].Enabled = false;
                }
            }

            //count connected moves
            //int connectedMovesCount = UniMoveController.GetNumConnected();
            //console.AppendText(string.Format("Connected Motion Controllers: {0}" + Environment.NewLine, connectedMovesCount));
        } //MainWindow_Load

        public UniMoveController initMove(int index)
        {
            UniMoveController move = new UniMoveController();
            PSMoveConnectStatus status = move.Init(index);
            if (status != PSMoveConnectStatus.OK)
            {
                console.AppendText(string.Format("Move {0} Status: {1}" + Environment.NewLine, index, status));
                return null;
            }

            // This example program only uses Bluetooth-connected controllers
            PSMoveConnectionType conn = move.ConnectionType;
            if (conn == PSMoveConnectionType.Unknown || conn == PSMoveConnectionType.USB)
            {
                console.AppendText(string.Format("Move {0} Status: {1}" + Environment.NewLine, index, status));
                return null;
            }
            else
            {
                move.OnControllerDisconnected += HandleControllerDisconnected;
                move.SetLED(UnityEngine.Color.blue);
                console.AppendText(string.Format("Move {0} Status: {1}" + Environment.NewLine, index, status));
            }
            return move;
        } //initMove

        void HandleControllerDisconnected(object sender, EventArgs e)
        {
            // TODO: Remove this disconnected controller from the list and maybe give an update to the player
        }

        private void button_initMove_Click(object sender, EventArgs e)
        {
            moves = new List<UniMoveController>();
            //for now, only init first move
            UniMoveController move = initMove(0);

            if(move != null)
            {
                moves.Add(move);

                foreach (CameraControlBox cc in cameraControls)
                {
                    cc.moves.Add(move);
                }
            }

            //allow tracking
            if (moves.Count > 0)
            {
                button_initTracking.Enabled = true;
            }
        } //initMove_Click

        private bool checkTrackerAvailability()
        {
            foreach (CameraControlBox cc in cameraControls)
            {
                if (cc.enabledForTracking)
                {
                    return true;
                }
            }
            return false;
        }

        private void enableAvailableTrackers()
        {
            trackers = new List<UniMoveTracker>();

            //enable tracking for each camera control that is enabled for tracking
            foreach (CameraControlBox cc in cameraControls)
            {
                if (cc.Enabled)
                {
                    if (cc.enabledForTracking)
                    {
                        cc.initTracker(moves);
                        trackers.Add(cc.tracker);
                        tcpServerNitoControl.server.init(cameraControls);
                    }
                }
            }

            enableBackgroundWorkers();
        }

        /// <summary>
        /// start background workers
        /// needs to be done after enabling tracking (i.e. blinking calibration)
        /// </summary>
        private void enableBackgroundWorkers()
        {
            foreach (CameraControlBox cc in cameraControls)
            {
                if (cc.Enabled)
                {
                    if (cc.enabledForTracking)
                    {
                        if (cc.tracker != null)
                        {
                            cc.bw.RunWorkerAsync();
                        }
                    }
                }
            }
        } //enableBackgroundWorkers

        #region [ Control Clicks ]
        /// <summary>
        /// Start Tracking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_initTrackers_Click(object sender, EventArgs e)
        {
            if (!checkTrackerAvailability())
            {
                console.AppendText("No tracker available.\n");
                return;
            }

            enableAvailableTrackers();

            button_initMove.Enabled = false;
            button_initTracking.Enabled = false;
            button_stopTracking.Enabled = true;
        } //initTrackers_Click

        private void button_stopTracking_Click(object sender, EventArgs e)
        {
            foreach (CameraControlBox cc in cameraControls)
            {
                if (cc.bw != null)
                {
                    cc.cancelBackgroundWorker();
                    cc.bw.Dispose();
                }

                if (cc.tracker != null)
                {
                    cc.tracker.Dispose();
                }
            }
            button_stopTracking.Enabled = false;
            button_initTracking.Enabled = true;
        } //stopTracking_Click
        #endregion [ Control Clicks ]

        #region [ Misc ]
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //destroy server on closing
            if (server != null)
            {
                server.stop();
            }

            //destroy trackers and background workers
            foreach (CameraControlBox cc in cameraControls)
            {
                if (cc.bw != null)
                {
                    cc.cancelBackgroundWorker();
                }

                if (cc.tracker != null)
                {
                    cc.tracker.DisableTracking();
                }
                //cc.Dispose();
            }
            //destroy moves
            if (moves != null)
            {
                foreach (UniMoveController move in moves)
                {
                    move.Disconnect();
                }
            }

            base.OnFormClosing(e);
        } //OnFormClosing
        #endregion [ Misc ]
    } //mainwindow
} // namespace