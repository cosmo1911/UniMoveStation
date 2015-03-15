using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.Representation.ViewModel.Dialog;

namespace UniMoveStation.UI.View.Dialog
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class CameraCalibrationView : BaseMetroDialog
    {
        private CameraCalibrationViewModel _viewModel;
        private RelayCommand _closeCommand;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationView class.
        /// </summary>
        public CameraCalibrationView(MetroWindow parentWindow) : this(parentWindow, null)
        {
            
        }

        public CameraCalibrationView(MetroWindow parentWindow, MetroDialogSettings settings) : base(parentWindow, settings)
        {
            DataContextChanged += CameraCalibrationView_DataContextChanged;
            InitializeComponent();
        }

        void CameraCalibrationView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.CalibrationService.StopCapture();
            }
            _viewModel = (CameraCalibrationViewModel)e.NewValue;
            _viewModel.CalibrationService.StartCapture();

            CloseButton.Command = CloseCommand;
        }

        #region Commands
        /// <summary>
        /// Gets the ButtonCommand.
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand(DoClose, () => !_viewModel.Camera.Calibration.StartFlag));
            }
        }
        #endregion

        #region Command Executions
        public async void DoClose()
        {
            _viewModel.CalibrationService.StopCapture();
            await RequestCloseAsync();
        }
        #endregion
    }
}