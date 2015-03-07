using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace UniMoveStation.UI.View.Dialog
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class CameraPositioningCalibrationView : BaseMetroDialog
    {
        /// <summary>
        /// Initializes a new instance of the CameraCalibrationView class.
        /// </summary>
        public CameraPositioningCalibrationView(MetroWindow parentWindow) : this(parentWindow, null)
        {
            
        }

        public CameraPositioningCalibrationView(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
        }
    }
}