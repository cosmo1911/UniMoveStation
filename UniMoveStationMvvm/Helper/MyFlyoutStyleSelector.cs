using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.ViewModel;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.Helper
{
    public class MyFlyoutStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null)
            {
                if (item is AddMotionControllerViewModel)
                {
                    return element.FindResource("AddMotionControllerFlyoutStyle") as Style;
                }
                else if (item is AddCameraViewModel)
                {
                    return element.FindResource("AddCameraFlyoutStyle") as Style;
                }
            }

            return null;
        }
    }
}