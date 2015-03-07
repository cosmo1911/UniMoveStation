using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace UniMoveStation.UI.View.Dialog
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class CameraCalibrationView : BaseMetroDialog
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