using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using UniMoveStation.Business.Model;
using UniMoveStation.Common.Utils;
using UniMoveStation.Representation.ViewModel;
using UniMoveStation.UI.View.Dialog;

namespace UniMoveStation.UI.View
{
    /// <summary>
    /// Description for CameraView.
    /// </summary>
    public partial class CameraView : UserControl
    {
        #region Member
        private RelayCommand<ListBox> _applySelectionCommand;
        private RelayCommand<ListBox> _cancelSelectionCommand;
        private RelayCommand _calibrateCameraCommand;
        private CameraViewModel _cvm;
        private BaseMetroDialog _dialog;
        private MetroWindow _parentWindow;
        #endregion

        /// <summary>
        /// Initializes a new instance of the CameraView class.
        /// </summary>
        public CameraView()
        {
            InitializeComponent();
            DataContextChanged += CameraView_DataContextChanged;

            ApplySelectionButton.CommandParameter = TrackedControllersListBox;
            ApplySelectionButton.Command = ApplySelectionCommand;
            CancelSelectionButton.CommandParameter = TrackedControllersListBox;
            CancelSelectionButton.Command = CancelSelectionCommand;
            CalibrateButton.Command = CalibrateCameraCommand;

            _parentWindow = (MetroWindow) Application.Current.MainWindow;
        }

        #region Commands
        /// <summary>
        /// Gets the ApplySelectionCommand.
        /// </summary>
        public RelayCommand<ListBox> ApplySelectionCommand
        {
            get
            {
                return _applySelectionCommand
                    ?? (_applySelectionCommand = new RelayCommand<ListBox>(
                        DoApplySelection,
                        box => box.Items.Count > 0));
            }
        }

        /// <summary>
        /// Gets the CancelSelectionCommand.
        /// </summary>
        public RelayCommand<ListBox> CancelSelectionCommand
        {
            get
            {
                return _cancelSelectionCommand
                    ?? (_cancelSelectionCommand = new RelayCommand<ListBox>(
                        DoCancelSelection,
                        box => box.Items.Count > 0));
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
        public void DoApplySelection(ListBox listBox)
        {
            foreach (MotionControllerModel mc in listBox.Items)
            {
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = WpfHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                bool isChecked = (bool)checkBox.IsChecked;
                if (isChecked)
                {
                    mc.Tracking[_cvm.Camera] = true;
                }
                else
                {
                    mc.Tracking[_cvm.Camera] = false;
                }
                _cvm.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, isChecked));
            }
        } // DoApplySelection

        public void DoCancelSelection(ListBox listBox)
        {
            foreach (MotionControllerModel mc in listBox.Items)
            {
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = WpfHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                checkBox.IsChecked = mc.Tracking[_cvm.Camera];
                _cvm.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
            }
        }

        public void DoCalibrateCamera()
        {
            _cvm.DoToggleCamera(false);
            _cvm.DoToggleTracking(false);
            _cvm.CalibrationService.StartCapture();
            ShowDialog();
        }
        #endregion

        #region Misc
        void CameraView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is CameraViewModel)
            {
                //Console.WriteLine("old " + ((CameraViewModel)e.OldValue).Camera.Name);
            }
            if (e.NewValue != null && e.NewValue is CameraViewModel)
            {
                //Console.WriteLine("new " + ((CameraViewModel)e.NewValue).Camera.Name);
                _cvm = (CameraViewModel)DataContext;
            }
        }

        public async void ShowDialog()
        {
            _dialog = new CameraCalibrationView(_parentWindow)
            {
                DataContext = _cvm
            };

            await _parentWindow.ShowMetroDialogAsync(_dialog);
        }
        #endregion
    } // CameraView
} // namespace