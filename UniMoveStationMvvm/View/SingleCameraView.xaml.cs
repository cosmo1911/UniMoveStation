using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UniMoveStation.Model;
using UniMoveStation.Service;
using UniMoveStation.Utilities;
using UniMoveStation.ViewModel;

namespace UniMoveStation.View
{
    /// <summary>
    /// Description for CameraView.
    /// </summary>
    public partial class SingleCameraView : UserControl
    {
        private RelayCommand<ListBox> _applySelectionCommand;
        private RelayCommand<ListBox> _cancelSelectionCommand;
        private SingleCameraViewModel _scvm;

        /// <summary>
        /// Initializes a new instance of the CameraView class.
        /// </summary>
        public SingleCameraView()
        {
            InitializeComponent();
            DataContextChanged += SingleCameraView_DataContextChanged;

            ApplySelectionButton.Command = ApplySelectionCommand;
            ApplySelectionButton.CommandParameter = TrackedControllersListBox;
            CancelSelectionButton.Command = CancelSelectionCommand;
            CancelSelectionButton.CommandParameter = TrackedControllersListBox;
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
                        (box) => box.Items.Count > 0));
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
                        (box) => box.Items.Count > 0));
            }
        }
        #endregion

        void SingleCameraView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is SingleCameraViewModel)
            {
                //Console.WriteLine("old " + ((SingleCameraViewModel)e.OldValue).Camera.Name);

            }
            if (e.NewValue != null && e.NewValue is SingleCameraViewModel)
            {
                //Console.WriteLine("new " + ((SingleCameraViewModel)e.NewValue).Camera.Name);
                _scvm = (SingleCameraViewModel) DataContext;
            }
        }

        public void DoApplySelection(ListBox listBox)
        {
            int index = -1;
            foreach (MotionControllerModel mc in listBox.Items)
            {
                index++;
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = Utils.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                bool isChecked = (bool)checkBox.IsChecked;
                if (isChecked)
                {
                    mc.Tracking[_scvm.Camera] = true;
                }
                else
                {
                    mc.Tracking[_scvm.Camera] = false;
                }
                _scvm.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, isChecked));
            }
        } // DoApplySelection

        public void DoCancelSelection(ListBox listBox)
        {
            int index = -1;
            foreach (MotionControllerModel mc in listBox.Items)
            {
                index++;
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(mc);
                ContentPresenter contentPresenter = Utils.FindVisualChild<ContentPresenter>(listBoxItem);
                DataTemplate dataTemplate = contentPresenter.ContentTemplate;
                CheckBox checkBox = (CheckBox)dataTemplate.FindName("CheckBox", contentPresenter);
                checkBox.IsChecked = mc.Tracking[_scvm.Camera];
                _scvm.ConsoleService.WriteLine(string.Format("Tracking ({0}): {1}", mc.Name, checkBox.IsChecked));
            }
        }
    } // SingleCameraView
} // namespace