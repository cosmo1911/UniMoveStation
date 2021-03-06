﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using UniMoveStation.Business.Model;
using UniMoveStation.Common.Utils;

namespace UniMoveStation.Business.Service
{
    public class CameraPositioningCalibrationService
    {
        private ObservableCollection<Visual3D> _helixItems;
        private ObservableCollection<CameraModel> _cameras;
        private bool _inputAnglesManually;

        public ObservableCollection<CameraModel> Cameras
        {
            get { return _cameras ?? (_cameras = new ObservableCollection<CameraModel>()); }
            private set { _cameras = value; }
        }

        public ObservableCollection<Visual3D> HelixItems
        {
            get
            {
                if (_helixItems == null)
                {
                    _helixItems = new ObservableCollection<Visual3D>();

                    InitializeHelix();
                }
                return _helixItems;
            }
        }

        public bool InputAnglesManually
        {
            get
            {
                // TODO: allow more or less than four cameras.
                // always input angles manually if more or less than four cameras are available
                if (Cameras.Count != 4)
                {
                    return true;
                }
                return _inputAnglesManually;
            }
            set { _inputAnglesManually = value; }
        }

        #region Constructor
        public CameraPositioningCalibrationService(ObservableCollection<CameraModel> cameras)
        {
            Cameras = cameras;
        }
        #endregion

        public void ApplyInput()
        {
            _helixItems.Clear();

            InitializeHelix();

            if(Cameras.Count == 4 && !InputAnglesManually)
            {
                {
                    double a = Math.Abs(GetCamera(0).Calibration.TranslationToWorld[0, 0]) + Math.Abs(GetCamera(2).Calibration.TranslationToWorld[0, 0]);
                    double b = Math.Abs(GetCamera(0).Calibration.TranslationToWorld[2, 0]) + Math.Abs(GetCamera(2).Calibration.TranslationToWorld[2, 0]);
                    double c = Math.Sqrt(a * a + b * b);
                    double q = (a * a) / c;
                    double p = c - q;
                    double h = Math.Sqrt(p * q);
                    double alpha = (180 / Math.PI) * Math.Atan(h / p);
                    double beta = 90 - alpha;

                    GetCamera(0).Calibration.YAngle = (float)alpha;
                    GetCamera(1).Calibration.YAngle = (float)-alpha;
                    GetCamera(2).Calibration.YAngle = (float)(-180 + alpha);
                    GetCamera(3).Calibration.YAngle = (float)(180 - alpha);

                    GetCamera(0).Calibration.RotationToWorld = CvHelper.GetYRotationMatrix(GetCamera(0).Calibration.YAngle);
                    GetCamera(1).Calibration.RotationToWorld = CvHelper.GetYRotationMatrix(GetCamera(1).Calibration.YAngle);
                    GetCamera(2).Calibration.RotationToWorld = CvHelper.GetYRotationMatrix(GetCamera(2).Calibration.YAngle);
                    GetCamera(3).Calibration.RotationToWorld = CvHelper.GetYRotationMatrix(GetCamera(3).Calibration.YAngle);
                }
                {
                    double a = GetCamera(0).Calibration.TranslationToWorld[1, 0] - GetCamera(2).Calibration.TranslationToWorld[1, 0];
                    double ax = Math.Abs(GetCamera(0).Calibration.TranslationToWorld[0, 0]) + Math.Abs(GetCamera(2).Calibration.TranslationToWorld[0, 0]);
                    double az = Math.Abs(GetCamera(0).Calibration.TranslationToWorld[2, 0]) + Math.Abs(GetCamera(2).Calibration.TranslationToWorld[2, 0]);
                    double c = Math.Sqrt(ax * ax + az * az);
                    double q = (a * a) / c;
                    double p = c - q;
                    double h = Math.Sqrt(p * q);
                    double alpha = (180 / Math.PI) * Math.Atan(h / p);
                    double beta = 90 - alpha;

                    if (GetCamera(0).Calibration.TranslationToWorld[1, 0] < GetCamera(2).Calibration.TranslationToWorld[1, 0])
                    {
                        GetCamera(0).Calibration.XAngle = (float)alpha;
                        GetCamera(2).Calibration.XAngle = (float)-alpha;
                    }
                    else
                    {
                        GetCamera(0).Calibration.XAngle = (float)-alpha;
                        GetCamera(2).Calibration.XAngle = (float)alpha;
                    }
                }
                {
                    double a = GetCamera(1).Calibration.TranslationToWorld[1, 0] - GetCamera(3).Calibration.TranslationToWorld[1, 0];
                    double ax = Math.Abs(GetCamera(1).Calibration.TranslationToWorld[0, 0]) + Math.Abs(GetCamera(3).Calibration.TranslationToWorld[0, 0]);
                    double az = Math.Abs(GetCamera(1).Calibration.TranslationToWorld[2, 0]) + Math.Abs(GetCamera(3).Calibration.TranslationToWorld[2, 0]);
                    double c = Math.Sqrt(ax * ax + az * az);
                    double q = (a * a) / c;
                    double p = c - q;
                    double h = Math.Sqrt(p * q);
                    double alpha = (180 / Math.PI) * Math.Atan(h / p);
                    double beta = 90 - alpha;
                    

                    if (GetCamera(1).Calibration.TranslationToWorld[1, 0] < GetCamera(3).Calibration.TranslationToWorld[1, 0])
                    {
                        GetCamera(1).Calibration.XAngle = (float)alpha;
                        GetCamera(3).Calibration.XAngle = (float)-alpha;
                    }
                    else
                    {
                        GetCamera(1).Calibration.XAngle = (float)-alpha;
                        GetCamera(3).Calibration.XAngle = (float)alpha;
                    }
                }
            }
        }

        private CameraModel GetCamera(int position)
        {
            foreach(CameraModel cameraModel in Cameras)
            {
                if (cameraModel.Calibration.Index == position) return cameraModel;
            }
            return null;
        }

        private void InitializeHelix()
        {
            _helixItems.Add(new SunLight());
            _helixItems.Add(new GridLinesVisual3D
            {
                Width = 500,
                Length = 500
            });

            foreach (CameraModel cameraModel in Cameras)
            {
                CubeVisual3D cube = new CubeVisual3D();
                cube.SideLength = 10;
                cube.Fill = new SolidColorBrush(Colors.Blue);
                cube.Center = new Point3D(cameraModel.Calibration.TranslationToWorld[0, 0],
                    cameraModel.Calibration.TranslationToWorld[2, 0],
                    cameraModel.Calibration.TranslationToWorld[1, 0]);
                _helixItems.Add(cube);
            }

            if (Cameras.Count == 4)
            {
                ArrowVisual3D arrow = new ArrowVisual3D
                {
                    Point1 = new Point3D(
                        GetCamera(0).Calibration.TranslationToWorld[0, 0],
                        GetCamera(0).Calibration.TranslationToWorld[2, 0],
                        GetCamera(0).Calibration.TranslationToWorld[1, 0]),
                    Point2 = new Point3D(
                        GetCamera(2).Calibration.TranslationToWorld[0, 0],
                        GetCamera(2).Calibration.TranslationToWorld[2, 0],
                        GetCamera(2).Calibration.TranslationToWorld[1, 0]),
                    Fill = new SolidColorBrush(Colors.Yellow)
                };
                _helixItems.Add(arrow);

                arrow = new ArrowVisual3D
                {
                    Point1 = new Point3D(
                        GetCamera(1).Calibration.TranslationToWorld[0, 0],
                        GetCamera(1).Calibration.TranslationToWorld[2, 0],
                        GetCamera(1).Calibration.TranslationToWorld[1, 0]),
                    Point2 = new Point3D(
                        GetCamera(3).Calibration.TranslationToWorld[0, 0],
                        GetCamera(3).Calibration.TranslationToWorld[2, 0],
                        GetCamera(3).Calibration.TranslationToWorld[1, 0]),
                    Fill = new SolidColorBrush(Colors.Yellow)
                };
                _helixItems.Add(arrow);
            }

            ArrowVisual3D axis = new ArrowVisual3D
            {
                Origin = new Point3D(0, 0, 0),
                Direction = new Vector3D(100, 0, 0),
                Diameter = 2,
                Fill = new SolidColorBrush(Colors.Red)
            };
            _helixItems.Add(axis);

            axis = new ArrowVisual3D
            {
                Origin = new Point3D(0, 0, 0),
                Direction = new Vector3D(0, 100, 0),
                Diameter = 2,
                Fill = new SolidColorBrush(Colors.Green)
            };
            _helixItems.Add(axis);

            axis = new ArrowVisual3D
            {
                Origin = new Point3D(0, 0, 0),
                Direction = new Vector3D(0, 0, 100),
                Diameter = 2,
                Fill = new SolidColorBrush(Colors.Blue)
            };
            _helixItems.Add(axis);
        }
    } // CameraCalibrationService
} // namespace
