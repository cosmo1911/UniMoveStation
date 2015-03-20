using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using MahApps.Metro.Controls;
using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.UI.View.Flyout
{
    /// <summary>
    /// Description for SettingsView.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        private MetroWindow _parentWindow;
        private RelayCommand _centerWindowCommand;
        private SettingsViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the SettingsView class.
        /// </summary>
        public SettingsView()
        {
            InitializeComponent();
            if (DataContext is SettingsViewModel)
            {
                _viewModel = (SettingsViewModel) DataContext;
                CenterWindowButton.Command = CenterWindowCommand;
            }
            DataContextChanged += SettingsView_DataContextChanged;

            _parentWindow = (MetroWindow) Application.Current.MainWindow;
            
        }

        void SettingsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is SettingsViewModel)
            {
                _viewModel = (SettingsViewModel) e.NewValue;
                CenterWindowButton.Command = CenterWindowCommand;
            }
        }

        /// <summary>
        /// Gets the CenterWindowCommand.
        /// </summary>
        public RelayCommand CenterWindowCommand
        {
            get
            {
                return _centerWindowCommand
                    ?? (_centerWindowCommand = new RelayCommand(DoCenterWindow));
            }
        }

        private void DoCenterWindow()
        {

            var screen = System.Windows.Forms.Screen.FromHandle(
                new WindowInteropHelper(_parentWindow).Handle);

            if (screen.Primary)
            {
                _viewModel.Settings.Left = screen.WorkingArea.Width / 2 - _viewModel.Settings.Width / 2;
                _viewModel.Settings.Top = screen.WorkingArea.Height / 2 - _viewModel.Settings.Height / 2;
            }
            else
            {
                if (SystemParameters.VirtualScreenHeight == SystemParameters.PrimaryScreenHeight 
                    && SystemParameters.VirtualScreenHeight == screen.Bounds.Height)
                {
                    _viewModel.Settings.Top = screen.WorkingArea.Height / 2 - _viewModel.Settings.Height / 2;
                    _viewModel.Settings.Left = SystemParameters.PrimaryScreenWidth + screen.Bounds.Width / 2 - _viewModel.Settings.Width / 2;
                }

                
            }

            
        }
    }
}