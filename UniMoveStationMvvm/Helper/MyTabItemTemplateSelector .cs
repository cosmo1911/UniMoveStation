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
    public class MyTabItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null)
            {

                if (item is SingleCameraViewModel)
                {
                    return element.FindResource("SingleCameraTabItemTemplate") as DataTemplate;
                }
                else if(item is CamerasViewModel)
                {
                    return element.FindResource("AllCamerasTabItemTemplate") as DataTemplate;
                }
                else if(item is MotionControllerViewModel)
                {
                    return element.FindResource("MotionControllerTabItemTemplate") as DataTemplate;
                }
                else if (item == CollectionView.NewItemPlaceholder)
                {
                    return element.FindResource("AddButtonTabItemTemplate") as DataTemplate;
                }
                else if (item is AddMotionControllerViewModel)
                {
                    return element.FindResource("AddMotionControllerItemTemplate") as DataTemplate;
                }
                else if (item is AddCameraViewModel)
                {
                    return element.FindResource("AddCameraItemTemplate") as DataTemplate;
                }
                else if (item is SettingsViewModel)
                {
                    return element.FindResource("SettingsItemTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}