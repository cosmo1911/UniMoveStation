using Emgu.CV;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using UniMoveStation.Helper;
using UnityEngine;

namespace UniMoveStation.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CameraCalibrationModel : ViewModelBase
    {
        private double _error;
        private int _frameBufferSize;
        private bool _startFlag;
        private CameraCalibrationMode _currentMode;
        private IntrinsicCameraParameters _intrinsicParameters;
        private ExtrinsicCameraParameters[] _extrinsicParameters;
        private Matrix<double> _rotationMatrix;
        private Matrix<double> _translationVector;
        private Vector3 _point;
        private float _yAngle;
        private int _position;
        private List<PointF> _correspondingPoints;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationModel class.
        /// </summary>
        public CameraCalibrationModel()
        {
            _frameBufferSize = 100;
            _currentMode = CameraCalibrationMode.SavingFrames;
            _intrinsicParameters = new IntrinsicCameraParameters();
            _translationVector = new Matrix<double>(3, 1);
            _rotationMatrix = new Matrix<double>(3, 3);
            _correspondingPoints = new List<PointF>();
        }

        public Vector3 Point
        {
            get { return _point; }
            set { Set(() => Point, ref _point, value); }
        }

        [JsonProperty]
        public List<PointF> CorrespondingPoints
        {
            get { return _correspondingPoints; }
            set { Set(() => CorrespondingPoints, ref _correspondingPoints, value); }
        }

        [JsonProperty]
        public int Position
        {
            get { return _position; }
            set { Set(() => Position, ref _position, value); }
        }

        [JsonProperty]
        public float YAngle
        {
            get { return _yAngle; }
            set { Set(() => YAngle, ref _yAngle, value); }
        }

        [JsonProperty]
        public int FrameBufferSize
        {
            get { return _frameBufferSize; }
            set { Set(() => FrameBufferSize, ref _frameBufferSize, value); }
        }

        [JsonProperty]
        public double Error
        {
            get { return _error; }
            set { Set(() => Error, ref _error, value); }
        }

        [JsonProperty]
        public Matrix<double> RotationMatrix
        {
            get { return _rotationMatrix; }
            set { Set(() => RotationMatrix, ref _rotationMatrix, value); }
        }

        [JsonProperty]
        public Matrix<double> TranslationVector
        {
            get { return _translationVector; }
            set { Set(() => TranslationVector, ref _translationVector, value); }
        }

        public bool StartFlag
        {
            get { return _startFlag; }
            set { Set(() => StartFlag, ref _startFlag, value); }
        }

        [JsonProperty]
        public CameraCalibrationMode CurrentMode
        {
            get { return _currentMode; }
            set { Set(() => CurrentMode, ref _currentMode, value); }
        }
        
        [JsonProperty]
        public IntrinsicCameraParameters IntrinsicParameters
        {
            get { return _intrinsicParameters; }
            set { Set(() => IntrinsicParameters, ref _intrinsicParameters, value); }
        }
        
        [JsonProperty]
        public ExtrinsicCameraParameters[] ExtrinsicParameters
        {
            get { return _extrinsicParameters; }
            set { Set(() => ExtrinsicParameters, ref _extrinsicParameters, value); }
        }

        [JsonProperty]
        public string Type
        {
            get { return "CameraCalibrationModel"; }
        }
    } // CameraCalibrationModel
} // namespace