using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UniMoveStation.ViewModel;

namespace UniMoveStation.Helper 
{
    public class MyTabContentTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null)
            {

                if (item is SingleCameraViewModel)
                {
                    return element.FindResource("SingleCameraTabContentTemplate") as DataTemplate;
                }
                else if (item is CamerasViewModel)
                {
                    return element.FindResource("AllCamerasTabContentTemplate") as DataTemplate;
                }
                else if (item is MotionControllerViewModel)
                {
                    return element.FindResource("MotionControllerTabContentTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}