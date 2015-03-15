using System;
using System.Globalization;
using System.Windows.Data;

namespace UniMoveStation.Representation.ValueConverter
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool)) throw new InvalidOperationException(targetType + " != boolean");
        
            return !((bool) (value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
