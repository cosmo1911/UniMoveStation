using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common;
using UniMoveStation.Common.Utils;

namespace UniMoveStation.Business.Service
{
    /// <summary>
    /// http://www.emgu.com/wiki/index.php?title=Camera_Calibration
    /// </summary>
    public class CameraCalibrationService
    {
        private CancellationTokenSource _ctsCameraCalibration;
        private Task _captureTask;
        private CameraModel _camera;
        private readonly ISettingsService _settingsService; 
        
        #region Display and aquaring chess board info
        private Capture _capture; // capture device
        private Image<Bgr, Byte> _img; // image captured
        private Image<Gray, Byte> _grayFrame; // image for processing
        private const int Width = 9;//9 //width of chessboard no. squares in width - 1
        private const int Height = 6;//6 // height of chess board no. squares in heigth - 1
        private readonly Size _patternSize = new Size(Width, Height); //size of chess board to be detected
        private PointF[] _corners; //corners found from chessboard
        private readonly Bgr[] _lineColourArray = new Bgr[Width * Height]; // just for displaying coloured lines of detected chessboard

        private static Image<Gray, Byte>[] _frameArrayBuffer = new Image<Gray, byte>[100]; //number of images to calibrate camera over
        private int _frameBufferSavepoint;
        #endregion

        #region Getting the camera calibration
        private MCvPoint3D32f[][] _cornersObjectList = new MCvPoint3D32f[_frameArrayBuffer.Length][];
        private PointF[][] _cornersPointsList = new PointF[_frameArrayBuffer.Length][];

        #endregion

        #region Constructor
        /// <summary>
        /// resolves its dependencies via the service locator
        /// </summary>
        [PreferredConstructor]
        public CameraCalibrationService() : this(null)
        {
            if (SimpleIoc.Default.IsRegistered<ISettingsService>())
            {
                _settingsService = SimpleIoc.Default.GetInstance<ISettingsService>();
            }
            else
            {
                _settingsService = new JsonSettingsService();
            }
        }

        public CameraCalibrationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            //fill line colour array
            Random r = new Random();
            for (int i = 0; i < _lineColourArray.Length; i++)
            {
                _lineColourArray[i] = new Bgr(r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));
            }
        }
        #endregion

        void ImageGrabbed()
        {
            //lets get a frame from our capture device
            _img = _capture.RetrieveBgrFrame();
            if (_img == null) return;
            _grayFrame = _img.Convert<Gray, Byte>();
            
            //apply chess board detection
            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.SavingFrames)
            {
                _corners = CameraCalibration.FindChessboardCorners(_grayFrame, _patternSize, CALIB_CB_TYPE.ADAPTIVE_THRESH);
                //we use this loop so we can show a colour image rather than a gray: 
                //CameraCalibration.DrawChessboardCorners(Gray_Frame, patternSize, corners);

                if (_corners != null) //chess board found
                {
                    //make mesurments more accurate by using FindCornerSubPixel
                    _grayFrame.FindCornerSubPix(new PointF[1][] { _corners }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                    //if go button has been pressed start aquiring frames else we will just display the points
                    if (_camera.Calibration.StartFlag)
                    {
                        _frameArrayBuffer[_frameBufferSavepoint] = _grayFrame.Copy(); //store the image
                        _frameBufferSavepoint++;//increase buffer positon

                        //check the state of buffer
                        if (_frameBufferSavepoint == _frameArrayBuffer.Length) _camera.Calibration.CurrentMode = CameraCalibrationMode.CalculatingIntrinsics; //buffer full
                    }

                    //draw the results
                    _img.Draw(new CircleF(_corners[0], 3), new Bgr(Color.Yellow), 1);
                    for (int i = 1; i < _corners.Length; i++)
                    {
                        _img.Draw(new LineSegment2DF(_corners[i - 1], _corners[i]), _lineColourArray[i], 2);
                        _img.Draw(new CircleF(_corners[i], 3), new Bgr(Color.Yellow), 1);
                    }
                    //calibrate the delay bassed on size of buffer
                    //if buffer small you want a big delay if big small delay
                    Thread.Sleep(100);//allow the user to move the board to a different position
                }
                _corners = null;
            }
            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.CalculatingIntrinsics)
            {
                //we can do this in the loop above to increase speed
                for (int k = 0; k < _frameArrayBuffer.Length; k++)
                {

                    _cornersPointsList[k] = CameraCalibration.FindChessboardCorners(_frameArrayBuffer[k], _patternSize, CALIB_CB_TYPE.ADAPTIVE_THRESH);
                    //for accuracy
                    _grayFrame.FindCornerSubPix(_cornersPointsList, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                    //Fill our objects list with the real world mesurments for the intrinsic calculations
                    List<MCvPoint3D32f> objectList = new List<MCvPoint3D32f>();
                    for (int i = 0; i < Height; i++)
                    {
                        for (int j = 0; j < Width; j++)
                        {
                            objectList.Add(new MCvPoint3D32f(j * 55.0F, i * 55.0F, 0.0F));
                        }
                    }
                    _cornersObjectList[k] = objectList.ToArray();
                }
                ExtrinsicCameraParameters[] ex;
                //our error should be as close to 0 as possible
                _camera.Calibration.Error = CameraCalibration.CalibrateCamera(
                    _cornersObjectList, 
                    _cornersPointsList, 
                    _grayFrame.Size, 
                    _camera.Calibration.IntrinsicParameters, 
                    CALIB_TYPE.CV_CALIB_RATIONAL_MODEL, 
                    new MCvTermCriteria(30, 0.1), 
                    out ex);

                //_camera.Calibration.ExtrinsicParameters = ex;

                //set up to allow another calculation
                //SetButtonState(true);
                _camera.Calibration.StartFlag = false;

                //If Emgu.CV.CvEnum.CALIB_TYPE == CV_CALIB_USE_INTRINSIC_GUESS and/or CV_CALIB_FIX_ASPECT_RATIO are specified, some or all of fx, fy, cx, cy must be initialized before calling the function
                //if you use FIX_ASPECT_RATIO and FIX_FOCAL_LEGNTH options, these values needs to be set in the intrinsic parameters before the CalibrateCamera function is called. Otherwise 0 values are used as default.
                Console.WriteLine("Intrinsic Calculation Error: " + _camera.Calibration.Error); //display the results to the user
                _camera.Calibration.CurrentMode = CameraCalibrationMode.Calibrated;
            }
            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.Calibrated)
            {
                _corners = CameraCalibration.FindChessboardCorners(_grayFrame, _patternSize, CALIB_CB_TYPE.ADAPTIVE_THRESH);
                //we use this loop so we can show a colour image rather than a gray: 
                //CameraCalibration.DrawChessboardCorners(Gray_Frame, patternSize, corners);

                if (_corners != null) //chess board found
                {
                    //make mesurments more accurate by using FindCornerSubPixel
                    _grayFrame.FindCornerSubPix(new PointF[1][] { _corners }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                    //draw the results
                    _img.Draw(new CircleF(_corners[0], 3), new Bgr(Color.Yellow), 1);
                    for (int i = 1; i < _corners.Length; i++)
                    {
                        _img.Draw(new LineSegment2DF(_corners[i - 1], _corners[i]), _lineColourArray[i], 2);
                        _img.Draw(new CircleF(_corners[i], 3), new Bgr(Color.Yellow), 1);
                    }
                    //calibrate the delay bassed on size of buffer
                    //if buffer small you want a big delay if big small delay
                    Thread.Sleep(100);//allow the user to move the board to a different position
                }
                //display the original image
                //Sub_PicturBox.Image = img.ToBitmap();
                //calculate the camera intrinsics
                Matrix<float> map1, map2;
                _camera.Calibration.IntrinsicParameters.InitUndistortMap(_img.Width, _img.Height, out map1, out map2);

                //remap the image to the particular intrinsics
                //In the current version of EMGU any pixel that is not corrected is set to transparent allowing the original image to be displayed if the same
                //image is mapped backed, in the future this should be controllable through the flag '0'
                Image<Bgr, Byte> temp = _img.CopyBlank();
                CvInvoke.cvRemap(_img, temp, map1, map2, 0, new MCvScalar(0));
                _img = temp.Copy();
            }
            if(!_ctsCameraCalibration.Token.IsCancellationRequested)
            {
                BitmapSource bitmapSource = BitmapHelper.ToBitmapSource(_img);
                bitmapSource.Freeze();
                _camera.ImageSource = bitmapSource;
            }
        }

        public void Initialize(CameraModel camera)
        {
            _camera = camera;
        }

        public void CancelCalibration()
        {
            _camera.Calibration.StartFlag = false;
            _camera.Calibration.CurrentMode = CameraCalibrationMode.Stopped;
        }

        public async void StartCapture()
        {
            // Can only access the first camera without CL Eye SDK
            if (_camera.TrackerId == 0 && !_camera.Design)
            {
                _capture = new Capture(_camera.TrackerId);
                _ctsCameraCalibration = new CancellationTokenSource();
                CancellationToken token = _ctsCameraCalibration.Token;

                _capture.Start();
                try
                {
                    // needed to avoid bitmapsource access violation?
                    _captureTask = Task.Run(() =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            ImageGrabbed();
                        }
                    }, token);
                    await _captureTask;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                finally
                {
                    _capture.Stop();
                    _capture.Dispose();
                }
            }
        }

        public async void StopCapture()
        {
            if (_ctsCameraCalibration != null)
            {
                _ctsCameraCalibration.Cancel();
                await _captureTask;
            }
        }

        public void StartCalibration()
        {
            //set up the arrays needed
            _frameArrayBuffer = new Image<Gray, byte>[_camera.Calibration.FrameBufferSize];
            _cornersObjectList = new MCvPoint3D32f[_frameArrayBuffer.Length][];
            _cornersPointsList = new PointF[_frameArrayBuffer.Length][];
            _frameBufferSavepoint = 0;
            //allow the start
            if (_camera.Calibration.CurrentMode != CameraCalibrationMode.SavingFrames) _camera.Calibration.CurrentMode = CameraCalibrationMode.SavingFrames;
            _camera.Calibration.StartFlag = true;
        }

        public void SaveCalibration()
        {
            _settingsService.SaveCalibration(_camera.Calibration);
        }
    } // CameraCalibrationService
} // namespace
