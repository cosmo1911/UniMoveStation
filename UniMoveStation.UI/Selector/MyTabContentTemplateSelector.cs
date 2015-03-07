using System;
using System.Windows;
using System.Windows.Controls;
using UniMoveStation.Representation.ViewModel;

namespace UniMoveStation.UI.Selector 
{
    public class MyTabContentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("/UniMoveStation.UI;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);  

            if (item != null)
            {
                if (item is CameraViewModel)
                {
                    return myResourceDictionary["SingleCameraTabContentTemplate"] as DataTemplate;
                }
                if (item is MotionControllersViewModel)
                {
                    return myResourceDictionary["AllMotionControllersTabContentTemplate"] as DataTemplate;
                }
                if (item is CamerasViewModel)
                {
                    return myResourceDictionary["AllCamerasTabContentTemplate"] as DataTemplate;
                }
                if (item is MotionControllerViewModel)
                {
                    return myResourceDictionary["MotionControllerTabContentTemplate"] as DataTemplate;
                }
                if (item is ServerViewModel)
                {
                    return myResourceDictionary["ServerTabContentTemplate"] as DataTemplate;
                }
            }

            return null;
        }
    }
}