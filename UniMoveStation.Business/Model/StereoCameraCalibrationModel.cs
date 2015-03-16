using Emgu.CV;
using GalaSoft.MvvmLight;

namespace UniMoveStation.Business.Model
{
    public class StereoCameraCalibrationModel : ObservableObject
    {
        private CameraModel _camera1;
        private CameraModel _camera2;
        private int _currentMode;
        private int _frameBuffer;
        private IntrinsicCameraParameters _intrinsicCameraParameters1;
        private IntrinsicCameraParameters _intrinsicCameraParameters2;
        private Matrix<double> _fundamentalMatrix;
        private Matrix<double> _essentialMatrix; 

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

        public int CurrentMode
        {
            get { return _currentMode; }
            set { Set(() => CurrentMode, ref _currentMode, value); }
        }

        public int FrameBuffer
        {
            get { return _frameBuffer; }
            set { Set(() => FrameBuffer, ref _frameBuffer, value); }
        }
    }
}
