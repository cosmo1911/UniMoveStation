using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.PsMove;
using UniMoveStation.Business.Service.Design;
using UniMoveStation.Business.Service.Event;
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
        TrackerService _Capture1;
        TrackerService _Capture2;
        #endregion

        #region Image Processing

        //Frames
        Image<Bgr, Byte> frame_S1;
        Image<Gray, Byte> Gray_frame_S1;
        Image<Bgr, Byte> frame_S2;
        Image<Gray, Byte> Gray_frame_S2;

        //Chessboard detection
        const int width = 9;//9 //width of chessboard no. squares in width - 1
        const int height = 6;//6 // heght of chess board no. squares in heigth - 1
        Size patternSize = new Size(width, height); //size of chess board to be detected
        Bgr[] line_colour_array = new Bgr[width * height]; // just for displaying coloured lines of detected chessboard
        PointF[] corners_Left;
        PointF[] corners_Right;
        bool start_Flag = true; //start straight away

        //buffers
        static int buffer_length = 100; //define the aquasition length of the buffer 
        int buffer_savepoint = 0; //tracks the filled partition of the buffer
        MCvPoint3D32f[][] corners_object_Points = new MCvPoint3D32f[buffer_length][]; //stores the calculated size for the chessboard
        PointF[][] corners_points_Left = new PointF[buffer_length][];//stores the calculated points from chessboard detection Camera 1
        PointF[][] corners_points_Right = new PointF[buffer_length][];//stores the calculated points from chessboard detection Camera 2

        //Calibration parmeters
        IntrinsicCameraParameters IntrinsicCam1 = new IntrinsicCameraParameters(); //Camera 1
        IntrinsicCameraParameters IntrinsicCam2 = new IntrinsicCameraParameters(); //Camera 2
        ExtrinsicCameraParameters EX_Param; //Output of Extrinsics for Camera 1 & 2
        Matrix<double> fundamental; //fundemental output matrix for StereoCalibrate
        Matrix<double> essential; //essential output matrix for StereoCalibrate
        Rectangle Rec1 = new Rectangle(); //Rectangle Calibrated in camera 1
        Rectangle Rec2 = new Rectangle(); //Rectangle Caliubrated in camera 2
        Matrix<double> Q = new Matrix<double>(4, 4); //This is what were interested in the disparity-to-depth mapping matrix
        Matrix<double> R1 = new Matrix<double>(3, 3); //rectification transforms (rotation matrices) for Camera 1.
        Matrix<double> R2 = new Matrix<double>(3, 3); //rectification transforms (rotation matrices) for Camera 1.
        Matrix<double> P1 = new Matrix<double>(3, 4); //projection matrices in the new (rectified) coordinate systems for Camera 1.
        Matrix<double> P2 = new Matrix<double>(3, 4); //projection matrices in the new (rectified) coordinate systems for Camera 2.
        private MCvPoint3D32f[] _points; //Computer3DPointsFromStereoPair
        #endregion

        #region Current mode variables
        StereoCameraCalibrationMode currentMode = StereoCameraCalibrationMode.SavingFrames;
        #endregion

        public StereoCameraCalibrationService()
        {
            //set up chessboard drawing array
            Random R = new Random();
            for (int i = 0; i < line_colour_array.Length; i++)
            {
                line_colour_array[i] = new Bgr(R.Next(0, 255), R.Next(0, 255), R.Next(0, 255));
            }
        }

        public void Initialize(StereoCameraCalibrationModel calibration, IConsoleService consoleService)
        {
            Calibration = calibration;

            _Capture1 = new TrackerService(consoleService);
            _Capture1.Initialize(Calibration.Camera1);
            _Capture2 = new TrackerService(consoleService);
            _Capture2.Initialize(Calibration.Camera2);
        }

        public async void StartCapture()
        {
            if (!Calibration.Camera1.Design && !Calibration.Camera2.Design)
            {
                _ctsCameraCalibration = new CancellationTokenSource();
                CancellationToken token = _ctsCameraCalibration.Token;

                _Capture1.StartTracker(0f);
                _Capture2.StartTracker(0f);
                try
                {
                    // needed to avoid bitmapsource access violation?
                    _captureTask = Task.Run(() =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            #region Frame Aquasition
                            //Aquire the frames or calculate two frames from one camera
                            frame_S1 = _Capture1.GetImage();
                            if (frame_S1 == null) return;
                            Gray_frame_S1 = frame_S1.Convert<Gray, Byte>();
                            frame_S2 = _Capture2.GetImage();
                            if (frame_S2 == null) return;
                            Gray_frame_S2 = frame_S2.Convert<Gray, Byte>();
                            #endregion

                            #region Saving Chessboard Corners in Buffer
                            if (currentMode == StereoCameraCalibrationMode.SavingFrames)
                            {
                                //Find the chessboard in bothe images
                                corners_Left = CameraCalibration.FindChessboardCorners(Gray_frame_S1, patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH);
                                corners_Right = CameraCalibration.FindChessboardCorners(Gray_frame_S2, patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH);

                                //we use this loop so we can show a colour image rather than a gray: //CameraCalibration.DrawChessboardCorners(Gray_Frame, patternSize, corners);
                                //we we only do this is the chessboard is present in both images
                                if (corners_Left != null && corners_Right != null) //chess board found in one of the frames?
                                {
                                    //make mesurments more accurate by using FindCornerSubPixel
                                    Gray_frame_S1.FindCornerSubPix(new PointF[1][] { corners_Left }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.01));
                                    Gray_frame_S2.FindCornerSubPix(new PointF[1][] { corners_Right }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.01));

                                    //if go button has been pressed start aquiring frames else we will just display the points
                                    if (start_Flag)
                                    {
                                        //save the calculated points into an array
                                        corners_points_Left[buffer_savepoint] = corners_Left;
                                        corners_points_Right[buffer_savepoint] = corners_Right;
                                        buffer_savepoint++;//increase buffer positon

                                        //check the state of buffer
                                        if (buffer_savepoint == buffer_length) currentMode = StereoCameraCalibrationMode.Caluculating_Stereo_Intrinsics; //buffer full

                                        //Show state of Buffer
                                        //UpdateTitle("Form1: Buffer " + buffer_savepoint.ToString() + " of " + buffer_length.ToString());
                                    }

                                    //draw the results
                                    frame_S1.Draw(new CircleF(corners_Left[0], 3), new Bgr(Color.Yellow), 1);
                                    frame_S2.Draw(new CircleF(corners_Right[0], 3), new Bgr(Color.Yellow), 1);
                                    for (int i = 1; i < corners_Left.Length; i++)
                                    {
                                        //left
                                        frame_S1.Draw(new LineSegment2DF(corners_Left[i - 1], corners_Left[i]), line_colour_array[i], 2);
                                        frame_S1.Draw(new CircleF(corners_Left[i], 3), new Bgr(Color.Yellow), 1);
                                        //right
                                        frame_S2.Draw(new LineSegment2DF(corners_Right[i - 1], corners_Right[i]), line_colour_array[i], 2);
                                        frame_S2.Draw(new CircleF(corners_Right[i], 3), new Bgr(Color.Yellow), 1);
                                    }
                                    //calibrate the delay bassed on size of buffer
                                    //if buffer small you want a big delay if big small delay
                                    Thread.Sleep(100);//allow the user to move the board to a different position
                                }
                                corners_Left = null;
                                corners_Right = null;
                            }
                            #endregion
                            #region Calculating Stereo Cameras Relationship
                            if (currentMode == StereoCameraCalibrationMode.Caluculating_Stereo_Intrinsics)
                            {
                                //fill the MCvPoint3D32f with correct mesurments
                                for (int k = 0; k < buffer_length; k++)
                                {
                                    //Fill our objects list with the real world mesurments for the intrinsic calculations
                                    List<MCvPoint3D32f> object_list = new List<MCvPoint3D32f>();
                                    for (int i = 0; i < height; i++)
                                    {
                                        for (int j = 0; j < width; j++)
                                        {
                                            object_list.Add(new MCvPoint3D32f(j * 20.0F, i * 20.0F, 0.0F));
                                        }
                                    }
                                    corners_object_Points[k] = object_list.ToArray();
                                }
                                //If Emgu.CV.CvEnum.CALIB_TYPE == CV_CALIB_USE_INTRINSIC_GUESS and/or CV_CALIB_FIX_ASPECT_RATIO are specified, some or all of fx, fy, cx, cy must be initialized before calling the function
                                //if you use FIX_ASPECT_RATIO and FIX_FOCAL_LEGNTH options, these values needs to be set in the intrinsic parameters before the CalibrateCamera function is called. Otherwise 0 values are used as default.
                                CameraCalibration.StereoCalibrate(corners_object_Points, corners_points_Left, corners_points_Right, IntrinsicCam1, IntrinsicCam2, frame_S1.Size,
                                                                                 Emgu.CV.CvEnum.CALIB_TYPE.DEFAULT, new MCvTermCriteria(0.1e5),
                                                                                 out EX_Param, out fundamental, out essential);
                                MessageBox.Show("Intrinsic Calculation Complete"); //display that the mothod has been succesful
                                //currentMode = Mode.Calibrated;

                                //Computes rectification transforms for each head of a calibrated stereo camera.
                                CvInvoke.cvStereoRectify(IntrinsicCam1.IntrinsicMatrix, IntrinsicCam2.IntrinsicMatrix,
                                                         IntrinsicCam1.DistortionCoeffs, IntrinsicCam2.DistortionCoeffs,
                                                         frame_S1.Size,
                                                         EX_Param.RotationVector.RotationMatrix, EX_Param.TranslationVector,
                                                         R1, R2, P1, P2, Q,
                                                         Emgu.CV.CvEnum.STEREO_RECTIFY_TYPE.DEFAULT, 0,
                                                         frame_S1.Size, ref Rec1, ref Rec2);

                                //This will Show us the usable area from each camera
                                MessageBox.Show("Left: " + Rec1.ToString() + " \nRight: " + Rec2.ToString());
                                currentMode = StereoCameraCalibrationMode.Calibrated;

                            }
                            #endregion
                            #region Caluclating disparity map after calibration
                            if (currentMode == StereoCameraCalibrationMode.Calibrated)
                            {
                                Image<Gray, short> disparityMap;

                                Computer3DPointsFromStereoPair(Gray_frame_S1, Gray_frame_S2, out disparityMap, out _points);

                                //Display the disparity map
                                //DisparityMap.Image = disparityMap.ToBitmap();
                                //Draw the accurate area
                                frame_S1.Draw(Rec1, new Bgr(Color.LimeGreen), 1);
                                frame_S2.Draw(Rec2, new Bgr(Color.LimeGreen), 1);
                            }
                            #endregion
                            //display image

                            if (!_ctsCameraCalibration.Token.IsCancellationRequested)
                            {
                                BitmapSource bitmapSource = BitmapHelper.ToBitmapSource(frame_S1);
                                bitmapSource.Freeze();
                                Calibration.Camera1.ImageSource = bitmapSource;

                                bitmapSource = BitmapHelper.ToBitmapSource(frame_S2);
                                bitmapSource.Freeze();
                                Calibration.Camera2.ImageSource = bitmapSource;
                            }
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
                    _Capture1.Destroy();
                    _Capture2.Destroy();
                }
            }
        }

        public async void StopCapture()
        {
            if (_ctsCameraCalibration != null)
            {
                _ctsCameraCalibration.Cancel();
                await _captureTask;
                _Capture1.Destroy();
            }
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
            disparityMap = null;
            points = new MCvPoint3D32f[] { };
            //Size size = left.Size;

            //disparityMap = new Image<Gray, short>(size);
            ////thread safe calibration values


            ///*This is maximum disparity minus minimum disparity. Always greater than 0. In the current implementation this parameter must be divisible by 16.*/
            //int numDisparities = GetSliderValue(Num_Disparities);

            ///*The minimum possible disparity value. Normally it is 0, but sometimes rectification algorithms can shift images, so this parameter needs to be adjusted accordingly*/
            //int minDispatities = GetSliderValue(Min_Disparities);

            ///*The matched block size. Must be an odd number >=1 . Normally, it should be somewhere in 3..11 range*/
            //int SAD = GetSliderValue(SAD_Window);

            ///*P1, P2 – Parameters that control disparity smoothness. The larger the values, the smoother the disparity. 
            // * P1 is the penalty on the disparity change by plus or minus 1 between neighbor pixels. 
            // * P2 is the penalty on the disparity change by more than 1 between neighbor pixels. 
            // * The algorithm requires P2 > P1 . 
            // * See stereo_match.cpp sample where some reasonably good P1 and P2 values are shown 
            // * (like 8*number_of_image_channels*SADWindowSize*SADWindowSize and 32*number_of_image_channels*SADWindowSize*SADWindowSize , respectively).*/

            //int P1 = 8 * 1 * SAD * SAD;//GetSliderValue(P1_Slider);
            //int P2 = 32 * 1 * SAD * SAD;//GetSliderValue(P2_Slider);

            ///* Maximum allowed difference (in integer pixel units) in the left-right disparity check. Set it to non-positive value to disable the check.*/
            //int disp12MaxDiff = GetSliderValue(Disp12MaxDiff);

            ///*Truncation value for the prefiltered image pixels. 
            // * The algorithm first computes x-derivative at each pixel and clips its value by [-preFilterCap, preFilterCap] interval. 
            // * The result values are passed to the Birchfield-Tomasi pixel cost function.*/
            //int PreFilterCap = GetSliderValue(pre_filter_cap);

            ///*The margin in percents by which the best (minimum) computed cost function value should “win” the second best value to consider the found match correct. 
            // * Normally, some value within 5-15 range is good enough*/
            //int UniquenessRatio = GetSliderValue(uniquenessRatio);

            ///*Maximum disparity variation within each connected component. 
            // * If you do speckle filtering, set it to some positive value, multiple of 16. 
            // * Normally, 16 or 32 is good enough*/
            //int Speckle = GetSliderValue(Speckle_Window);

            ///*Maximum disparity variation within each connected component. If you do speckle filtering, set it to some positive value, multiple of 16. Normally, 16 or 32 is good enough.*/
            //int SpeckleRange = GetSliderValue(specklerange);

            ///*Set it to true to run full-scale 2-pass dynamic programming algorithm. It will consume O(W*H*numDisparities) bytes, 
            // * which is large for 640x480 stereo and huge for HD-size pictures. By default this is usually false*/
            ////Set globally for ease
            ////bool fullDP = true;

            //using (StereoSGBM stereoSolver = new StereoSGBM(minDispatities, numDisparities, SAD, P1, P2, disp12MaxDiff, PreFilterCap, UniquenessRatio, Speckle, SpeckleRange, fullDP))
            ////using (StereoBM stereoSolver = new StereoBM(Emgu.CV.CvEnum.STEREO_BM_TYPE.BASIC, 0))
            //{
            //    stereoSolver.FindStereoCorrespondence(left, right, disparityMap);//Computes the disparity map using: 
            //    /*GC: graph cut-based algorithm
            //      BM: block matching algorithm
            //      SGBM: modified H. Hirschmuller algorithm HH08*/
            //    points = PointCollection.ReprojectImageTo3D(disparityMap, Q); //Reprojects disparity image to 3D space.
            //}
        }
    }
}
