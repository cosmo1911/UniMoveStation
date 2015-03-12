using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Controls;
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
    /// Description for CamerasView.
    /// </summary>
    public partial class CamerasView : UserControl
    {
        private BaseMetroDialog _dialog;
        private readonly MetroWindow _parentWindow;
        private readonly CamerasViewModel _viewModel;
        private RelayCommand _positioningCalibrationCommand;
        private RelayCommand _applySelectionCommand;
        private RelayCommand _cancelSelectionCommand;

        /// <summary>
        /// Initializes a new instance of the AllCamerasView class.
        /// </summary>
        public CamerasView()
        {
            InitializeComponent();

            _viewModel = (CamerasViewModel)DataContext;
            _parentWindow = (MetroWindow) Application.Current.MainWindow;

            PositioningCalibrationButton.Command = PositioningCalibrationCommand;
            ApplySelectionButton.Command = ApplySelectionCommand;
            CancelSelectionButton.Command = CancelSelectionCommand;
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
                        () => _viewModel.CamerasModel.Cameras.Count > 0 && _viewModel.CamerasModel.Controllers.Count > 0));
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
                        () => _viewModel.CamerasModel.Cameras.Count > 0 && _viewModel.CamerasModel.Controllers.Count > 0));
            }
        }

        /// <summary>
        /// Gets the PositioningCalibrationCommand.
        /// </summary>
        public RelayCommand PositioningCalibrationCommand
        {
            get
            {
                return _positioningCalibrationCommand
                    ?? (_positioningCalibrationCommand = new RelayCommand(
                        ShowCameraPositioningDialog,
                        () => _viewModel.CamerasModel.Cameras.Count > 0));
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

                foreach (CameraModel cameraModel in _viewModel.CamerasModel.Cameras)
                {
                    if (isChecked)
                    {
                        mc.Tracking[cameraModel] = true;
                    }
                    else
                    {
                        mc.Tracking[cameraModel] = false;
                    }
                    //cameraModel.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
                }
            }
        }

        public void DoCancelSelection()
        {
            foreach (MotionControllerModel mc in TrackedControllersListBox.Items)
            {
                ListBoxItem listBoxItem = (ListBoxItem)TrackedControllersListBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = WpfHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);

                foreach (CameraModel cameraModel in _viewModel.CamerasModel.Cameras)
                {
                    checkBox.IsChecked = mc.Tracking[cameraModel];
                    //cameraModel.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
                }
            }
        }
        #endregion

        #region Misc
        private void RefreshCheckBoxes()
        {
            foreach (MotionControllerModel mc in TrackedControllersListBox.Items)
            {
                ListBoxItem listBoxItem = (ListBoxItem)TrackedControllersListBox.ItemContainerGenerator.ContainerFromItem(mc);
                if (listBoxItem != null)
                {
                    ContentPresenter contentPresenter = WpfHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                    DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                    CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                    int trackingCount = 0;
                    foreach (CameraModel cameraModel in _viewModel.CamerasModel.Cameras)
                    {
                        if (mc.Tracking[cameraModel]) trackingCount++;
                    }
                    if (trackingCount == _viewModel.CamerasModel.Controllers.Count)
                    {
                        checkBox.IsChecked = true;
                    }
                    else if (trackingCount == 0)
                    {
                        checkBox.IsChecked = false;
                    }
                    else
                    {
                        checkBox.IsChecked = null;
                    }
                }
            }
        }

        public async void ShowCameraPositioningDialog()
        {
            _dialog = new CameraPositioningCalibrationView(_parentWindow)
            {
                DataContext = new CameraPositioningCalibrationViewModel(_viewModel.CamerasModel.Cameras)
            };

            await _parentWindow.ShowMetroDialogAsync(_dialog);
        }
        #endregion
    }
}