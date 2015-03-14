using System;
using System.Collections.Concurrent;
using GalaSoft.MvvmLight;
using UniMoveStation.Common;
using UniMoveStation.Business.PsMove;
using UnityEngine;

namespace UniMoveStation.Business.Model
{
    public class MotionControllerModel : ObservableObject
    {
        private bool _design;
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
        public static int COUNTER = 0;

        public MotionControllerModel()
        {
            Orientation = new Quaternion();
            Color = Color.blue;
        }
#endif

        public bool Design
        {
            get { return _design; }
            set { Set(() => Design, ref _design, value); }
        }

        /// <summary>
        /// positions of the motion controllers in the image coordinate frame
        /// 
        /// x = 0..640
        /// y = 0..480
        /// z = sphere's radius in pixels
        /// </summary>
        public ObservableConcurrentDictionary<CameraModel, Vector3> RawPosition
        {
            get { return _rawPosition ?? (_rawPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>()); }
            set { Set(() => RawPosition, ref _rawPosition, value); }
        }

        /// <summary>
        /// positions calculated by sensor fusion with the ps move api
        /// </summary>
        public ObservableConcurrentDictionary<CameraModel, Vector3> FusionPosition
        {
            get { return _fusionPosition ?? (_fusionPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>()); }
            set { Set(() => FusionPosition, ref _fusionPosition, value); }
        }

        /// <summary>
        /// positions of the motion controllers in the camera coordinate frame, i.e. origin 
        /// of the coordinate frame in the center of the image. 
        /// 
        /// x, y and z (depth) given in centimeters
        /// </summary>
        public ObservableConcurrentDictionary<CameraModel, Vector3> CameraPosition
        {
            get { return _cameraPosition ?? (_cameraPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>()); }
            set { Set(() => CameraPosition, ref _cameraPosition, value); }
        }

        /// <summary>
        /// positions of the motion controllers in the world coordinate frame
        /// 
        /// x, y and z (depth) given in centimeters
        /// </summary>
        public ObservableConcurrentDictionary<CameraModel, Vector3> WorldPosition
        {
            get { return _worldPosition ?? (_worldPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>()); }
            set { Set(() => WorldPosition, ref _worldPosition, value); }
        }
        
        /// <summary>
        /// which cameras are tracking this controller?
        /// </summary>
        public ObservableConcurrentDictionary<CameraModel, bool> Tracking
        {
            get { return _tracking ?? (_tracking = new ObservableConcurrentDictionary<CameraModel, bool>()); }
            set { Set(() => Tracking, ref _tracking, value); }
        }

        /// <summary>
        /// whats the tracking status of this controller for each camera?
        /// </summary>
        public ObservableConcurrentDictionary<CameraModel, PSMoveTrackerStatus> TrackerStatus
        {
            get { return _trackerStatus ?? (_trackerStatus = new ObservableConcurrentDictionary<CameraModel, PSMoveTrackerStatus>()); }
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

        public override string ToString()
        {
            return Name;
        }
    } // MotionControllerModel
} // namespace
