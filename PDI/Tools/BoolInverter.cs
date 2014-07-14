using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace PDI.Tools
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class BooleanInverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
                return !(bool)value;
            throw new Exception("Only bool values supported");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
                return !(bool)value;
            throw new Exception("Only bool values supported");
        }
    }
}

