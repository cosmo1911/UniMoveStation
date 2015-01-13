using GalaSoft.MvvmLight.Messaging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace UniMoveStation.View
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class CameraCalibrationView : MahApps.Metro.Controls.Dialogs.BaseMetroDialog
    {
        /// <summary>
        /// Initializes a new instance of the CameraCalibrationView class.
        /// </summary>
        public CameraCalibrationView(MetroWindow parentWindow) : this(parentWindow, null)
        {
            
        }

        public CameraCalibrationView(MetroWindow parentWindow, MetroDialogSettings settings) : base(parentWindow, settings)
        {
            InitializeComponent();
        }
    }
}