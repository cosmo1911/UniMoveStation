using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace UniMoveStation.Utilities
{
    [ValueConversion(typeof(Nullable<bool>), typeof(ObservableNullableBool))]
    public class NullableBoolAndBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ObservableNullableBool) value).IsChecked;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(Nullable<bool>), typeof(Observable<bool>))]
    public class NullableBoolAndObservableBoolConverter : IValueConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new Observable<bool>(null)
                {
                    Value = false
                };
            else
                return new Observable<bool>(null)
                {
                    Value = ((Nullable<bool>)value).Value
                };
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return new Nullable<bool>(false);
            }
            else
            {
                return new Nullable<bool>(((Observable<bool>)value).Value);
            }
        }
    }

    [ValueConversion(typeof(double), typeof(Observable<double>))]
    public class DoubleAndObservableDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return 0;
            }
            else
            {
                return ((Observable<double>)value).Value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new Observable<double>(null)
                {
                    Value = 0
                };
            else
                return new Observable<double>(null)
                {
                    Value = (double) value
                };
        }
    }
}
