using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.Representation.ViewModel;

namespace UniMoveStation.UI.Selector 
{
    public class MyTabContainerStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("/UniMoveStation.UI;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);  

            if (item != null)
            {
                if (item == CollectionView.NewItemPlaceholder)
                {
                    return myResourceDictionary["AddButtonTabItemStyle"] as Style;
                }
                if (item is CamerasViewModel)
                {
                    return myResourceDictionary["MyTabItemStyle"] as Style;
                }
                if (item is MotionControllersViewModel)
                {
                    return myResourceDictionary["MyTabItemStyle"] as Style;
                }
                if (item is ServerViewModel)
                {
                    return myResourceDictionary["MyTabItemStyle"] as Style;
                }
                if (item is ClientViewModel)
                {
                    return myResourceDictionary["MyClosableTabItemStyle"] as Style;
                }
                if (item is MotionControllerViewModel)
                {
                    return myResourceDictionary["MyClosableTabItemStyle"] as Style;
                }
                if (item is CameraViewModel)
                {
                    return myResourceDictionary["MyClosableTabItemStyle"] as Style;
                }
            }

            return null;
        }
    }
}