using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
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
        private float _xAngle;
        private float _yAngle;
        private int _position;

        private int _rotX;
        private int _rotY;
        private int _rotZ;

        private List<PointF> _correspondingPoints;
        private MCvPoint3D32f[] _objectPoints2d;
        private PointF[] _objectPointsProjected;
        private MCvPoint3D32f[] _objectPoints3d;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationModel class.
        /// </summary>
        public CameraCalibrationModel()
        {
            _frameBufferSize = 100;
            _currentMode = CameraCalibrationMode.SavingFrames;
            _intrinsicParameters = new IntrinsicCameraParameters();
            _extrinsicParameters = new ExtrinsicCameraParameters[5];
            _translationVector = new Matrix<double>(3, 1);
            _rotationMatrix = new Matrix<double>(3, 3);
            _correspondingPoints = new List<PointF>();
        }

        public int RotX
        {
            get { return _rotX; }
            set { Set(() => RotX, ref _rotX, value); }
        }

        public int RotY
        {
            get { return _rotY; }
            set { Set(() => RotY, ref _rotY, value); }
        }

        public int RotZ
        {
            get { return _rotZ; }
            set { Set(() => RotZ, ref _rotZ, value); }
        }

        public MCvPoint3D32f[] ObjectPoints2D
        {
            get
            {
                if (_objectPoints2d == null)
                {
                    float radiusCm = (float)((int)((14.0 / Math.PI) * 100)) / 200f;
                    float diameterCm = (float)((int)((14.0 / Math.PI) * 100)) / 100f;

                    _objectPoints2d = new MCvPoint3D32f[4];
                    _objectPoints2d[0] = new MCvPoint3D32f(-radiusCm, -radiusCm, 0);
                    _objectPoints2d[1] = new MCvPoint3D32f(radiusCm, -radiusCm, 0);
                    _objectPoints2d[2] = new MCvPoint3D32f(radiusCm, radiusCm, 0);
                    _objectPoints2d[3] = new MCvPoint3D32f(-radiusCm, radiusCm, 0);

                    //_objectPoints2d[0] = new MCvPoint3D32f(-radiusCm, 0, -radiusCm);
                    //_objectPoints2d[1] = new MCvPoint3D32f(radiusCm, 0, -radiusCm);
                    //_objectPoints2d[2] = new MCvPoint3D32f(radiusCm, 0, radiusCm);
                    //_objectPoints2d[3] = new MCvPoint3D32f(-radiusCm, 0, radiusCm);

                    //_objectPoints2d[0] = new MCvPoint3D32f(0, 0, 0);
                    //_objectPoints2d[1] = new MCvPoint3D32f(diameterCm, 0, 0);
                    //_objectPoints2d[2] = new MCvPoint3D32f(diameterCm, diameterCm, 0);
                    //_objectPoints2d[3] = new MCvPoint3D32f(0, diameterCm, 0);

                    return _objectPoints2d;
                }
                else
                {
                    return _objectPoints2d;
                }
            }
            set { Set(() => ObjectPoints2D, ref _objectPoints2d, value); }
        }

        public MCvPoint3D32f[] ObjectPoints3D
        {
            get
            {
                if (_objectPoints3d == null)
                {
                    float radiusCm = (float)((int)((14.0 / Math.PI) * 100)) / 200f;
                    float diameterCm = (float)((int)((14.0 / Math.PI) * 100)) / 100f;

                    _objectPoints3d = new MCvPoint3D32f[8];
                    _objectPoints3d[0] = new MCvPoint3D32f(-radiusCm, -radiusCm, -radiusCm);
                    _objectPoints3d[1] = new MCvPoint3D32f(radiusCm, -radiusCm, -radiusCm);
                    _objectPoints3d[2] = new MCvPoint3D32f(radiusCm, radiusCm, -radiusCm);
                    _objectPoints3d[3] = new MCvPoint3D32f(-radiusCm, radiusCm, -radiusCm);

                    _objectPoints3d[4] = new MCvPoint3D32f(-radiusCm, radiusCm, radiusCm);
                    _objectPoints3d[5] = new MCvPoint3D32f(radiusCm, radiusCm, radiusCm);
                    _objectPoints3d[6] = new MCvPoint3D32f(radiusCm, -radiusCm, radiusCm);
                    _objectPoints3d[7] = new MCvPoint3D32f(-radiusCm, -radiusCm, radiusCm);

                    //_objectPoints3d[0] = new MCvPoint3D32f(0, 0, 0);
                    //_objectPoints3d[1] = new MCvPoint3D32f(diameterCm, 0, 0);
                    //_objectPoints3d[2] = new MCvPoint3D32f(diameterCm, diameterCm, 0);
                    //_objectPoints3d[3] = new MCvPoint3D32f(0, diameterCm, 0);

                    //_objectPoints3d[4] = new MCvPoint3D32f(0, diameterCm, diameterCm);
                    //_objectPoints3d[5] = new MCvPoint3D32f(diameterCm, diameterCm, diameterCm);
                    //_objectPoints3d[6] = new MCvPoint3D32f(diameterCm, 0, diameterCm);
                    //_objectPoints3d[7] = new MCvPoint3D32f(0, 0, diameterCm);

                    return _objectPoints3d;
                }
                else
                {
                    return _objectPoints3d;
                }
            }
            set { Set(() => ObjectPoints3D, ref _objectPoints3d, value); }
        }

        public PointF[] ObjectPointsProjected
        {
            get { return _objectPointsProjected; }
            set { Set(() => ObjectPointsProjected, ref _objectPointsProjected, value); }
        }

        public Vector3 Point
        {
            get { return _point; }
            set { Set(() => Point, ref _point, value); }
        }

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
        public float XAngle
        {
            get { return _xAngle; }
            set { Set(() => XAngle, ref _xAngle, value); }
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