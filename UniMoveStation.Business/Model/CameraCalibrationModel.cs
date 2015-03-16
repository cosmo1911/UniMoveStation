using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using UniMoveStation.Common;
using UnityEngine;

namespace UniMoveStation.Business.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CameraCalibrationModel : ViewModelBase
    {
        private string _cameraGuid;
        private double _error;
        private int _frameBufferSize;
        private bool _startFlag;
        private CameraCalibrationMode _currentMode;
        private IntrinsicCameraParameters _intrinsicParameters;
        private ExtrinsicCameraParameters[] _extrinsicParameters;
        private Matrix<double> _rotationToWorld;
        private Matrix<double> _translationToWorld;
        private Vector3 _point;
        private float _xAngle;
        private float _yAngle;
        private float _zAngle;
        private int _index;

        private int _rotX;
        private int _rotY;
        private int _rotZ;

        private List<PointF> _correspondingPoints;
        private MCvPoint3D32f[] _objectPoints2D;
        private PointF[] _objectPointsProjected;
        private MCvPoint3D32f[] _objectPoints3D;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationModel class.
        /// </summary>
        public CameraCalibrationModel()
        {
            _frameBufferSize = 100;
            _currentMode = CameraCalibrationMode.SavingFrames;
            _intrinsicParameters = new IntrinsicCameraParameters();
            _extrinsicParameters = new ExtrinsicCameraParameters[5];
            _translationToWorld = new Matrix<double>(3, 1);
            _rotationToWorld = new Matrix<double>(3, 3);
            _correspondingPoints = new List<PointF>();
        }

        /// <summary>
        /// identifier of the camera to which this calibration belongs
        /// </summary>
        [JsonProperty]
        public string CameraGuid
        {
            get { return _cameraGuid; }
            set { Set(() => CameraGuid, ref _cameraGuid, value); }
        }

        /// <summary>
        /// debug purposes only
        /// </summary>
        public int RotX
        {
            get { return _rotX; }
            set { Set(() => RotX, ref _rotX, value); }
        }

        /// <summary>
        /// debug purposes only
        /// </summary>
        public int RotY
        {
            get { return _rotY; }
            set { Set(() => RotY, ref _rotY, value); }
        }

        /// <summary>
        /// debug purposes only
        /// </summary>
        public int RotZ
        {
            get { return _rotZ; }
            set { Set(() => RotZ, ref _rotZ, value); }
        }

        /// <summary>
        /// auxiliary rectangle surrounding the motion controller's sphere
        /// </summary>
        public MCvPoint3D32f[] ObjectPoints2D
        {
            get
            {
                if (_objectPoints2D == null)
                {
                    const float radiusCm = (int)((13.5 / Math.PI) * 100) / 200f;
                    const float diameterCm = (int)((13.5 / Math.PI) * 100) / 100f;

                    _objectPoints2D = new MCvPoint3D32f[4];
                    _objectPoints2D[0] = new MCvPoint3D32f(-radiusCm, -radiusCm, 0);
                    _objectPoints2D[1] = new MCvPoint3D32f(radiusCm, -radiusCm, 0);
                    _objectPoints2D[2] = new MCvPoint3D32f(radiusCm, radiusCm, 0);
                    _objectPoints2D[3] = new MCvPoint3D32f(-radiusCm, radiusCm, 0);

                    //_objectPoints2d[0] = new MCvPoint3D32f(-radiusCm, 0, -radiusCm);
                    //_objectPoints2d[1] = new MCvPoint3D32f(radiusCm, 0, -radiusCm);
                    //_objectPoints2d[2] = new MCvPoint3D32f(radiusCm, 0, radiusCm);
                    //_objectPoints2d[3] = new MCvPoint3D32f(-radiusCm, 0, radiusCm);

                    //_objectPoints2d[0] = new MCvPoint3D32f(0, 0, 0);
                    //_objectPoints2d[1] = new MCvPoint3D32f(diameterCm, 0, 0);
                    //_objectPoints2d[2] = new MCvPoint3D32f(diameterCm, diameterCm, 0);
                    //_objectPoints2d[3] = new MCvPoint3D32f(0, diameterCm, 0);

                    return _objectPoints2D;
                }
                return _objectPoints2D;
            }
            set { Set(() => ObjectPoints2D, ref _objectPoints2D, value); }
        }

        /// <summary>
        /// auxiliary cube surrounding the motion controller's sphere
        /// </summary>
        public MCvPoint3D32f[] ObjectPoints3D
        {
            get
            {
                if (_objectPoints3D == null)
                {
                    const float radiusCm = (int)((13.5 / Math.PI) * 100) / 200f;
                    const float diameterCm = (int)((13.5 / Math.PI) * 100) / 100f;

                    _objectPoints3D = new MCvPoint3D32f[8];
                    _objectPoints3D[0] = new MCvPoint3D32f(-radiusCm, -radiusCm, -radiusCm);
                    _objectPoints3D[1] = new MCvPoint3D32f(radiusCm, -radiusCm, -radiusCm);
                    _objectPoints3D[2] = new MCvPoint3D32f(radiusCm, radiusCm, -radiusCm);
                    _objectPoints3D[3] = new MCvPoint3D32f(-radiusCm, radiusCm, -radiusCm);

                    _objectPoints3D[4] = new MCvPoint3D32f(-radiusCm, radiusCm, radiusCm);
                    _objectPoints3D[5] = new MCvPoint3D32f(radiusCm, radiusCm, radiusCm);
                    _objectPoints3D[6] = new MCvPoint3D32f(radiusCm, -radiusCm, radiusCm);
                    _objectPoints3D[7] = new MCvPoint3D32f(-radiusCm, -radiusCm, radiusCm);

                    //_objectPoints3d[0] = new MCvPoint3D32f(0, 0, 0);
                    //_objectPoints3d[1] = new MCvPoint3D32f(diameterCm, 0, 0);
                    //_objectPoints3d[2] = new MCvPoint3D32f(diameterCm, diameterCm, 0);
                    //_objectPoints3d[3] = new MCvPoint3D32f(0, diameterCm, 0);

                    //_objectPoints3d[4] = new MCvPoint3D32f(0, diameterCm, diameterCm);
                    //_objectPoints3d[5] = new MCvPoint3D32f(diameterCm, diameterCm, diameterCm);
                    //_objectPoints3d[6] = new MCvPoint3D32f(diameterCm, 0, diameterCm);
                    //_objectPoints3d[7] = new MCvPoint3D32f(0, 0, diameterCm);

                    return _objectPoints3D;
                }
                return _objectPoints3D;
            }
            set { Set(() => ObjectPoints3D, ref _objectPoints3D, value); }
        }

        /// <summary>
        /// projection of the auxiliary cube onto the image plane
        /// </summary>
        public PointF[] ObjectPointsProjected
        {
            get { return _objectPointsProjected; }
            set { Set(() => ObjectPointsProjected, ref _objectPointsProjected, value); }
        }

        /// <summary>
        /// result of the bundle adjustment
        /// </summary>
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

        /// <summary>
        /// index / position of the camera in the camera array
        /// in case of four cameras total and a rectangular positioning, camera 0 is diagonally opposite of camera 2
        /// and subsequently camera 1 is diagonally across from camera 3.
        /// </summary>
        [JsonProperty]
        public int Index
        {
            get { return _index; }
            set { Set(() => Index, ref _index, value); }
        }

        /// <summary>
        /// euler rotation around the X axis of the camera coordinate frame towards the world coordinate frame
        /// </summary>
        [JsonProperty]
        public float XAngle
        {
            get { return _xAngle; }
            set { Set(() => XAngle, ref _xAngle, value); }
        }

        /// <summary>
        /// euler rotation around the Y axis of the camera coordinate frame towards the world coordinate frame
        /// </summary>
        [JsonProperty]
        public float YAngle
        {
            get { return _yAngle; }
            set { Set(() => YAngle, ref _yAngle, value); }
        }

        /// <summary>
        /// euler rotation around the Z axis of the camera coordinate frame towards the world coordinate frame
        /// </summary>
        [JsonProperty]
        public float ZAngle
        {
            get { return _zAngle; }
            set { Set(() => ZAngle, ref _zAngle, value); }
        }

        /// <summary>
        /// indicates how many pictures are captured during camera calibration
        /// </summary>
        [JsonProperty]
        public int FrameBufferSize
        {
            get { return _frameBufferSize; }
            set { Set(() => FrameBufferSize, ref _frameBufferSize, value); }
        }

        /// <summary>
        /// camera calibration error
        /// </summary>
        [JsonProperty]
        public double Error
        {
            get { return _error; }
            set { Set(() => Error, ref _error, value); }
        }

        /// <summary>
        /// rotation of the camera coordinate frame towards the world coordinate frame
        /// </summary>
        [JsonProperty]
        public Matrix<double> RotationToWorld
        {
            get { return _rotationToWorld; }
            set { Set(() => RotationToWorld, ref _rotationToWorld, value); }
        }

        /// <summary>
        /// position of the camera in the world coordinate frame
        /// </summary>
        [JsonProperty]
        public Matrix<double> TranslationToWorld
        {
            get { return _translationToWorld; }
            set { Set(() => TranslationToWorld, ref _translationToWorld, value); }
        }

        /// <summary>
        /// indiciates if the camera calibration is currently running
        /// </summary>
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

        public override string ToString()
        {
            return CameraGuid;
        }
    } // CameraCalibrationModel
} // namespace