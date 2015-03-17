using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.PsMove;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common;
using UniMoveStation.Common.Utils;
using Size = System.Drawing.Size;

namespace UniMoveStation.Business.Service
{
    public class StereoCameraCalibrationService
    {
        private CancellationTokenSource _ctsCameraCalibration;
        private Task _captureTask;

        public bool Enabled { get; set; }

        public StereoCameraCalibrationModel Calibration { get; set; }

        #region Devices
        TrackerService _capture1;
        TrackerService _capture2;
        #endregion

        #region Image Processing

        //Frames
        Image<Bgr, Byte> _frameS1;
        Image<Gray, Byte> _grayFrameS1;
        Image<Bgr, Byte> _frameS2;
        Image<Gray, Byte> _grayFrameS2;

        //Chessboard detection
        const int Width = 9;//9 //width of chessboard no. squares in width - 1
        const int Height = 6;//6 // heght of chess board no. squares in height - 1
        readonly Size _patternSize = new Size(Width, Height); //size of chess board to be detected
        readonly Bgr[] _lineColourArray = new Bgr[Width * Height]; // just for displaying coloured lines of detected chessboard
        PointF[] _cornersLeft;
        PointF[] _cornersRight;

        //buffers
        int _bufferSavepoint = 0; //tracks the filled partition of the buffer
        MCvPoint3D32f[][] _cornersObjectPoints; //stores the calculated size for the chessboard
        PointF[][] _cornersPointsLeft;//stores the calculated points from chessboard detection Camera 1
        PointF[][] _cornersPointsRight;//stores the calculated points from chessboard detection Camera 2

        //Calibration parmeters
        Rectangle _rec1 = new Rectangle(); //Rectangle Calibrated in camera 1
        Rectangle _rec2 = new Rectangle(); //Rectangle Caliubrated in camera 2
        private MCvPoint3D32f[] _points; //Computer3DPointsFromStereoPair
        #endregion


        public StereoCameraCalibrationService()
        {
            //set up chessboard drawing array
            Random R = new Random();
            for (int i = 0; i < _lineColourArray.Length; i++)
            {
                _lineColourArray[i] = new Bgr(R.Next(0, 255), R.Next(0, 255), R.Next(0, 255));
            }
        }

        public void Initialize(StereoCameraCalibrationModel calibration, IConsoleService consoleService)
        {
            Calibration = calibration;

            _capture1 = new TrackerService(consoleService);
            _capture1.Initialize(Calibration.Camera1);
            _capture2 = new TrackerService(consoleService);
            _capture2.Initialize(Calibration.Camera2);
        }

        public async void StartCapture()
        {
            if (!Calibration.Camera1.Design && !Calibration.Camera2.Design)
            {
                _ctsCameraCalibration = new CancellationTokenSource();
                CancellationToken token = _ctsCameraCalibration.Token;

                _capture1.StartTracker(PSMoveTrackerExposure.High);
                _capture2.StartTracker(PSMoveTrackerExposure.High);
                try
                {
                    _captureTask = Task.Run(() =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            _capture1.UpdateImage();
                            _capture2.UpdateImage();
                            #region Frame Aquasition
                            //Aquire the frames or calculate two frames from one camera
                            _frameS1 = _capture1.GetImage();
                            if (_frameS1 == null) return;
                            _grayFrameS1 = _frameS1.Convert<Gray, Byte>();
                            _frameS2 = _capture2.GetImage();
                            if (_frameS2 == null) return;
                            _grayFrameS2 = _frameS2.Convert<Gray, Byte>();
                            #endregion

                            #region Saving Chessboard Corners in Buffer
                            if (Calibration.CurrentMode == CameraCalibrationMode.SavingFrames)
                            {
                                //Find the chessboard in bothe images
                                _cornersLeft = CameraCalibration.FindChessboardCorners(_grayFrameS1, _patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH);
                                _cornersRight = CameraCalibration.FindChessboardCorners(_grayFrameS2, _patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH);

                                //we use this loop so we can show a colour image rather than a gray: //CameraCalibration.DrawChessboardCorners(Gray_Frame, patternSize, corners);
                                //we we only do this is the chessboard is present in both images
                                if (_cornersLeft != null && _cornersRight != null) //chess board found in one of the frames?
                                {
                                    //make mesurments more accurate by using FindCornerSubPixel
                                    _grayFrameS1.FindCornerSubPix(new PointF[1][] { _cornersLeft }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.01));
                                    _grayFrameS2.FindCornerSubPix(new PointF[1][] { _cornersRight }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.01));

                                    //if go button has been pressed start aquiring frames else we will just display the points
                                    if (Calibration.StartFlag)
                                    {
                                        //save the calculated points into an array
                                        _cornersPointsLeft[_bufferSavepoint] = _cornersLeft;
                                        _cornersPointsRight[_bufferSavepoint] = _cornersRight;
                                        _bufferSavepoint++;//increase buffer positon

                                        //check the state of buffer
                                        if (_bufferSavepoint == Calibration.FrameBufferSize) Calibration.CurrentMode = CameraCalibrationMode.CalculatingIntrinsics; //buffer full

                                        //Show state of Buffer
                                        //UpdateTitle("Form1: Buffer " + buffer_savepoint.ToString() + " of " + buffer_length.ToString());
                                    }

                                    //draw the results
                                    _frameS1.Draw(new CircleF(_cornersLeft[0], 3), new Bgr(Color.Yellow), 1);
                                    _frameS2.Draw(new CircleF(_cornersRight[0], 3), new Bgr(Color.Yellow), 1);
                                    for (int i = 1; i < _cornersLeft.Length; i++)
                                    {
                                        //left
                                        _frameS1.Draw(new LineSegment2DF(_cornersLeft[i - 1], _cornersLeft[i]), _lineColourArray[i], 2);
                                        _frameS1.Draw(new CircleF(_cornersLeft[i], 3), new Bgr(Color.Yellow), 1);
                                        //right
                                        _frameS2.Draw(new LineSegment2DF(_cornersRight[i - 1], _cornersRight[i]), _lineColourArray[i], 2);
                                        _frameS2.Draw(new CircleF(_cornersRight[i], 3), new Bgr(Color.Yellow), 1);
                                    }
                                    //calibrate the delay bassed on size of buffer
                                    //if buffer small you want a big delay if big small delay
                                    Thread.Sleep(100);//allow the user to move the board to a different position
                                }
                                _cornersLeft = null;
                                _cornersRight = null;
                            }
                            #endregion
                            #region Calculating Stereo Cameras Relationship
                            if (Calibration.CurrentMode == CameraCalibrationMode.CalculatingIntrinsics)
                            {
                                //fill the MCvPoint3D32f with correct mesurments
                                for (int k = 0; k < Calibration.FrameBufferSize; k++)
                                {
                                    //Fill our objects list with the real world mesurments for the intrinsic calculations
                                    List<MCvPoint3D32f> objectList = new List<MCvPoint3D32f>();
                                    for (int i = 0; i < Height; i++)
                                    {
                                        for (int j = 0; j < Width; j++)
                                        {
                                            objectList.Add(new MCvPoint3D32f(j * Calibration.SquareSizeX, i * Calibration.SquareSizeY, 0.0F));
                                        }
                                    }
                                    _cornersObjectPoints[k] = objectList.ToArray();
                                }
                                //If Emgu.CV.CvEnum.CALIB_TYPE == CV_CALIB_USE_INTRINSIC_GUESS and/or CV_CALIB_FIX_ASPECT_RATIO are specified, some or all of fx, fy, cx, cy must be initialized before calling the function
                                //if you use FIX_ASPECT_RATIO and FIX_FOCAL_LEGNTH options, these values needs to be set in the intrinsic parameters before the CalibrateCamera function is called. Otherwise 0 values are used as default.
                                ExtrinsicCameraParameters extrinsicCameraParameters;
                                Matrix<double> fundamentalMatrix;
                                Matrix<double> essentialMatrix;
                                CameraCalibration.StereoCalibrate(
                                    _cornersObjectPoints, 
                                    _cornersPointsLeft, 
                                    _cornersPointsRight,
                                    Calibration.IntrinsicCameraParameters1,
                                    Calibration.IntrinsicCameraParameters2, 
                                    _frameS1.Size,
                                    Emgu.CV.CvEnum.CALIB_TYPE.CV_CALIB_RATIONAL_MODEL, 
                                    new MCvTermCriteria(0.1e5),
                                    out extrinsicCameraParameters, 
                                    out fundamentalMatrix, 
                                    out essentialMatrix);

                                Calibration.ExtrinsicCameraParameters = extrinsicCameraParameters;
                                Calibration.FundamentalMatrix = fundamentalMatrix;
                                Calibration.EssentialMatrix = essentialMatrix;

                                MessageBox.Show("Intrinsic Calculation Complete"); //display that the mothod has been succesful
                                //currentMode = Mode.Calibrated;

                                //Computes rectification transforms for each head of a calibrated stereo camera.
                                CvInvoke.cvStereoRectify(
                                    Calibration.IntrinsicCameraParameters1.IntrinsicMatrix,
                                    Calibration.IntrinsicCameraParameters2.IntrinsicMatrix,
                                    Calibration.IntrinsicCameraParameters1.DistortionCoeffs,
                                    Calibration.IntrinsicCameraParameters2.DistortionCoeffs,
                                    _frameS1.Size,
                                    Calibration.ExtrinsicCameraParameters.RotationVector.RotationMatrix, 
                                    Calibration.ExtrinsicCameraParameters.TranslationVector,
                                    Calibration.R1,
                                    Calibration.R2,
                                    Calibration.P1,
                                    Calibration.P2,
                                    Calibration.Q,
                                    Emgu.CV.CvEnum.STEREO_RECTIFY_TYPE.DEFAULT, 
                                    0,
                                    _frameS1.Size, 
                                    ref _rec1, 
                                    ref _rec2);

                                //This will Show us the usable area from each camera
                                MessageBox.Show("Left: " + _rec1.ToString() + " \nRight: " + _rec2.ToString());
                                Calibration.CurrentMode = CameraCalibrationMode.Calibrated;
                                Calibration.StartFlag = false;
                                
                            }
                            #endregion
                            #region Caluclating disparity map after calibration
                            if (Calibration.CurrentMode == CameraCalibrationMode.Calibrated)
                            {
                                Image<Gray, short> disparityMap;

                                Computer3DPointsFromStereoPair(_grayFrameS1, _grayFrameS2, out disparityMap, out _points);

                                //Display the disparity map
                                BitmapSource bitmapSource = BitmapHelper.ToBitmapSource(disparityMap);
                                bitmapSource.Freeze();
                                Calibration.DisparityImageSource = bitmapSource;
                                //Draw the accurate area
                                _frameS1.Draw(_rec1, new Bgr(Color.LimeGreen), 1);
                                _frameS2.Draw(_rec2, new Bgr(Color.LimeGreen), 1);
                            }
                            #endregion
                            //display image

                            if (!_ctsCameraCalibration.Token.IsCancellationRequested)
                            {
                                BitmapSource bitmapSource = BitmapHelper.ToBitmapSource(_frameS1);
                                bitmapSource.Freeze();
                                Calibration.Camera1.ImageSource = bitmapSource;

                                bitmapSource = BitmapHelper.ToBitmapSource(_frameS2);
                                bitmapSource.Freeze();
                                Calibration.Camera2.ImageSource = bitmapSource;
                            }
                        }
                    }, token);
                    await _captureTask;
                }
                catch (OperationCanceledException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        public async void StopCapture()
        {
            if (_ctsCameraCalibration != null)
            {
                _ctsCameraCalibration.Cancel();
                _captureTask.Wait();

                _capture1.StopStracker();
                _capture2.StopStracker();
                _capture1.Destroy();
                _capture2.Destroy();
            }
        }

        public void CancelCalibration()
        {
            Calibration.StartFlag = false;
            Calibration.CurrentMode = CameraCalibrationMode.Stopped;
        }

        public void StartCalibration()
        {
            //set up the arrays needed
            _cornersObjectPoints = new MCvPoint3D32f[Calibration.FrameBufferSize][]; //stores the calculated size for the chessboard
            _cornersPointsLeft = new PointF[Calibration.FrameBufferSize][];//stores the calculated points from chessboard detection Camera 1
            _cornersPointsRight = new PointF[Calibration.FrameBufferSize][];//stores the calculated points from chessboard detection Camera 2
            _bufferSavepoint = 0;

            //allow the start
            if (Calibration.CurrentMode != CameraCalibrationMode.SavingFrames) Calibration.CurrentMode = CameraCalibrationMode.SavingFrames;
            Calibration.StartFlag = true;
        }
        /// <summary>
        /// Given the left and right image, computer the disparity map and the 3D point cloud.
        /// </summary>
        /// <param name="left">The left image</param>
        /// <param name="right">The right image</param>
        /// <param name="disparityMap">The left disparity map</param>
        /// <param name="points">The 3D point cloud within a [-0.5, 0.5] cube</param>
        private void Computer3DPointsFromStereoPair(Image<Gray, Byte> left, Image<Gray, Byte> right, out Image<Gray, short> disparityMap, out MCvPoint3D32f[] points)
        {
            Size size = left.Size;

            disparityMap = new Image<Gray, short>(size);

            int P1 = 8 * 1 * Calibration.SAD * Calibration.SAD;//GetSliderValue(P1_Slider);
            int P2 = 32 * 1 * Calibration.SAD * Calibration.SAD;//GetSliderValue(P2_Slider);

            using (StereoSGBM stereoSolver = new StereoSGBM(
                Calibration.MinDisparities, 
                Calibration.NumDisparities, 
                Calibration.SAD, 
                P1, 
                P2, 
                Calibration.MaxDiff, 
                Calibration.PrefilterCap,
                Calibration.UniquenessRatio,
                Calibration.Speckle,
                Calibration.SpeckleRange,
                Calibration.DisparityMode))
            //using (StereoBM stereoSolver = new StereoBM(Emgu.CV.CvEnum.STEREO_BM_TYPE.BASIC, 0))
            {
                stereoSolver.FindStereoCorrespondence(left, right, disparityMap);//Computes the disparity map using: 
                /*GC: graph cut-based algorithm
                  BM: block matching algorithm
                  SGBM: modified H. Hirschmuller algorithm HH08*/
                points = PointCollection.ReprojectImageTo3D(disparityMap, Calibration.Q); //Reprojects disparity image to 3D space.
            }
        }
    } // StereoCameraCalibrationService
} // namespace
