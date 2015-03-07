using System.Windows;
using System.Windows.Controls;
using UniMoveStation.ViewModel;

namespace UniMoveStation.Selector 
{
    public class MyTabContentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null && element != null)
            {
                if (item is CameraViewModel)
                {
                    return element.FindResource("SingleCameraTabContentTemplate") as DataTemplate;
                }
                if (item is MotionControllersViewModel)
                {
                    return element.FindResource("AllMotionControllersTabContentTemplate") as DataTemplate;
                }
                if (item is CamerasViewModel)
                {
                    return element.FindResource("AllCamerasTabContentTemplate") as DataTemplate;
                }
                if (item is MotionControllerViewModel)
                {
                    return element.FindResource("MotionControllerTabContentTemplate") as DataTemplate;
                }
                if (item is ServerViewModel)
                {
                    return element.FindResource("ServerTabContentTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}