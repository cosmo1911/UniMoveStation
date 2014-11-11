using UniMove;
using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace UniMoveStation.Model
{
    
    public class SingleCameraModel : ObservableObject
    {
        private UniMoveTracker _tracker;
        private List<UniMoveController> _moves;
        private CameraState _state;
        private bool _annotate;

        public enum CameraState
        {
            CLEye_Tracking,
            PSMove_Tracking,
            none
        }



        public bool Annotate
        {
            get
            {
                return _annotate;
            }
            set
            {
                Set(() => Annotate, ref _annotate, value);
            }
        }

        
        public UniMoveTracker Tracker
        {
            get 
            {
                return _tracker;
            }
            set
            {
                Set(() => Tracker, ref _tracker, value);
            }
        }

        public List<UniMoveController> Moves
        {
            get
            {
                return _moves;
            }
            set
            {
                Set(() => Moves, ref _moves, value);
            }
        }

        public CameraState State
        {
            get
            {
                return _state;
            }
            set
            {
                Set(() => State, ref _state, value);
            }
        }
    }
}
