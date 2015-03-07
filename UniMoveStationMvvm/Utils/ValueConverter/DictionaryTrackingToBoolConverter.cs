using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using UniMoveStation.Model;

namespace UniMoveStation.Utils.ValueConverter
{
    public class DictionaryTrackingToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null) return false;
            if (values[0] == DependencyProperty.UnsetValue) return false;
            if (values[1] == DependencyProperty.UnsetValue) return false;
            ObservableConcurrentDictionary<CameraModel, bool> dict = (ObservableConcurrentDictionary<CameraModel, bool>)values[0];
            CameraModel camera = (CameraModel)values[1];
            bool tracking = dict[camera];

            return tracking;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
