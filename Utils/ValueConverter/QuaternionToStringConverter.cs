using System;
using System.Globalization;
using System.Windows.Data;
using UnityEngine;

namespace UniMoveStation.Utils.ValueConverter
{
    public class QuaternionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Quaternion q = (Quaternion) value;
            return q.ToString("F4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
