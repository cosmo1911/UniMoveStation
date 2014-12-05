using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using UniMoveStation.Messages;
using UniMoveStation.Model;
using UnityEngine;

namespace UniMoveStation.Helper
{
    public class DictionaryTrackingToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null) return false;
            else if (values[0] == DependencyProperty.UnsetValue) return false;
            else if (values[1] == DependencyProperty.UnsetValue) return false;
            else
            {
                ConcurrentDictionary<SingleCameraModel, bool> dict = (ConcurrentDictionary<SingleCameraModel, bool>)values[0];
                SingleCameraModel camera = (SingleCameraModel)values[1];
                bool tracking = dict[camera];

                return tracking;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
