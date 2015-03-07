using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.ViewModel;

namespace UniMoveStation.Selector 
{
    public class MyTabContainerStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null & element != null)
            {
                if (item == CollectionView.NewItemPlaceholder)
                {
                    return element.FindResource("AddButtonTabItemStyle") as Style;
                }
                if (item is CamerasViewModel)
                {
                    return element.FindResource("MyTabItemStyle") as Style;
                }
                if (item is MotionControllersViewModel)
                {
                    return element.FindResource("MyTabItemStyle") as Style;
                }
                if (item is ServerViewModel)
                {
                    return element.FindResource("MyTabItemStyle") as Style;
                }
                if (item is MotionControllerViewModel)
                {
                    return element.FindResource("MyClosableTabItemStyle") as Style;
                }
                if (item is CameraViewModel)
                {
                    return element.FindResource("MyClosableTabItemStyle") as Style;
                }
            }

            return null;
        }
    }
}