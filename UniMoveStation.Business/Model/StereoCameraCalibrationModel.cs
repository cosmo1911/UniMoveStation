using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using UniMoveStation.Common;
using UniMoveStation.Common.Utils;

namespace UniMoveStation.Business.Model
{
    public class StereoCameraCalibrationModel : ObservableObject
    {
        private CameraModel _camera1;
        private CameraModel _camera2;

        private bool _startFlag;
        private CameraCalibrationMode _currentMode;
        private int _frameBufferSize;
        private float _squareSizeX;
        private float _squareSizeY;

        private BitmapSource _disparityImageSource;

        private IntrinsicCameraParameters _intrinsicCameraParameters1;
        private IntrinsicCameraParameters _intrinsicCameraParameters2;
        private ExtrinsicCameraParameters _extrinsicCameraParameters;

        private Matrix<double> _fundamentalMatrix;
        private Matrix<double> _essentialMatrix;
        private Matrix<double> _qMatrix;
        private Matrix<double> _r1;
        private Matrix<double> _r2;
        private Matrix<double> _projectionMatrix1;
        private Matrix<double> _projectionMatrix2;

        private int _numDisparities;
        private int _minDispatities;
        private int _sad;
        private int _maxDiff;
        private int _prefilterCap;
        private int _uniquenessRatio;
        private int _speckle;
        private int _speckleRange;
        private StereoSGBM.Mode _disparityMode;

        public StereoCameraCalibrationModel()
        {
            _frameBufferSize = 100;
            _currentMode = CameraCalibrationMode.SavingFrames;
            _intrinsicCameraParameters1 = new IntrinsicCameraParameters();
            _intrinsicCameraParameters2 = new IntrinsicCameraParameters();
            _squareSizeX = 55.0f;
            _squareSizeY = 55.0f;

            BitmapSource bs = BitmapHelper.ToBitmapSource(new Image<Gray, byte>(640, 480, new Gray(1.0)));
            bs.Freeze();
            _disparityImageSource = bs;

            _numDisparities = 64;
            _minDispatities = 0;
            _sad = 3;
            _maxDiff = -1;
            _prefilterCap = 0;
            _uniquenessRatio = 0;
            _speckle = 16;
            _speckleRange = 16;
            _disparityMode = StereoSGBM.Mode.SGBM;
        }

        public BitmapSource DisparityImageSource
        {
            get { return _disparityImageSource; }
            set { Set(() => DisparityImageSource, ref _disparityImageSource, value); }
        }

        public float SquareSizeX
        {
            get { return _squareSizeX; }
            set { Set(() => SquareSizeX, ref _squareSizeX, value); }
        }

        public float SquareSizeY
        {
            get { return _squareSizeY; }
            set { Set(() => SquareSizeY, ref _squareSizeY, value); }
        }

        public CameraModel Camera1
        {
            get { return _camera1; }
            set { Set(() => Camera1, ref _camera1, value); }
        }

        public CameraModel Camera2
        {
            get { return _camera2; }
            set { Set(() => Camera2, ref _camera2, value); }
        }

        public bool StartFlag
        {
            get { return _startFlag; }
            set { Set(() => StartFlag, ref _startFlag, value); }
        }

        /// <summary>
        /// Output of Extrinsics for Camera 1 & 2
        /// </summary>
        public ExtrinsicCameraParameters ExtrinsicCameraParameters
        {
            get { return _extrinsicCameraParameters; }
            set { Set(() => ExtrinsicCameraParameters, ref _extrinsicCameraParameters, value); }
        } 

        public CameraCalibrationMode CurrentMode
        {
            get { return _currentMode; }
            set { Set(() => CurrentMode, ref _currentMode, value); }
        }

        /// <summary>
        /// defines the acquisition length of the buffer 
        /// </summary>
        public int FrameBufferSize
        {
            get { return _frameBufferSize; }
            set { Set(() => FrameBufferSize, ref _frameBufferSize, value); }
        }

        /// <summary>
        /// Camera 1
        /// </summary>
        public IntrinsicCameraParameters IntrinsicCameraParameters1
        {
            get { return _intrinsicCameraParameters1; }
            set { Set(() => IntrinsicCameraParameters1, ref _intrinsicCameraParameters1, value); }
        }

        /// <summary>
        /// Camera 2
        /// </summary>
        public IntrinsicCameraParameters IntrinsicCameraParameters2
        {
            get { return _intrinsicCameraParameters2; }
            set { Set(() => IntrinsicCameraParameters2, ref _intrinsicCameraParameters2, value); }
        }

        /// <summary>
        /// fundamental output matrix for StereoCalibrate
        /// 
        /// 3x3
        /// </summary>
        public Matrix<double> FundamentalMatrix
        {
            get { return _fundamentalMatrix ?? (_fundamentalMatrix = new Matrix<double>(3, 3)); }
            set { Set(() => FundamentalMatrix, ref _fundamentalMatrix, value); }
        }

        /// <summary>
        /// essential output matrix for StereoCalibrate
        /// 
        /// 3x3
        /// </summary>
        public Matrix<double> EssentialMatrix
        {
            get { return _essentialMatrix ?? (_essentialMatrix = new Matrix<double>(3, 3)); }
            set { Set(() => EssentialMatrix, ref _essentialMatrix, value); }
        }

        /// <summary>
        /// This is what were interested in the disparity-to-depth mapping matrix
        /// 
        /// 4x4
        /// </summary>
        public Matrix<double> Q
        {
            get { return _qMatrix ?? (_qMatrix = new Matrix<double>(4, 4)); }
            set { Set(() => Q, ref _qMatrix, value); }
        }

        /// <summary>
        /// rectification transforms (rotation matrices) for Camera 1.
        /// 
        /// 3x3
        /// </summary>
        public Matrix<double> R1
        {
            get { return _r1 ?? (_r1 = new Matrix<double>(3, 3)); }
            set { Set(() => R1, ref _r1, value); }
        }

        /// <summary>
        /// rectification transforms (rotation matrices) for Camera 2.
        /// 
        /// 3x3
        /// </summary>
        public Matrix<double> R2
        {
            get { return _r2 ?? (_r2 = new Matrix<double>(3, 3)); }
            set { Set(() => R2, ref _r2, value); }
        }

        /// <summary>
        /// projection matrices in the new (rectified) coordinate systems for Camera 1
        /// 
        /// 3x4
        /// </summary>
        public Matrix<double> P1
        {
            get { return _projectionMatrix1 ?? (_projectionMatrix1 = new Matrix<double>(3, 4)); }
            set { Set(() => P1, ref _projectionMatrix1, value); }
        }

        /// <summary>
        /// projection matrices in the new (rectified) coordinate systems for Camera 2
        /// 
        /// 3x4
        /// </summary>
        public Matrix<double> P2
        {
            get { return _projectionMatrix2 ?? (_projectionMatrix2 = new Matrix<double>(3, 4)); }
            set { Set(() => P2, ref _projectionMatrix2, value); }
        }


        /// <summary>
        /// This is maximum disparity minus minimum disparity. 
        /// Always greater than 0. 
        /// In the current implementation this parameter must be divisible by 16.
        /// 
        /// 16..160 ~ i % 16 == 0
        /// </summary>
        public int NumDisparities
        {
            get { return _numDisparities; }
            set { Set(() => NumDisparities, ref _numDisparities, value); }
        }

        /// <summary>
        /// The minimum possible disparity value. 
        /// Normally it is 0, but sometimes rectification algorithms can shift images, so this parameter needs to be adjusted accordingly
        /// 
        /// 0..159
        /// </summary>
        public int MinDisparities
        {
            get { return _minDispatities; }
            set { Set(() => MinDisparities, ref _minDispatities, value); }
        }

        /// <summary>
        /// The matched block size. Must be an odd number >=1. 
        /// Normally, it should be somewhere in 3..11 range
        /// 
        /// 1..11 ~ i % 2 == 1
        /// </summary>
        public int SAD
        {
            get { return _sad; }
            set { Set(() => SAD, ref _sad, value); }
        }

        /// <summary>
        /// Maximum allowed difference (in integer pixel units) in the left-right disparity check. 
        /// Set it to non-positive value to disable the check.
        /// 
        /// -1..100
        /// </summary>
        public int MaxDiff
        {
            get { return _maxDiff; }
            set { Set(() => MaxDiff, ref _maxDiff, value); }
        }

        /// <summary>
        /// Truncation value for the prefiltered image pixels. 
        /// The algorithm first computes x-derivative at each pixel and clips its value by [-preFilterCap, preFilterCap] interval. 
        /// The result values are passed to the Birchfield-Tomasi pixel cost function.
        /// 
        /// 0..1000
        /// </summary>
        public int PrefilterCap
        {
            get { return _prefilterCap; }
            set { Set(() => PrefilterCap, ref _prefilterCap, value); }
        }

        /// <summary>
        /// The margin in percents by which the best (minimum) computed cost function value 
        /// should “win” the second best value to consider the found match correct. 
        /// Normally, some value within 5-15 range is good enough
        /// 
        /// 0..30
        /// </summary>
        public int UniquenessRatio
        {
            get { return _uniquenessRatio; }
            set { Set(() => UniquenessRatio, ref _uniquenessRatio, value); }
        }

        /// <summary>
        /// Maximum disparity variation within each connected component. 
        /// If you do speckle filtering, set it to some positive value, multiple of 16. 
        /// Normally, 16 or 32 is good enough
        /// 
        /// 0..64
        /// </summary>
        public int Speckle
        {
            get { return _speckle; }
            set { Set(() => Speckle, ref _speckle, value); }
        }

        /// <summary>
        /// Maximum disparity variation within each connected component. 
        /// If you do speckle filtering, set it to some positive value, multiple of 16. 
        /// Normally, 16 or 32 is good enough.
        /// 
        /// 0..160 ~ i % 16 == 0
        /// </summary>
        public int SpeckleRange
        {
            get { return _speckleRange; }
            set { Set(() => SpeckleRange, ref _speckleRange, value); }
        }

        public StereoSGBM.Mode DisparityMode
        {
            get { return _disparityMode; }
            set { Set(() => DisparityMode, ref _disparityMode, value); }
        }
    } // StereoCameraCalibrationModel
} // namespace
