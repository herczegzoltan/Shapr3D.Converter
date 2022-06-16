using System;
using Windows.UI.Xaml.Data;

namespace Shapr3D.Converter.Ui.ValueConverters
{
    public class NotNullBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Convert nullable to boolian
        /// </summary>
        /// <param name="value">Any type</param>
        /// <param name="targetType">Not required</param>
        /// <param name="parameter">Not required</param>
        /// <param name="language">Not required</param>
        /// <returns>If the passed parameter is not null then returns true otherwise false.</returns>
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value != null;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
           throw new NotSupportedException();
    }
}
