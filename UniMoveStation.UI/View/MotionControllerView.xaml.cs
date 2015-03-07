using System.Windows;
using System.Windows.Controls;

namespace UniMoveStation.UI.View
{
    /// <summary>
    /// Description for MotionControllerView.
    /// </summary>
    public partial class MotionControllerView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MotionControllerView class.
        /// </summary>
        public MotionControllerView()
        {
            InitializeComponent();
        }

        private void color_wheel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) DataContext = e.OldValue;
        }
    }
}