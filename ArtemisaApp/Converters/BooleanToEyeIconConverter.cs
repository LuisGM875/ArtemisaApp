using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArtemisaApp.Converters
{
    public class BooleanToEyeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isVisible && isVisible ? "eye" : "eye-slash";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string icon && icon == "eye";
        }
    }
}

