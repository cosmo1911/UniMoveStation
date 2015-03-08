using ColorWheel.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using UniMoveStation.Business.Model;
using UniMoveStation.Business.Service.Design;
using UniMoveStation.Business.Service.Interfaces;
using UniMoveStation.Common.SharpMove;
using UniMoveStation.Representation.MessengerMessage;
using UnityEngine;

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
        private RelayCommand<MetroWindow> _calibrateMagnetometerCommand;

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
            _motionController = mc;
            _motionControllerService = mcs;
            Palette = Palette.Create(new RGBColorWheel(), System.Windows.Media.Colors.Red, PaletteSchemaType.Analogous, 1);

            MotionControllerService.Initialize(MotionController);
            DoSelectColor(Palette);

            Messenger.Default.Register<AddCameraMessage>(this,
                message =>
                {
                    if(!MotionController.Tracking.ContainsKey(message.Camera))
                    {
                        MotionController.TrackerStatus.Add(message.Camera, PSMoveTrackerStatus.NotCalibrated);
                        MotionController.Tracking.Add(message.Camera, false);
                        MotionController.RawPosition.Add(message.Camera, Vector3.zero);
                    }
                });

            Messenger.Default.Register<RemoveCameraMessage>(this,
                message =>
                {
                    MotionController.TrackerStatus.Remove(message.Camera);
                    MotionController.Tracking.Remove(message.Camera);
                    MotionController.RawPosition.Remove(message.Camera);
                });

            
            if (mc.Serial != null)
            {
                SimpleIoc.Default.Register(() => this, mc.Serial, true);
                Messenger.Default.Send(new AddMotionControllerMessage(MotionController));
            }

            // initialize existing cameras
            foreach (CameraViewModel scvm in SimpleIoc.Default.GetAllCreatedInstances<CameraViewModel>())
            {
                MotionController.RawPosition.Add(scvm.Camera, Vector3.zero);
                MotionController.FusionPosition.Add(scvm.Camera, Vector3.zero);
                MotionController.CameraPosition.Add(scvm.Camera, Vector3.zero);
                MotionController.WorldPosition.Add(scvm.Camera, Vector3.zero);
                MotionController.Tracking.Add(scvm.Camera, false);
                MotionController.TrackerStatus.Add(scvm.Camera, PSMoveTrackerStatus.NotCalibrated);
            }
        }

        public MotionControllerViewModel()
            : this(new MotionControllerModel(), new DesignMotionControllerService())
        {
#if DEBUG
            if(IsInDesignMode)
            {
                
            }
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

        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand<MetroWindow> CalibrateMagnetometerCommand
        {
            get
            {
                return _calibrateMagnetometerCommand
                    ?? (_calibrateMagnetometerCommand = new RelayCommand<MetroWindow>(DoCalibrateMagnetometer));
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

            _motionControllerService.SetColor(new Color(r, g, b));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window">
        /// A reference to the main MetroWindow is needed in order to create dialogs
        /// </param>
        public void DoCalibrateMagnetometer(MetroWindow window)
        {
            // TODO move to view
            //_motionControllerService.CalibrateMagnetometer(window);
        }
        #endregion

        public override void Cleanup()
        {
            MotionControllerService.Stop();
            Messenger.Default.Send(new RemoveMotionControllerMessage(_motionController));
            SimpleIoc.Default.Unregister<MotionControllerViewModel>(_motionController.Serial);
            base.Cleanup();
        }
    } // MotionControllerViewModel
} // namespace