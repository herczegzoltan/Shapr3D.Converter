using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Shapr3D.Converter.Helpers
{
    public class NotNullBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value != null;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
           throw new NotSupportedException();

    }
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (bool)value ^ (parameter as string ?? string.Empty).Equals("Reverse") ?
                Visibility.Visible : Visibility.Collapsed;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (Visibility)value == Visibility.Visible ^ (parameter as string ?? string.Empty).Equals("Reverse");

    }
}
