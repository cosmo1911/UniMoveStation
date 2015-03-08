using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Ioc;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Event;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common.Utils;

namespace UniMoveStation.Business.Service
{
    public class MultipleViewsService
    {
        public ObservableCollection<CameraModel> Cameras { get; set; }
        public List<ITrackerService> TrackerServices { get; set; } 

        public MultipleViewsService()
        {
            
        }

        public void Initialize(ObservableCollection<CameraModel> cameras, List<ITrackerService> trackerServices)
        {
            Cameras = cameras;
            TrackerServices = trackerServices;

            foreach (CameraModel cameraModel in Cameras)
            {
                TrackerServices[Cameras.IndexOf(cameraModel)].OnImageReady += TrackerService_OnImageReady;
            }
        }


        private void TrackerService_OnImageReady(object sender, EventArgs e)
        {
            OnImageReadyEventArgs args = (OnImageReadyEventArgs) e;

            IEnumerable<CameraModel> orderedCameras = Cameras.OrderBy(camera => camera.Calibration.Position);

            foreach (CameraModel cameraModel1 in orderedCameras)
            {
                foreach (CameraModel cameraModel2 in orderedCameras)
                {
                    if (cameraModel1.Calibration.ObjectPointsProjected == null ||
                        cameraModel2.Calibration.ObjectPointsProjected == null) continue;
                    if (cameraModel1.Calibration.Position == cameraModel2.Calibration.Position) continue;
                    if (cameraModel1.Calibration.Position > 1) continue;

                    IntPtr points1Ptr = CvHelper.CreatePointListPointer(cameraModel1.Calibration.ObjectPointsProjected);
                    IntPtr points2Ptr = CvHelper.CreatePointListPointer(cameraModel2.Calibration.ObjectPointsProjected);

                    Matrix<double> fundamentalMatrix = new Matrix<double>(3, 3);

                    IntPtr fundamentalMatrixPtr = CvInvoke.cvCreateMat(3, 3, MAT_DEPTH.CV_32F);
                    CvInvoke.cvFindFundamentalMat(points1Ptr, points2Ptr, fundamentalMatrix.Ptr, CV_FM.CV_FM_RANSAC, 3,
                        0.99, IntPtr.Zero);

                    Matrix<double> lines1 = new Matrix<double>(8, 3);
                    CvInvoke.cvComputeCorrespondEpilines(points2Ptr, 2, fundamentalMatrix.Ptr, lines1.Ptr);

                    Matrix<double> lines2 = new Matrix<double>(8, 3);
                    CvInvoke.cvComputeCorrespondEpilines(points1Ptr, 1, fundamentalMatrix.Ptr, lines2.Ptr);

                    for (int i = 0; i < cameraModel1.Calibration.ObjectPointsProjected.Length; i++)
                    {
                        {
                            System.Drawing.Point[] points = new System.Drawing.Point[2]
                            {
                                new System.Drawing.Point(0, (int) -(lines2[i, 2]/lines2[i, 1])),
                                new System.Drawing.Point(args.Image.Cols,
                                    (int) (-(lines2[i, 2] + lines2[i, 0]*args.Image.Cols)/lines2[i, 1]))
                            };
                            args.Image.DrawPolyline(points, false, new Bgr(255, 255, 0), 1);
                        }

                        //{
                        //    System.Drawing.Point[] points = new System.Drawing.Point[2]
                        //    {
                        //        new System.Drawing.Point(0, (int) -(lines1[i, 2]/lines1[i, 1])),
                        //        new System.Drawing.Point(img.Cols, (int) (-(lines1[i, 2] + lines1[i, 0] * img.Cols) / lines1[i, 1]))
                        //    };
                        //    img.DrawPolyline(points, false, new Bgr(255, 0, 255), 1);
                        //}
                    }
                }
            }
        }

        void test()
        {
            

            
        }

    }
}
