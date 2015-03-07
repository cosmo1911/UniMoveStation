using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.ViewModel;
using UniMoveStation.ViewModel.Flyout;

namespace UniMoveStation.Selector 
{
    public class MyTabItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null & element != null)
            {
                if (item is CameraViewModel)
                {
                    return element.FindResource("SingleCameraTabItemTemplate") as DataTemplate;
                }
                if (item is MotionControllersViewModel)
                {
                    return element.FindResource("AllMotionControllersTabItemTemplate") as DataTemplate;
                }
                if (item is CamerasViewModel)
                {
                    return element.FindResource("AllCamerasTabItemTemplate") as DataTemplate;
                }
                if (item is MotionControllerViewModel)
                {
                    return element.FindResource("MotionControllerTabItemTemplate") as DataTemplate;
                }
                if (item is ServerViewModel)
                {
                    return element.FindResource("ServerTabItemTemplate") as DataTemplate;
                }
                if (item == CollectionView.NewItemPlaceholder)
                {
                    return element.FindResource("AddButtonTabItemTemplate") as DataTemplate;
                }
                if (item is AddMotionControllerViewModel)
                {
                    return element.FindResource("AddMotionControllerItemTemplate") as DataTemplate;
                }
                if (item is AddCameraViewModel)
                {
                    return element.FindResource("AddCameraItemTemplate") as DataTemplate;
                }
                if (item is SettingsViewModel)
                {
                    return element.FindResource("SettingsItemTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}