using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Shapr3D.Converter.Ui.ValueConverters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var isReverse = (parameter as string ?? string.Empty).Equals("Reverse");

            if ((bool)value && isReverse == false)
            {
                return Visibility.Visible;
            }
            else if ((bool)value == false && isReverse)
            {
                return Visibility.Visible;
            }
            else if ((bool)value == false && isReverse == false)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (Visibility)value == Visibility.Visible ^ (parameter as string ?? string.Empty).Equals("Reverse");
    }
}
