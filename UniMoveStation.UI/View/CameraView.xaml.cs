using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.Business.Model;
using UniMoveStation.Common.Utils;
using UniMoveStation.Representation.ViewModel;
using UniMoveStation.Representation.ViewModel.Dialog;
using UniMoveStation.UI.View.Dialog;

namespace UniMoveStation.UI.View
{
    /// <summary>
    /// Description for CameraView.
    /// </summary>
    public partial class CameraView : UserControl
    {
        #region Member
        private RelayCommand _applySelectionCommand;
        private RelayCommand _cancelSelectionCommand;
        private RelayCommand _calibrateCameraCommand;
        private CameraViewModel _viewModel;
        private BaseMetroDialog _dialog;
        private readonly MetroWindow _parentWindow;
        #endregion

        /// <summary>
        /// Initializes a new instance of the CameraView class.
        /// </summary>
        public CameraView()
        {
            InitializeComponent();
            DataContextChanged += CameraView_DataContextChanged;

            _parentWindow = (MetroWindow) Application.Current.MainWindow;
        }

        #region Commands
        /// <summary>
        /// Gets the ApplySelectionCommand.
        /// </summary>
        public RelayCommand ApplySelectionCommand
        {
            get
            {
                return _applySelectionCommand
                    ?? (_applySelectionCommand = new RelayCommand(
                        DoApplySelection,
                        () => _viewModel.Camera.Controllers.Count > 0));
            }
        }

        /// <summary>
        /// Gets the CancelSelectionCommand.
        /// </summary>
        public RelayCommand CancelSelectionCommand
        {
            get
            {
                return _cancelSelectionCommand
                    ?? (_cancelSelectionCommand = new RelayCommand(
                        DoCancelSelection,
                        () => _viewModel.Camera.Controllers.Count > 0));
            }
        }

        /// <summary>
        /// Gets the CalibrateCameraCommand.
        /// </summary>
        public RelayCommand CalibrateCameraCommand
        {
            get
            {
                return _calibrateCameraCommand
                    ?? (_calibrateCameraCommand = new RelayCommand(DoCalibrateCamera));
            }
        }
        #endregion

        #region Command Executions
        public void DoApplySelection()
        {
            foreach (MotionControllerModel mc in TrackedControllersListBox.Items)
            {
                ListBoxItem listBoxItem = (ListBoxItem)TrackedControllersListBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = WpfHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                bool isChecked = (bool)checkBox.IsChecked;
                if (isChecked)
                {
                    mc.Tracking[_viewModel.Camera] = true;
                }
                else
                {
                    mc.Tracking[_viewModel.Camera] = false;
                }
                _viewModel.ConsoleService.Write(string.Format("Tracking ({0}): {1}", mc.Name, isChecked));
            }
        } // DoApplySelection

        public void DoCancelSelection()
        {
            foreach (MotionControllerModel mc in TrackedControllersListBox.Items)
            {
                ListBoxItem listBoxItem = (ListBoxItem)TrackedControllersListBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = WpfHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                checkBox.IsChecked = mc.Tracking[_viewModel.Camera];
                _viewModel.ConsoleService.Write(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
            }
        }

        public void DoCalibrateCamera()
        {
            _viewModel.DoToggleCamera(false);
            _viewModel.DoToggleTracking(false);
            ShowDialog();
        }
        #endregion

        #region Misc
        void CameraView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is CameraViewModel)
            {
                //Console.WriteLine("old " + ((CameraViewModel)e.OldValue).Camera.Name);
            }
            if (e.NewValue is CameraViewModel)
            {
                //Console.WriteLine("new " + ((CameraViewModel)e.NewValue).Camera.Name);
                _viewModel = (CameraViewModel)DataContext;

                ApplySelectionButton.Command = ApplySelectionCommand;
                CancelSelectionButton.Command = CancelSelectionCommand;
                CalibrateButton.Command = CalibrateCameraCommand;
            }
        }

        public async void ShowDialog()
        {
            _dialog = new CameraCalibrationView(_parentWindow)
            {
                DataContext = new CameraCalibrationViewModel
                {
                    Camera = _viewModel.Camera
                }
            };

            await _parentWindow.ShowMetroDialogAsync(_dialog);
        }
        #endregion
    } // CameraView
} // namespace