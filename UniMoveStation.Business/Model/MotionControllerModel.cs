using System;
using System.Collections.Concurrent;
using GalaSoft.MvvmLight;
using UniMoveStation.Common.SharpMove;
using UnityEngine;

namespace UniMoveStation.Business.Model
{
    public class MotionControllerModel : ObservableObject
    {
        protected static float MIN_UPDATE_RATE = 0.02f;

        private int _id;
        private IntPtr _handle;
        private string _name;
        private string _serial;

        private Color _color;
        private int _rumble;
        private float _temperature;

        private PSMoveBatteryLevel _batteryLevel;
        private PSMoveConnectStatus _connectStatus = PSMoveConnectStatus.Unknown;
        private PSMoveConnectionType _connectionType = PSMoveConnectionType.Unknown;
        private string _hostIp = "Unknown";
        private bool _remote;
        private float _updateRate = 0.05f;

        #region Buttons
        private bool _circle;
        private bool _cross;
        private bool _triangle;
        private bool _square;
        private bool _move;
        private bool _ps;
        private bool _start;
        private bool _select;
        private int _trigger;
        #endregion

        #region Inertial Data
        private bool _oriented;
        private Quaternion _orientation;
        private Vector3 _rawAccelerometer;
        private Vector3 _accelerometer;
        private Vector3 _rawGyroscope;
        private Vector3 _gyroscope;
        private Vector3 _magnetometer;
        #endregion

        #region Tracker
        private ObservableConcurrentDictionary<CameraModel, bool> _tracking;
        private ObservableConcurrentDictionary<CameraModel, PSMoveTrackerStatus> _trackerStatus;
        private ObservableConcurrentDictionary<CameraModel, Vector3> _rawPosition;
        private ObservableConcurrentDictionary<CameraModel, Vector3> _fusionPosition;
        private ObservableConcurrentDictionary<CameraModel, Vector3> _cameraPosition;
        private ObservableConcurrentDictionary<CameraModel, Vector3> _worldPosition;
        #endregion

#if DEBUG
        private static int COUNTER = -1;

        public MotionControllerModel()
        {
            Id = COUNTER--;
            Name = "Design " + Id;
            Serial = "00:00:00:00:00:0" + Id;
            System.Random random = new System.Random();
            Circle = random.Next(2) > 0;
            Cross = random.Next(2) > 0;
            Triangle = random.Next(2) > 0;
            Square = random.Next(2) > 0;
            Start = random.Next(2) > 0;
            Select = random.Next(2) > 0;
            Move = random.Next(2) > 0;
            PS = random.Next(2) > 0;
            Trigger = random.Next(256);
            Orientation = new Quaternion();

            Color = Color.blue;
        }
#endif

        public ObservableConcurrentDictionary<CameraModel, Vector3> RawPosition
        {
            get 
            { 
                if(_rawPosition == null)
                {
                    _rawPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>();
                    // TODO add already existing cameras
                    //foreach(CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                    //{
                    //    _rawPosition.Add(scvm.Camera, Vector3.zero);
                    //}
                }
                return _rawPosition; 
            }
            set { Set(() => RawPosition, ref _rawPosition, value); }
        }

        public ObservableConcurrentDictionary<CameraModel, Vector3> FusionPosition
        {
            get
            {
                if (_fusionPosition == null)
                {
                    _fusionPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>();
                    // TODO add already existing cameras
                    //foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                    //{
                    //    _fusionPosition.Add(scvm.Camera, Vector3.zero);
                    //}
                }
                return _fusionPosition;
            }
            set { Set(() => FusionPosition, ref _fusionPosition, value); }
        }

        public ObservableConcurrentDictionary<CameraModel, Vector3> CameraPosition
        {
            get
            {
                if (_cameraPosition == null)
                {
                    _cameraPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>();
                    // TODO add already existing cameras
                    //foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                    //{
                    //    _cameraPosition.Add(scvm.Camera, Vector3.zero);
                    //}
                }
                return _cameraPosition;
            }
            set { Set(() => CameraPosition, ref _cameraPosition, value); }
        }

        public ObservableConcurrentDictionary<CameraModel, Vector3> WorldPosition
        {
            get
            {
                if (_worldPosition == null)
                {
                    _worldPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>();
                    // TODO add already existing cameras
                    //foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                    //{
                    //    _worldPosition.Add(scvm.Camera, Vector3.zero);
                    //}
                }
                return _worldPosition;
            }
            set { Set(() => WorldPosition, ref _worldPosition, value); }
        }

        public ObservableConcurrentDictionary<CameraModel, bool> Tracking
        {
            get
            {
                if (_tracking == null)
                {
                    _tracking = new ObservableConcurrentDictionary<CameraModel, bool>();
                    // TODO add already existing cameras
                    //foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                    //{
                    //    _tracking.Add(scvm.Camera, false);
                    //}
                }
                return _tracking; 
            }
            set { Set(() => Tracking, ref _tracking, value); }
        }

        public ObservableConcurrentDictionary<CameraModel, PSMoveTrackerStatus> TrackerStatus
        {
            get
            {
                if (_trackerStatus == null)
                {
                    _trackerStatus = new ObservableConcurrentDictionary<CameraModel, PSMoveTrackerStatus>();
                    // TODO add already existing cameras
                    //foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllInstances<CameraViewModel>())
                    //{
                    //    _trackerStatus.Add(scvm.Camera, PSMoveTrackerStatus.NotCalibrated);
                    //}
                }
                return _trackerStatus; 
            }
            set { Set(() => TrackerStatus, ref _trackerStatus, value); }
        }

        public IntPtr Handle
        {
            get { return _handle; }
            set { Set(() => Handle, ref _handle, value); }
        }

        public bool Oriented
        {
            get { return _oriented; }
            set { Set(() => Oriented, ref _oriented, value); }
        }

        /// <summary>
        /// The 3-axis acceleration values. 
        /// </summary>
        public Vector3 RawAccelerometer
        {
            get { return _rawAccelerometer; }
            set { Set(() => RawAccelerometer, ref _rawAccelerometer, value); }
        }

        /// <summary>
        /// The 3-axis acceleration values, roughly scaled between -3g to 3g (where 1g is Earth's gravity).
        /// </summary>
        public Vector3 Accelerometer
        {
            get { return _accelerometer; }
            set { Set(() => Accelerometer, ref _accelerometer, value); }
        }

        /// <summary>
        /// The raw values of the 3-axis gyroscope. 
        /// </summary>
        public Vector3 RawGyroscope
        {
            get { return _rawGyroscope; }
            set { Set(() => RawGyroscope, ref _rawGyroscope, value); }
        }

        /// <summary>
        /// The raw values of the 3-axis gyroscope. 
        /// </summary>
        public Vector3 Gyroscope
        {
            get { return _gyroscope; }
            set { Set(() => Gyroscope, ref _gyroscope, value); }

        }

        /// <summary>
        /// The raw values of the 3-axis magnetometer.
        /// To be honest, we don't fully understand what the magnetometer does.
        /// The C API on which this code is based warns that this isn't fully tested.
        /// </summary>
        public Vector3 Magnetometer
        {
            get { return _magnetometer; }
            set { Set(() => Magnetometer, ref _magnetometer, value); }
        }

        /// <summary>
        /// The temperature in Celcius
        /// </summary>
        public float Temperature
        {
            get { return _temperature; }
            set { Set(() => Temperature, ref _temperature, value); }
        }

        public Quaternion Orientation
        {
            get { return _orientation; }
            set { Set(() => Orientation, ref _orientation, value); }
        }

        public Vector3 Up
        {
            get { return Orientation * Vector3.up; }
        }

        public Vector3 Forward
        {
            get { return Orientation * Vector3.forward; }
        }

        public Vector3 Right
        {
            get { return Orientation * Vector3.right; }
        }

        public PSMoveConnectStatus ConnectStatus
        {
            get { return _connectStatus; }
            set { Set(() => ConnectStatus, ref _connectStatus, value); }
        }

        public PSMoveBatteryLevel BatteryLevel
        {
            get { return _batteryLevel; }
            set { Set(() => BatteryLevel, ref _batteryLevel, value); }
        }

        public PSMoveConnectionType ConnectionType
        {
            get { return _connectionType; }
            set { Set(() => ConnectionType, ref _connectionType, value); }
        }

        public string HostIp
        {
            get { return _hostIp; }
            set { Set(() => HostIp, ref _hostIp, value); }
        }

        public bool Remote
        {
            get { return _remote; }
            set { Set(() => Remote, ref _remote, value); }
        }

        /// <summary>
        /// Bluetooth MAC Address
        /// </summary>
        public string Serial
        {
            get { return _serial; }
            set { Set(() => Serial, ref _serial, value); }
        }

        /// <summary>
        /// The amount of time, in seconds, between update calls.
        /// The faster this rate, the more responsive the controllers will be.
        /// However, update too fast and your computer won't be able to keep up (see below).
        /// You almost certainly don't want to make this faster than 20 milliseconds (0.02f).
        /// 
        /// NOTE! We find that slower/older computers can have trouble keeping up with a fast update rate,
        /// especially the more controllers that are connected. See the README for more information.
        /// </summary>
        public float UpdateRate
        {
            get { return _updateRate; }
            set { Set(() => UpdateRate, ref _updateRate, Math.Max(value, MIN_UPDATE_RATE)); }
        }

        /// <summary>
        /// 0..1
        /// </summary>
        public Color Color
        {
            get { return _color; }
            set { Set(() => Color, ref _color, value); }
        }
        
        public int Id
        {
            get { return _id; }
            set { Set(() => Id, ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(() => Name, ref _name, value); }
        }

        public bool Circle
        {
            get { return _circle; }
            set { Set(() => Circle, ref _circle, value); }
        }

        public bool Cross
        {
            get { return _cross; }
            set { Set(() => Cross, ref _cross, value); }
        }

        public bool Triangle
        {
            get { return _triangle; }
            set { Set(() => Triangle, ref _triangle, value); }
        }

        public bool Square
        {
            get { return _square; }
            set { Set(() => Square, ref _square, value); }
        }

        public bool Move
        {
            get { return _move; }
            set { Set(() => Move, ref _move, value); }
        }

        public bool PS
        {
            get { return _ps; }
            set { Set(() => PS, ref _ps, value); }
        }

        public bool Start
        {
            get { return _start; }
            set { Set(() => Start, ref _start, value); }
        }

        public bool Select
        {
            get { return _select; }
            set { Set(() => Select, ref _select, value); }
        }

        /// <summary>
        /// 0..255
        /// </summary>
        public int Trigger
        {
            get { return _trigger; }
            set { Set(() => Trigger, ref _trigger, value); }
        }

        /// <summary>
        /// TODO 0..1 => 0..255?
        /// </summary>
        public int Rumble
        {
            get { return _rumble; }
            set { Set(() => Rumble, ref _rumble, value); }
        }
    } // MotionControllerModel
} // namespace
