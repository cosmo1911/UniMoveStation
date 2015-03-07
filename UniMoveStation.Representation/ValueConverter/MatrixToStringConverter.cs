using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using UnityEngine;

namespace UniMoveStation.Representation.ValueConverter
{
    public class MatrixToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == DependencyProperty.UnsetValue) return null;
            Matrix4x4 mat = (Matrix4x4) value;
            return mat.ToString("F4");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
