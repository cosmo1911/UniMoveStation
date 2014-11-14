using ColorWheel.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
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
        public MotionControllerModel MotionController
        {
            get;
            set;
        }

        public IMotionControllerService MotionControllerService;

        public Palette Palette
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the MotionControllerViewModel class.
        /// </summary>
        public MotionControllerViewModel(int id)
        {
            MotionController = new MotionControllerModel();
            MotionController.Serial = "m #" + id;
            MotionController.Name = "m #" + id;

            MotionControllerService = new MotionControllerService();

            SimpleIoc ioc = (SimpleIoc) ServiceLocator.Current;
            ioc.Register(() => this, MotionController.Serial, true);

            Palette = Palette.Create(new RGBColorWheel(), System.Windows.Media.Colors.Blue, PaletteSchemaType.Analogous, 1);
        }

        private RelayCommand _connectCommand;
        private RelayCommand<Palette> _selectColorCommand;

        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand ConnectCommand
        {
            get
            {
                return _connectCommand
                    ?? (_connectCommand = new RelayCommand(DoConnect));
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

        public void DoConnect()
        {
            MotionControllerService.Start(MotionController);
        }

        public void DoSelectColor(Palette palette)
        {
            System.Windows.Media.Color color = palette.Colors[0].RgbColor;

            float r = ((byte)color.R) / 51f;
            float g = ((byte)color.G) / 51f;
            float b = ((byte)color.B) / 51f;

            MotionController.Color = new Color(r, g, b);
        }
    }
}