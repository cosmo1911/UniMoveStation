using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows.Controls;
using UniMoveStation.Business.Model;
using UniMoveStation.Common.Utils;
using UniMoveStation.Representation.ViewModel;

namespace UniMoveStation.UI.View
{
    /// <summary>
    /// Description for CamerasView.
    /// </summary>
    public partial class CamerasView : UserControl
    {
        private CamerasViewModel _viewModel;
        private RelayCommand _applySelectionCommand;
        private RelayCommand _cancelSelectionCommand;

        /// <summary>
        /// Initializes a new instance of the AllCamerasView class.
        /// </summary>
        public CamerasView()
        {
            InitializeComponent();

            _viewModel = (CamerasViewModel)DataContext;

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
                        () => _viewModel.Cameras.Count > 0 && _viewModel.Controllers.Count > 0));
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
                        () => _viewModel.Cameras.Count > 0 && _viewModel.Controllers.Count > 0));
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

                foreach (CameraViewModel cameraViewModel in _viewModel.Cameras)
                {
                    if (isChecked)
                    {
                        mc.Tracking[cameraViewModel.Camera] = true;
                    }
                    else
                    {
                        mc.Tracking[cameraViewModel.Camera] = false;
                    }
                    cameraViewModel.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
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

                foreach (CameraViewModel cameraViewModel in _viewModel.Cameras)
                {
                    checkBox.IsChecked = mc.Tracking[cameraViewModel.Camera];
                    cameraViewModel.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
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
                    bool isChecked = false;
                    int trackingCount = 0;
                    foreach (CameraViewModel cameraViewModel in _viewModel.Cameras)
                    {
                        if (mc.Tracking[cameraViewModel.Camera]) trackingCount++;
                    }
                    if (trackingCount == _viewModel.Controllers.Count)
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
        #endregion
    }
}