using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace UniMoveStation.UI.View.Dialog
{
    /// <summary>
    /// Description for CameraCalibrationView.
    /// </summary>
    public partial class FcpView : BaseMetroDialog
    {
        /// <summary>
        /// Initializes a new instance of the CameraCalibrationView class.
        /// </summary>
        public FcpView(MetroWindow parentWindow) : this(parentWindow, null)
        {
            
        }

        public FcpView(MetroWindow parentWindow, MetroDialogSettings settings)
            : base(parentWindow, settings)
        {
            InitializeComponent();
        }
    }
}