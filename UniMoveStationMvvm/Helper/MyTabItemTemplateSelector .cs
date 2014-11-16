using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using UniMoveStation.ViewModel;

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
                else if(item is AllCamerasViewModel)
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
            }

            return null;
        }
    }
}