using System.Windows;
using System.Windows.Controls;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.Selector
{
    public class MyFlyoutStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null & element != null)
            {
                if (item is AddMotionControllerViewModel)
                {
                    return element.FindResource("AddMotionControllerFlyoutStyle") as Style;
                }
                if (item is AddCameraViewModel)
                {
                    return element.FindResource("AddCameraFlyoutStyle") as Style;
                }
                return element.FindResource("DefaultFlyoutStyle") as Style;
            }

            return null;
        }
    }
}