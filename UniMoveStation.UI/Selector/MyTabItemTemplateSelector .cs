using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.Representation.ViewModel;
using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.UI.Selector 
{
    public class MyTabItemTemplateSelector : DataTemplateSelector
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
                    return myResourceDictionary["SingleCameraTabItemTemplate"] as DataTemplate;
                }
                if (item is MotionControllersViewModel)
                {
                    return myResourceDictionary["AllMotionControllersTabItemTemplate"] as DataTemplate;
                }
                if (item is CamerasViewModel)
                {
                    return myResourceDictionary["AllCamerasTabItemTemplate"] as DataTemplate;
                }
                if (item is MotionControllerViewModel)
                {
                    return myResourceDictionary["MotionControllerTabItemTemplate"] as DataTemplate;
                }
                if (item is ServerViewModel)
                {
                    return myResourceDictionary["ServerTabItemTemplate"] as DataTemplate;
                }
                if (item == CollectionView.NewItemPlaceholder)
                {
                    return myResourceDictionary["AddButtonTabItemTemplate"] as DataTemplate;
                }
                if (item is AddMotionControllerViewModel)
                {
                    return myResourceDictionary["AddMotionControllerItemTemplate"] as DataTemplate;
                }
                if (item is AddCameraViewModel)
                {
                    return myResourceDictionary["AddCameraItemTemplate"] as DataTemplate;
                }
                if (item is SettingsViewModel)
                {
                    return myResourceDictionary["SettingsItemTemplate"] as DataTemplate;
                }
            }

            return null;
        }
    }
}