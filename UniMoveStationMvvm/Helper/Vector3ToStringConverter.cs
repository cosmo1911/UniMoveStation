using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UniMoveStation.Messages;
using UnityEngine;

namespace UniMoveStation.Helper
{
    public class Vector3ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            Vector3 v3 = (Vector3) value;
            return String.Format("{{ {0:F}, {1:F}, {2:F} }}", v3.x, v3.y, v3.z); ;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
