using System.Windows;
using System.Windows.Controls;
using UniMoveStation.ViewModel;

namespace UniMoveStation.View
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
            Loaded += (s, e) =>
            {
                Window.GetWindow(this).Closing +=
                    (s1, e1) =>
                    {
                        if (DataContext != null)
                        {
                            ((MotionControllerViewModel) DataContext).Cleanup();
                        }
                    };
            };
        }

        private void color_wheel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                DataContext = e.OldValue;
        }
    }
}