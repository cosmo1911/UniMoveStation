using ColorWheel.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using System.ComponentModel;
using UniMoveStation.Design;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UnityEngine;

namespace UniMoveStation.ViewModel
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
            get
            {
                return _motionController;
            }
            set
            {
                Set(() => MotionController, ref _motionController, value);
            }
        }

        public IMotionControllerService MotionControllerService
        {
            get
            {
                return _motionControllerService;
            }
            set
            {
                Set(() => MotionControllerService, ref _motionControllerService, value);
            }
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
            Palette = Palette.Create(new RGBColorWheel(), System.Windows.Media.Colors.Blue, PaletteSchemaType.Analogous, 1);
            DoSelectColor(Palette);

            if (mc.Serial != null) { }
            SimpleIoc.Default.Register<MotionControllerViewModel>(() => this, mc.Serial, true);
        }

        public MotionControllerViewModel()
            : this(new MotionControllerModel(), IsInDesignModeStatic ? 
            (IMotionControllerService) new DesignMotionControllerService() 
            : new MotionControllerService())
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
                    ?? (_connectCommand = new RelayCommand<bool>(DoToggleConnectionCommand));
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
        public void DoToggleConnectionCommand(bool enabled)
        {
            if (enabled)
            {
                MotionControllerService.Initialize(MotionController.Id);
                if (MotionController.ConnectStatus == UniMove.PSMoveConnectStatus.OK)
                {
                    MotionController = MotionControllerService.Start();
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

            float r = ((byte)color.R) / 51f;
            float g = ((byte)color.G) / 51f;
            float b = ((byte)color.B) / 51f;

            MotionController.Color = new Color(r, g, b);
        }
        #endregion
    }
}