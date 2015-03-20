using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.CommandWpf;
using MahApps.Metro.Controls;
using UniMoveStation.Representation.ViewModel;

namespace UniMoveStation.UI.View
{
    /// <summary>
    /// Description for MotionControllerView.
    /// </summary>
    public partial class MotionControllerView : UserControl
    {
        private RelayCommand _showMagnetometerCalibrationDialogCommand;
        private MetroWindow _parentWindow;
        private MotionControllerViewModel _viewModel;

        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand ShowMagnetometerCalibrationDialogCommand
        {
            get
            {
                return _showMagnetometerCalibrationDialogCommand
                    ?? (_showMagnetometerCalibrationDialogCommand = new RelayCommand(DoShowMagnetometerCalibrationDialog));
            }
        }

        /// <summary>
        /// Initializes a new instance of the MotionControllerView class.
        /// </summary>
        public MotionControllerView()
        {
            InitializeComponent();
            _parentWindow = (MetroWindow) Application.Current.MainWindow;
            DataContextChanged += MotionControllerView_DataContextChanged;
        }

        void MotionControllerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (MotionControllerViewModel) DataContext;
            CalibrationMagnetometerButton.Command = ShowMagnetometerCalibrationDialogCommand;
        }

        private void color_wheel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) DataContext = e.OldValue;
        }

        public void DoShowMagnetometerCalibrationDialog()
        {
            _viewModel.MotionControllerService.CalibrateMagnetometer(_parentWindow);
        }
    }
}