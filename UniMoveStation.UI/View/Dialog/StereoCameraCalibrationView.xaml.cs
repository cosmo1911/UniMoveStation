using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.Representation.ViewModel.Dialog;

namespace UniMoveStation.UI.View.Dialog
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class StereoCameraCalibrationView : BaseMetroDialog
    {
        private StereoCameraCalibrationViewModel _viewModel;
        private RelayCommand _closeCommand;

        /// <summary>
        /// Initializes a new instance of the CameraCalibrationView class.
        /// </summary>
        public StereoCameraCalibrationView(MetroWindow parentWindow) : this(parentWindow, null)
        {
            
        }

        public StereoCameraCalibrationView(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
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
            _viewModel = (StereoCameraCalibrationViewModel)e.NewValue;
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
                    ?? (_closeCommand = new RelayCommand(DoClose, () => true));
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