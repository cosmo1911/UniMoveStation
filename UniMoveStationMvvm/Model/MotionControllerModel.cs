using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniMove;
using UnityEngine;

namespace UniMoveStation.Model
{
    public class MotionControllerModel : ObservableObject
    {

        private int _id;
        private string _name;

        private Color _color;
        private int _rumble;
        private float _temperature;

        private PSMoveBatteryLevel _batteryLevel;
        private PSMoveConnectStatus _connectStatus = PSMoveConnectStatus.Unknown;
        private PSMoveConnectionType _connectionType = PSMoveConnectionType.Unknown;
        private string _hostIp = "Unknown";
        private bool _remote;
        private float _updateRate;

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
        private Vector3 _rawAcceleration;
        private Vector3 _acceleration;
        private Vector3 _rawGyroscope;
        private Vector3 _gyroscope;
        private Vector3 _magnetometer;
        #endregion

#if DEBUG
        private static int COUNTER = -1;

        public MotionControllerModel()
        {
            Name = "Design";
            Id = ++COUNTER;
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
            Rumble = random.Next(256);

            Color = UnityEngine.Color.green;
            Select = true;
        }
#endif

        public bool Oriented
        {
            get
            {
                return _oriented;
            }
            set
            {
                Set(() => Oriented, ref _oriented, value);
            }
        }

        /// <summary>
        /// The 3-axis acceleration values. 
        /// </summary>
        public Vector3 RawAcceleration
        {
            get
            {
                return _rawAcceleration;
            }
            set
            {
                Set(() => RawAcceleration, ref _rawAcceleration, value);
            }
        }

        /// <summary>
        /// The 3-axis acceleration values, roughly scaled between -3g to 3g (where 1g is Earth's gravity).
        /// </summary>
        public Vector3 Acceleration
        {
            get 
            { 
                return _acceleration; 
            }
            set
            {
                Set(() => Acceleration, ref _acceleration, value);
            }
        }

        /// <summary>
        /// The raw values of the 3-axis gyroscope. 
        /// </summary>
        public Vector3 RawGyroscope
        {
            get 
            { 
                return _rawGyroscope; 
            }
            set
            {
                Set(() => RawGyroscope, ref _rawGyroscope, value);
            }
        }
        /// <summary>
        /// The raw values of the 3-axis gyroscope. 
        /// </summary>
        public Vector3 Gyroscope
        {
            get 
            { 
                return _gyroscope; 
            }
            set
            {
                Set(() => Gyroscope, ref _gyroscope, value);
            }

        }

        /// <summary>
        /// The raw values of the 3-axis magnetometer.
        /// To be honest, we don't fully understand what the magnetometer does.
        /// The C API on which this code is based warns that this isn't fully tested.
        /// </summary>
        public Vector3 Magnetometer
        {
            get 
            { 
                return _magnetometer; 
            }
            set
            {
                Set(() => Magnetometer, ref _magnetometer, value);
            }
        }

        /// <summary>
        /// The temperature in Celcius
        /// </summary>
        public float Temperature
        {
            get 
            {
                return _temperature; 
            }
            set
            {
                Set(() => Temperature, ref _temperature, value);
            }
        }

        public Quaternion Orientation
        {
            get 
            { 
                return _orientation; 
            }
            set
            {
                Set(() => Orientation, ref _orientation, value);
            }
        }

        public Vector3 Up
        {
            get 
            { 
                return Orientation * Vector3.up; 
            }
        }

        public Vector3 Forward
        {
            get 
            { 
                return Orientation * Vector3.forward; 
            }
        }

        public Vector3 Right
        {
            get 
            { 
                return Orientation * Vector3.right; 
            }
        }

        public PSMoveConnectStatus ConnectStatus
        {
            get
            {
                return _connectStatus;
            }
            set
            {
                Set(() => ConnectStatus, ref _connectStatus, value);
            }
        }

        public PSMoveBatteryLevel BatteryLevel
        {
            get
            {
                return _batteryLevel;
            }
            set
            {
                Set(() => BatteryLevel, ref _batteryLevel, value);
            }
        }

        public PSMoveConnectionType ConnectionType
        {
            get
            {
                return _connectionType;
            }
            set
            {
                Set(() => ConnectionType, ref _connectionType, value);
            }
        }

        public string HostIp
        {
            get
            {
                return _hostIp;
            }
            set
            {
                Set(() => HostIp, ref _hostIp, value);
            }
        }

        public bool Remote
        {
            get
            {
                return _remote;
            }
            set
            {
                Set(() => Remote, ref _remote, value);
            }
        }

        public string Serial
        {
            get;
            set;
        }

        public float UpdateRate
        {
            get
            {
                return _updateRate;
            }
            set
            {
                Set(() => UpdateRate, ref _updateRate, value);
            }
        }

        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                Set(() => Color, ref _color, value);
            }
        }
        
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                Set(() => Id, ref _id, value);
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                Set(() => Name, ref _name, value);
            }
        }

        public bool Circle
        {
            get
            {
                return _circle;
            }
            set
            {
                Set(() => Circle, ref _circle, value);
            }
        }

        public bool Cross
        {
            get
            {
                return _cross;
            }
            set
            {
                Set(() => Cross, ref _cross, value);
            }
        }

        public bool Triangle
        {
            get
            {
                return _triangle;
            }
            set
            {
                Set(() => Triangle, ref _triangle, value);
            }
        }

        public bool Square
        {
            get
            {
                return _square;
            }
            set
            {
                Set(() => Square, ref _square, value);
            }
        }

        public bool Move
        {
            get
            {
                return _move;
            }
            set
            {
                Set(() => Move, ref _move, value);
            }
        }

        public bool PS
        {
            get
            {
                return _ps;
            }
            set
            {
                Set(() => PS, ref _ps, value);
            }
        }

        public bool Start
        {
            get
            {
                return _start;
            }
            set
            {
                Set(() => Start, ref _start, value);
            }
        }

        public bool Select
        {
            get
            {
                return _select;
            }
            set
            {
                Set(() => Select, ref _select, value);
            }
        }

        public int Trigger
        {
            get
            {
                return _trigger;
            }
            set
            {
                Set(() => Trigger, ref _trigger, value);
            }
        }

        public int Rumble
        {
            get
            {
                return _rumble;
            }
            set
            {
                Set(() => Rumble, ref _rumble, value);
            }
        }
    }
}
