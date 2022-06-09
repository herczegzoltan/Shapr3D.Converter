using System;
using Windows.UI.Xaml.Data;

namespace Shapr3D.Converter.Ui.ValueConverters
{
    public class NotNullBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value != null;

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
           throw new NotSupportedException();
    }
}
