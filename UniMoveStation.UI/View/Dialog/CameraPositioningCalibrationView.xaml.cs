using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.Representation.ViewModel;
using UniMoveStation.Representation.ViewModel.Dialog;

namespace UniMoveStation.UI.View.Dialog
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class CameraPositioningCalibrationView : BaseMetroDialog
    {
        private RelayCommand _closeCommand;
        private CameraPositioningCalibrationViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationView class.
        /// </summary>
        public CameraPositioningCalibrationView(MetroWindow parentWindow) : this(parentWindow, null)
        {
            
        }

        public CameraPositioningCalibrationView(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
            DataContextChanged += CameraPositioningCalibrationView_DataContextChanged;
            CloseButton.Command = CloseCommand;
        }

        void CameraPositioningCalibrationView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (CameraPositioningCalibrationViewModel)DataContext;
        }

        #region Commands

        /// <summary>
        /// Gets the CloseCommand.
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand(DoClose));
            }
        }
        #endregion


        public async void DoClose()
        {
            // reset unsaved settings
            foreach (CameraViewModel cameraViewModel in _viewModel.Cameras)
            {
                //cameraViewModel.Camera.Calibration = _viewModel.SettingsService.LoadCalibration(cameraViewModel.Camera.GUID);
            }
            await RequestCloseAsync();
        }
    }
}