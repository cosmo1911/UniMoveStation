using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
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
        private CameraModel _camera;
        private ISettingsService _settingsService; 
        
        #region Display and aquaring chess board info
        Capture _capture; // capture device
        Image<Bgr, Byte> img; // image captured
        Image<Gray, Byte> Gray_Frame; // image for processing
        const int width = 9;//9 //width of chessboard no. squares in width - 1
        const int height = 6;//6 // height of chess board no. squares in heigth - 1
        Size patternSize = new Size(width, height); //size of chess board to be detected
        PointF[] corners; //corners found from chessboard
        Bgr[] line_colour_array = new Bgr[width * height]; // just for displaying coloured lines of detected chessboard

        static Image<Gray, Byte>[] Frame_array_buffer = new Image<Gray, byte>[100]; //number of images to calibrate camera over
        int frame_buffer_savepoint;
        #endregion

        #region Getting the camera calibration
        MCvPoint3D32f[][] corners_object_list = new MCvPoint3D32f[Frame_array_buffer.Length][];
        PointF[][] corners_points_list = new PointF[Frame_array_buffer.Length][];

        #endregion

        #region Constructor
        public CameraCalibrationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            //fill line colour array
            Random R = new Random();
            for (int i = 0; i < line_colour_array.Length; i++)
            {
                line_colour_array[i] = new Bgr(R.Next(0, 255), R.Next(0, 255), R.Next(0, 255));
            }
        }
        #endregion

        void _Capture_ImageGrabbed(object sender, EventArgs e)
        {
            //lets get a frame from our capture device
            img = _capture.RetrieveBgrFrame();
            Gray_Frame = img.Convert<Gray, Byte>();
            
            //apply chess board detection
            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.SavingFrames)
            {
                corners = CameraCalibration.FindChessboardCorners(Gray_Frame, patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH);
                //we use this loop so we can show a colour image rather than a gray: //CameraCalibration.DrawChessboardCorners(Gray_Frame, patternSize, corners);

                if (corners != null) //chess board found
                {
                    //make mesurments more accurate by using FindCornerSubPixel
                    Gray_Frame.FindCornerSubPix(new PointF[1][] { corners }, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                    //if go button has been pressed start aquiring frames else we will just display the points
                    if (_camera.Calibration.StartFlag)
                    {
                        Frame_array_buffer[frame_buffer_savepoint] = Gray_Frame.Copy(); //store the image
                        frame_buffer_savepoint++;//increase buffer positon

                        //check the state of buffer
                        if (frame_buffer_savepoint == Frame_array_buffer.Length) _camera.Calibration.CurrentMode = CameraCalibrationMode.CalculatingIntrinsics; //buffer full
                    }

                    //draw the results
                    img.Draw(new CircleF(corners[0], 3), new Bgr(Color.Yellow), 1);
                    for (int i = 1; i < corners.Length; i++)
                    {
                        img.Draw(new LineSegment2DF(corners[i - 1], corners[i]), line_colour_array[i], 2);
                        img.Draw(new CircleF(corners[i], 3), new Bgr(Color.Yellow), 1);
                    }
                    //calibrate the delay bassed on size of buffer
                    //if buffer small you want a big delay if big small delay
                    Thread.Sleep(100);//allow the user to move the board to a different position
                }
                corners = null;
            }
            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.CalculatingIntrinsics)
            {
                //we can do this in the loop above to increase speed
                for (int k = 0; k < Frame_array_buffer.Length; k++)
                {

                    corners_points_list[k] = CameraCalibration.FindChessboardCorners(Frame_array_buffer[k], patternSize, Emgu.CV.CvEnum.CALIB_CB_TYPE.ADAPTIVE_THRESH);
                    //for accuracy
                    Gray_Frame.FindCornerSubPix(corners_points_list, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                    //Fill our objects list with the real world mesurments for the intrinsic calculations
                    List<MCvPoint3D32f> object_list = new List<MCvPoint3D32f>();
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            object_list.Add(new MCvPoint3D32f(j * 20.0F, i * 20.0F, 0.0F));
                        }
                    }
                    corners_object_list[k] = object_list.ToArray();
                }
                ExtrinsicCameraParameters[] ex;
                //our error should be as close to 0 as possible
                _camera.Calibration.Error = CameraCalibration.CalibrateCamera(
                    corners_object_list, 
                    corners_points_list, 
                    Gray_Frame.Size, 
                    _camera.Calibration.IntrinsicParameters, 
                    Emgu.CV.CvEnum.CALIB_TYPE.CV_CALIB_RATIONAL_MODEL, 
                    new MCvTermCriteria(30, 0.1), 
                    out ex);

                _camera.Calibration.ExtrinsicParameters = ex;

                //If Emgu.CV.CvEnum.CALIB_TYPE == CV_CALIB_USE_INTRINSIC_GUESS and/or CV_CALIB_FIX_ASPECT_RATIO are specified, some or all of fx, fy, cx, cy must be initialized before calling the function
                //if you use FIX_ASPECT_RATIO and FIX_FOCAL_LEGNTH options, these values needs to be set in the intrinsic parameters before the CalibrateCamera function is called. Otherwise 0 values are used as default.
                Console.WriteLine("Intrinsic Calculation Error: " + _camera.Calibration.Error); //display the results to the user
                _camera.Calibration.CurrentMode = CameraCalibrationMode.Calibrated;
            }
            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.Calibrated)
            {
                //display the original image
                //Sub_PicturBox.Image = img.ToBitmap();
                //calculate the camera intrinsics
                Matrix<float> Map1, Map2;
                _camera.Calibration.IntrinsicParameters.InitUndistortMap(img.Width, img.Height, out Map1, out Map2);

                //remap the image to the particular intrinsics
                //In the current version of EMGU any pixel that is not corrected is set to transparent allowing the original image to be displayed if the same
                //image is mapped backed, in the future this should be controllable through the flag '0'
                Image<Bgr, Byte> temp = img.CopyBlank();
                CvInvoke.cvRemap(img, temp, Map1, Map2, 0, new MCvScalar(0));
                img = temp.Copy();

                //set up to allow another calculation
                //SetButtonState(true);
                _camera.Calibration.StartFlag = false;
            }
            if(!_ctsCameraCalibration.IsCancellationRequested)
            {
                BitmapSource bitmapSource = BitmapHelper.ToBitmapSource(img);
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
            if (_camera.TrackerId == 0)
            {
                _capture = new Capture(_camera.TrackerId);
                _ctsCameraCalibration = new CancellationTokenSource();
                CancellationToken token = _ctsCameraCalibration.Token;

                _capture.ImageGrabbed += _Capture_ImageGrabbed;

                _capture.Start();
                try
                {
                    await Task.Run(() =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            if (_camera.Calibration.CurrentMode == CameraCalibrationMode.Calibrated)
                            {

                            }
                            Thread.Sleep(100);
                        }
                    });
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

        public void StopCapture()
        {
            if (_ctsCameraCalibration != null)
            {
                _ctsCameraCalibration.Cancel();
            }
        }

        public void StartCalibration()
        {
            if (_camera.Calibration.CurrentMode != CameraCalibrationMode.SavingFrames) _camera.Calibration.CurrentMode = CameraCalibrationMode.SavingFrames;
            _capture.ImageGrabbed += _Capture_ImageGrabbed;
            //set up the arrays needed
            Frame_array_buffer = new Image<Gray, byte>[_camera.Calibration.FrameBufferSize];
            corners_object_list = new MCvPoint3D32f[Frame_array_buffer.Length][];
            corners_points_list = new PointF[Frame_array_buffer.Length][];
            frame_buffer_savepoint = 0;
            //allow the start
            _camera.Calibration.StartFlag = true;
        }

        public void SaveCalibration()
        {
            _settingsService.SaveCalibration(_camera.Calibration);
        }
    } // CameraCalibrationService
} // namespace
