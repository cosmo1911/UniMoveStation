﻿using System;
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
    public class MyTabContainerStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (item != null)
            {
                if (item == CollectionView.NewItemPlaceholder)
                {
                    return element.FindResource("AddButtonTabItemStyle") as Style;
                }
                else
                {
                    return element.FindResource("MyTabItemStyle") as Style;
                }
            }

            return null;
        }
    }
}