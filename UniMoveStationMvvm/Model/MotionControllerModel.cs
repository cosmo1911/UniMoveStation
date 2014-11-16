using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UniMoveStation.Model
{
    public class MotionControllerModel : ObservableObject
    {
        private int _id;
        private string _name;
        private bool _circle;
        private bool _cross;
        private bool _triangle;
        private bool _square;
        private bool _move;
        private bool _ps;
        private bool _start;
        private bool _select;
        private int _trigger;
        private int _rumble;
        private Color _color;
        private float _updateRate;
        private bool _remote;
        private string _hostIp;

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
