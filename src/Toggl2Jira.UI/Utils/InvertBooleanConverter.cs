using System;
using System.Globalization;
using System.Windows.Data;

namespace Toggl2Jira.UI.Utils
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBooleanConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value as bool?;
            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value as bool?;
            return !boolValue;
        }
    }
}