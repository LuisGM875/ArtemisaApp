using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ArtemisaApp.Converters
{
    public class PasswordEyeIconConverter : IValueConverter
    {
        // \uf06e = eye, \uf070 = eye-slash
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isHidden = (bool)value;
            return isHidden ? "\uf070" : "\uf06e";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}