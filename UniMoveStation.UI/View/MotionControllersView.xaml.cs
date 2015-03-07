using System.Reflection;
using System.Windows.Controls;

namespace UniMoveStation.UI.View
{
    /// <summary>
    /// Description for AllCamerasView.
    /// </summary>
    public partial class MotionControllersView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the AllCamerasView class.
        /// </summary>
        public MotionControllersView()
        {
            InitializeComponent();
        }

        private void DataGrid_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            //ScrollViewer scrollViewer = (ScrollViewer) e.Source.GetType().BaseType.BaseType.BaseType.GetProperty("ScrollHost").GetValue(dataGrid, null);
            ItemsControl ic = e.Source as ItemsControl;
            ScrollViewer scrollViewer = ic.GetType().GetProperty("ScrollHost", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ic) as ScrollViewer;

            if (e.Delta < 0)
            {
                scrollViewer.LineRight();
                scrollViewer.LineRight();
            }
            else if (e.Delta > 0)
            {
                scrollViewer.LineLeft();
                scrollViewer.LineLeft();
            }
        }
    }
}