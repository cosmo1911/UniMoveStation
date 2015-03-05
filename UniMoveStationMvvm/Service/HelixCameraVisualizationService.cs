using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Threading;
using HelixToolkit.Wpf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using UniMoveStation.Model;
using UniMoveStation.SharpMove;
using UniMoveStation.Utilities;
using UniMoveStation.ViewModel;
using UnityEngine;

namespace UniMoveStation.Service
{
    public class HelixCameraVisualizationService : DependencyObject
    {
        #region Member
        private ObservableCollection<Visual3D> _items;
        private ObservableConcurrentDictionary<MotionControllerModel, SphereVisual3D> _controllerObjects;
        private CancellationTokenSource _ctsUpdate;
        private SingleCameraModel _camera;
        private Task _updateTask;
        #endregion

        #region Properties
        public ObservableCollection<Visual3D> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<Visual3D>();

                    InitializeHelix();
                }
                return _items;
            }
        }
        #endregion

        #region Constructor
        public HelixCameraVisualizationService(SingleCameraModel camera = null)
        {
            _controllerObjects = new ObservableConcurrentDictionary<MotionControllerModel, SphereVisual3D>();
        }
        #endregion

        #region Update Task
        private async void StartUpdateTask()
        {
            if (_updateTask != null && _updateTask.Status == TaskStatus.Running) return;

            _ctsUpdate = new CancellationTokenSource();
            try
            {
                _updateTask = Task.Factory.StartNew(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    while (!_ctsUpdate.Token.IsCancellationRequested)
                    {
                        sw.Restart();
                        foreach(MotionControllerModel mc in _camera.Controllers)
                        {
                            // update if the controller is selected for tracking
                            if(mc.Tracking.ContainsKey(_camera) && mc.Tracking[_camera])
                            {
                                // add if missing
                                if (!_controllerObjects.ContainsKey(mc))
                                {
                                    DispatcherHelper.UIDispatcher.Invoke((Action)(() =>
                                    {
                                        // convert color
                                        byte r = (byte)(mc.Color.r * 255 + 0.5);
                                        byte g = (byte)(mc.Color.g * 255 + 0.5);
                                        byte b = (byte)(mc.Color.b * 255 + 0.5);
                                        System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(r, g, b);

                                        SphereVisual3D sphere = new SphereVisual3D()
                                        {
                                            Center = new Point3D(mc.WorldPosition[_camera].x,
                                                mc.WorldPosition[_camera].z,
                                                mc.WorldPosition[_camera].y),
                                            Radius = ((int)((14.0 / Math.PI) * 100)) / 200.0,
                                            Fill = new SolidColorBrush(color)
                                        };
                                        _controllerObjects.Add(mc, sphere);
                                        _items.Add(sphere);
                                    }));
                                }
                                // update position
                                if (mc.WorldPosition.ContainsKey(_camera))
                                {
                                    DispatcherHelper.UIDispatcher.Invoke((Action)(() => _controllerObjects[mc].Center = new Point3D(
                                    mc.WorldPosition[_camera].x,
                                    mc.WorldPosition[_camera].z,
                                    mc.WorldPosition[_camera].y))); 
                                }
                            }
                            // remove objects corresponding to unselected controllers
                            else
                            {
                                if(_controllerObjects.ContainsKey(mc))
                                {
                                    DispatcherHelper.UIDispatcher.Invoke((Action)(() => _items.Remove(_controllerObjects[mc])));
                                    _controllerObjects.Remove(mc);
                                }
                            }
                        } // foreach
                        sw.Stop();
                        // taking the processing time of the task itself into account, pause the thread to approximately reach the given FPS 
                        Thread.Sleep((int)(Math.Max((1000.0 / (double)_camera.FPS) - sw.ElapsedMilliseconds, 0) + 0.5));
                    } // while
                }, _ctsUpdate.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                await _updateTask;
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Stop();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                Stop();
            }
            finally
            {
                // clean up
            }
        }

        private async void CancelUpdateTask()
        {
            if(_ctsUpdate != null)
            {
                _ctsUpdate.Cancel();
                await _updateTask;
            }
        }
        #endregion

        #region Interface Implementation
        public void Initialize(SingleCameraModel camera)
        {
            _camera = camera;
        }

        public void Start()
        {
            InitializeHelix();
            StartUpdateTask();
        }

        public void Stop()
        {
            CancelUpdateTask();
        }
        #endregion
        
        private void InitializeHelix()
        {
            _items.Clear();
            _controllerObjects = new ObservableConcurrentDictionary<MotionControllerModel, SphereVisual3D>();

            _items.Add(new HelixToolkit.Wpf.SunLight());
            _items.Add(new HelixToolkit.Wpf.GridLinesVisual3D()
            {
                Width = 500,
                Length = 500
            });
            RectangleVisual3D rec = new RectangleVisual3D();

            CubeVisual3D camera = new CubeVisual3D();
            camera.SideLength = 10;
            camera.Fill = new SolidColorBrush(Colors.Blue);
            camera.Center = new Point3D(_camera.Calibration.TranslationVector[0, 0],
                _camera.Calibration.TranslationVector[2, 0],
                _camera.Calibration.TranslationVector[1, 0]);
            _items.Add(camera);

            ArrowVisual3D axis = new ArrowVisual3D()
            {
                Origin = new Point3D(0, 0, 0),
                Direction = new Vector3D(100, 0, 0),
                Diameter = 2,
                Fill = new SolidColorBrush(Colors.Red)
            };
            _items.Add(axis);

            axis = new ArrowVisual3D()
            {
                Origin = new Point3D(0, 0, 0),
                Direction = new Vector3D(0, 100, 0),
                Diameter = 2,
                Fill = new SolidColorBrush(Colors.Green)
            };
            _items.Add(axis);

            axis = new ArrowVisual3D()
            {
                Origin = new Point3D(0, 0, 0),
                Direction = new Vector3D(0, 0, 100),
                Diameter = 2,
                Fill = new SolidColorBrush(Colors.Blue)
            };
            _items.Add(axis);
        }
    } // HelixCameraVisualizationService
} // namespace
