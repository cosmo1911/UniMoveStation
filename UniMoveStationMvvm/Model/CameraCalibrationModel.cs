using Emgu.CV;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using UniMoveStation.Helper;

namespace UniMoveStation.Model
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    [JsonObject]
    public class CameraCalibrationModel : ViewModelBase
    {
        private double _error;
        private int _frameBufferSize;
        private bool _startFlag;
        private CameraCalibrationMode _currentMode;
        private IntrinsicCameraParameters _intrinsicParameters;
        private ExtrinsicCameraParameters[] _extrinsicParameters;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationModel class.
        /// </summary>
        public CameraCalibrationModel()
        {
            _frameBufferSize = 100;
            _currentMode = CameraCalibrationMode.SavingFrames;
            _intrinsicParameters = new IntrinsicCameraParameters();
        }

        public int FrameBufferSize
        {
            get { return _frameBufferSize; }
            set { Set(() => FrameBufferSize, ref _frameBufferSize, value); }
        }

        public double Error
        {
            get { return _error; }
            set { Set(() => Error, ref _error, value); }
        }

        public bool StartFlag
        {
            get { return _startFlag; }
            set { Set(() => StartFlag, ref _startFlag, value); }
        }

        public CameraCalibrationMode CurrentMode
        {
            get { return _currentMode; }
            set { Set(() => CurrentMode, ref _currentMode, value); }
        }

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
    } // CameraCalibrationModel
} // namespace