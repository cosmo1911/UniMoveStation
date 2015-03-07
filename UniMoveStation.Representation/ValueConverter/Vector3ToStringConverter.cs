using System;
using System.Globalization;
using System.Windows.Data;
using UnityEngine;

namespace UniMoveStation.Representation.ValueConverter
{
    public class Vector3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Vector3 v3 = (Vector3) value;
            return v3.ToString("F4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
