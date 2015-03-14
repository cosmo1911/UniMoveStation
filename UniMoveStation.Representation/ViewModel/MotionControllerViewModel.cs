using System.Collections.Concurrent;
using System.Linq;
using ColorWheel.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Design;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common;
using UniMoveStation.Business.PsMove;
using UniMoveStation.Representation.MessengerMessage;
using UnityEngine;
using Random = System.Random;

namespace UniMoveStation.Representation.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MotionControllerViewModel : ViewModelBase
    {
        private MotionControllerModel _motionController;
        private IMotionControllerService _motionControllerService;
        private RelayCommand<bool> _connectCommand;
        private RelayCommand<Palette> _selectColorCommand;

        #region Properties
        public MotionControllerModel MotionController
        {
            get { return _motionController; }
            set { Set(() => MotionController, ref _motionController, value); }
        }

        public IMotionControllerService MotionControllerService
        {
            get { return _motionControllerService; }
            set { Set(() => MotionControllerService, ref _motionControllerService, value); }
        }

        public Palette Palette
        {
            get;
            private set;
        }
        #endregion

        #region Constructors 
        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        public MotionControllerViewModel(MotionControllerModel mc, IMotionControllerService mcs)
        {
            if (!IsInDesignMode)
            {
                _motionController = mc;
                _motionControllerService = mcs;
                Palette = Palette.Create(new RGBColorWheel(), System.Windows.Media.Colors.Red, PaletteSchemaType.Analogous, 1);

                MotionControllerService.Initialize(MotionController);
                DoSelectColor(Palette);

                Messenger.Default.Register<AddCameraMessage>(this,
                    message =>
                    {
                        // initialize entries for newly added camera
                        if (!MotionController.Tracking.ContainsKey(message.Camera))
                        {
                            MotionController.TrackerStatus.Add(message.Camera, PSMoveTrackerStatus.NotCalibrated);
                            MotionController.Tracking.Add(message.Camera, false);
                            MotionController.RawPosition.Add(message.Camera, Float3.Zero);
                            MotionController.WorldPosition.Add(message.Camera, Float3.Zero);
                            MotionController.CameraPosition.Add(message.Camera, Float3.Zero);
                            MotionController.FusionPosition.Add(message.Camera, Float3.Zero);
                        }
                    });

                Messenger.Default.Register<RemoveCameraMessage>(this,
                    message =>
                    {
                        // remove entries corresponding to recently removed camera
                        MotionController.TrackerStatus.Remove(message.Camera);
                        MotionController.Tracking.Remove(message.Camera);
                        MotionController.RawPosition.Remove(message.Camera);
                        MotionController.WorldPosition.Remove(message.Camera);
                        MotionController.CameraPosition.Remove(message.Camera);
                        MotionController.FusionPosition.Remove(message.Camera);
                    });

                if (mc.Serial != null)
                {
                    // register with service locator
                    SimpleIoc.Default.Register(() => this, mc.Serial, true);
                    // notify existing cameras to add this controller
                    Messenger.Default.Send(new AddMotionControllerMessage(MotionController));
                }

                // initialize existing cameras
                foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
                {
                    MotionController.RawPosition.Add(scvm.Camera, Float3.Zero);
                    MotionController.FusionPosition.Add(scvm.Camera, Float3.Zero);
                    MotionController.CameraPosition.Add(scvm.Camera, Float3.Zero);
                    MotionController.WorldPosition.Add(scvm.Camera, Float3.Zero);
                    MotionController.Tracking.Add(scvm.Camera, false);
                    MotionController.TrackerStatus.Add(scvm.Camera, PSMoveTrackerStatus.NotCalibrated);
                }
            }
        }

        /// <summary>
        /// for design purposes only
        /// </summary>
        public MotionControllerViewModel()
            : this(
            new MotionControllerModel
            {
                Design = true,
                Id = MotionControllerModel.COUNTER,
                Name = "Design " + MotionControllerModel.COUNTER,
                Serial = "00:00:00:00:00:0" + MotionControllerModel.COUNTER++,
            }, 
            new DesignMotionControllerService())
        {
#if DEBUG
            Random rnd = new Random();
            if (IsInDesignMode)
            {
                CameraModel cam0 = new CameraModel { Name = "cam0" };
                CameraModel cam1 = new CameraModel { Name = "cam1" };

                MotionController = new MotionControllerModel
                {
                    Name = "Design " + rnd.Next(10),
                    Serial = "00:00:00:00:00:0" + rnd.Next(10),
                    RawPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>
                {
                    {cam0, new Vector3(rnd.Next(640), rnd.Next(480), rnd.Next(30))},
                    {cam1, new Vector3(rnd.Next(640), rnd.Next(480), rnd.Next(30))}
                },
                    FusionPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>
                {
                    {cam0, new Vector3(rnd.Next(-50, 50), rnd.Next(-50, 50), rnd.Next(-50, 50))},
                    {cam1, new Vector3(rnd.Next(640), rnd.Next(480), rnd.Next(30))}
                },
                    CameraPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>
                {
                    {cam0, new Vector3(rnd.Next(-50, 50), rnd.Next(-50, 50), rnd.Next(-50, 50))},
                    {cam1, new Vector3(rnd.Next(640), rnd.Next(480), rnd.Next(30))}
                },
                    WorldPosition = new ObservableConcurrentDictionary<CameraModel, Vector3>
                {
                    {cam0, new Vector3(rnd.Next(-50, 50), rnd.Next(-50, 50), rnd.Next(-50, 50))},
                    {cam1, new Vector3(rnd.Next(640), rnd.Next(480), rnd.Next(30))}
                }
                };
            }

            MotionController.Circle = rnd.Next(2) > 0;
            MotionController.Cross = rnd.Next(2) > 0;
            MotionController.Triangle = rnd.Next(2) > 0;
            MotionController.Square = rnd.Next(2) > 0;
            MotionController.Start = rnd.Next(2) > 0;
            MotionController.Select = rnd.Next(2) > 0;
            MotionController.Move = rnd.Next(2) > 0;
            MotionController.PS = rnd.Next(2) > 0;
            MotionController.Trigger = rnd.Next(256);
#endif
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand<bool> ToggleConnectionCommand
        {
            get
            {
                return _connectCommand
                    ?? (_connectCommand = new RelayCommand<bool>(DoToggleConnection));
            }
        }

        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand<Palette> SelectColorCommand
        {
            get
            {
                return _selectColorCommand
                    ?? (_selectColorCommand = new RelayCommand<Palette>(DoSelectColor));
            }
        }
        #endregion

        #region Command Executions
        public void DoToggleConnection(bool enabled)
        {
            if (enabled)
            {
                if (MotionController.ConnectStatus == PSMoveConnectStatus.OK)
                {
                    MotionControllerService.Start();
                    DoSelectColor(Palette);
                }
            }
            else
            {
                MotionControllerService.Stop();
            }
        }

        public void DoSelectColor(Palette palette)
        {
            System.Windows.Media.Color color = palette.Colors[0].RgbColor;

            float r = color.R / 51f;
            float g = color.G / 51f;
            float b = color.B / 51f;

            _motionControllerService.SetColor(new UnityEngine.Color(r, g, b));
        }
        #endregion

        #region Misc
        public override void Cleanup()
        {
            MotionControllerService.Stop();
            Messenger.Default.Send(new RemoveMotionControllerMessage(_motionController));
            SimpleIoc.Default.Unregister<MotionControllerViewModel>(_motionController.Serial);
            base.Cleanup();
        }

        public override string ToString()
        {
            return MotionController.Name;
        }
        #endregion
    } // MotionControllerViewModel
} // namespace