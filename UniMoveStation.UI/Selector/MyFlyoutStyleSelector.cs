using System;
using System.Windows;
using System.Windows.Controls;
using UniMoveStation.Representation.ViewModel.Flyout;

namespace UniMoveStation.UI.Selector
{
    public class MyFlyoutStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            var myResourceDictionary = new ResourceDictionary();
            myResourceDictionary.Source = new Uri("/UniMoveStation.UI;component/Themes/Generic.xaml", UriKind.RelativeOrAbsolute);  

            if (item != null)
            {
                if (item is AddMotionControllerViewModel)
                {
                    return myResourceDictionary["AddMotionControllerFlyoutStyle"] as Style;
                }
                if (item is AddCameraViewModel)
                {
                    return myResourceDictionary["AddCameraFlyoutStyle"] as Style;
                }
                return myResourceDictionary["DefaultFlyoutStyle"] as Style;
            }

            return null;
        }
    }
}