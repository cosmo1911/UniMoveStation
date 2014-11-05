using CLEyeMulticam;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UniMove;

namespace UniMoveStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private int numCameras = 1;
        private int numControllers = 1;

        private List<CameraControl> cameraControls;
        private List<MoveControl> moveControls;

        public static TextBox console;

        private List<UniMoveController> moves;
        private List<UniMoveTracker> trackers;

        public List<CameraControl> getCameraControls() { return cameraControls; }

        public MainWindow()
        {
            InitializeComponent();

            console = textBox_allCameras;
            cameraControls = new List<CameraControl>();
            moveControls = new List<MoveControl>();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //button_initTracking.Enabled = false;
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

            //count connected moves
            int connectedMovesCount = UniMoveController.GetNumConnected();
            console.AppendText(string.Format("Connected Motion Controllers: {0}" + Environment.NewLine, connectedMovesCount));

            initCamerasGrid();

            initMovesGrid();

            serverControl.cameraControls = cameraControls;
        }

        private void initMovesGrid()
        {
            //init camera controls
            //maximum 2 columns
            int columns = numControllers >= 2 ? 2 : 1;

            //one row for each pair of controllers
            int rows = (int)((numControllers / 2f) + 0.5f);

            //clear grid
            grid_allMoves.ColumnDefinitions.Clear();
            grid_allMoves.RowDefinitions.Clear();

            for (int row = 0; row < rows; row++)
            {
                RowDefinition rd = new RowDefinition();
                grid_allMoves.RowDefinitions.Add(rd);

                for (int column = 0; column < columns; column++)
                {
                    if(row == 0)
                    {
                        ColumnDefinition cd = new ColumnDefinition();
                        grid_allMoves.ColumnDefinitions.Add(cd);
                    }

                    //init move control
                    int index = row * columns + column;
                    if (index >= numControllers)
                        break;

                    MoveControl mc = new MoveControl(index);
                    moveControls.Add(mc);
                    
                    grid_allMoves.Children.Add(mc);

                    //assign cell
                    Grid.SetRow(mc, row);
                    Grid.SetColumn(mc, column);

                    //disable excess controls
                    if (index >= UniMoveController.GetNumConnected())
                    {
                        mc.IsEnabled = false;
                    }
                } //for column
            } //for row
        }

        private void initCamerasGrid()
        {
            //init camera controls
            //maximum 2 columns
            int columns = numCameras >= 2 ? 2 : 1;

            //one row for each pair of cameras
            int rows = (int)((numCameras / 2f) + 0.5f);

            //clear grid
            grid_allCameras.ColumnDefinitions.Clear();
            grid_allCameras.RowDefinitions.Clear();

            for (int row = 0; row < rows; row++)
            {
                RowDefinition rd = new RowDefinition();
                grid_allCameras.RowDefinitions.Add(rd);

                for (int column = 0; column < columns; column++)
                {
                    if (row == 0)
                    {
                        ColumnDefinition cd = new ColumnDefinition();
                        grid_allCameras.ColumnDefinitions.Add(cd);
                    }

                    //init camera control
                    int index = row * columns + column;
                    if (index >= numCameras)
                        break;

                    CameraControl cc = new CameraControl(index);
                    cameraControls.Add(cc);

                    //populate checkBoxList with number of connected moves
                    cc.checkBoxList_moves.init(UniMoveController.GetNumConnected());
                    grid_allCameras.Children.Add(cc);

                    //assign cell
                    Grid.SetRow(cc, row);
                    Grid.SetColumn(cc, column);

                    //disable excess controls
                    if (index >= CLEyeCameraDevice.CameraCount)
                    {
                        cc.IsEnabled = false;
                    }
                } //for column
            } //for row
        }

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

            if (move != null)
            {
                moves.Add(move);

                foreach (CameraControl cc in cameraControls)
                {
                    cc.moves.Add(move);
                }
            }

            //allow tracking
            if (moves.Count > 0)
            {
                //button_initTracking.Enabled = true;
            }
        } //initMove_Click

        private bool checkTrackerAvailability()
        {
            foreach (CameraControl cc in cameraControls)
            {
                if (cc.enabledForTracking)
                {
                    return true;
                }
            }
            return false;
        } //checkTrackerAvailability

        private void enableAvailableTrackers()
        {
            trackers = new List<UniMoveTracker>();

            //enable tracking for each camera control that is enabled for tracking
            foreach (CameraControl cc in cameraControls)
            {
                if (cc.IsEnabled)
                {
                    if (cc.enabledForTracking)
                    {
                        cc.initTracker(moves);
                        trackers.Add(cc.tracker);
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
            foreach (CameraControl cc in cameraControls)
            {
                if (cc.IsEnabled)
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

            //button_initMove.Enabled = false;
            //button_initTracking.Enabled = false;
            //button_stopTracking.Enabled = true;
        } //initTrackers_Click

        private void button_stopTracking_Click(object sender, EventArgs e)
        {
            foreach (CameraControl cc in cameraControls)
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
            //button_stopTracking.Enabled = false;
            //button_initTracking.Enabled = true;
        }//stopTracking_Click
        #endregion [ Control Clicks ]



        #region [ Misc ]
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //e.Cancel = false;

        } //MetroWindow Closing
        #endregion [ Misc ]
    }
}