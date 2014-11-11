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
    public class MyDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null)
            {

                if (item is SingleCameraViewModel)
                {
                    return element.FindResource("SingleCameraTemplate") as DataTemplate;
                }
                else if(item is AllCamerasViewModel)
                {
                    return element.FindResource("AllCamerasTemplate") as DataTemplate;
                }
            }

            return null;
        }
    }
}