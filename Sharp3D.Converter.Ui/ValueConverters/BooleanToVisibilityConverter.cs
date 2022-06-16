using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Shapr3D.Converter.Ui.ValueConverters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert boolian to visibility
        /// </summary>
        /// <param name="value">True/False</param>
        /// <param name="targetType">Not required</param>
        /// <param name="parameter">If the reversed logic is required then pass "Reverse".</param>
        /// <param name="language">Not required</param>
        /// <returns>returnes visible when true</returns>
        /// <returns>returnes collapsed when false</returns>
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
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (Visibility)value == Visibility.Visible ^ (parameter as string ?? string.Empty).Equals("Reverse");
    }
}
