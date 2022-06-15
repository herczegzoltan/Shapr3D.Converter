﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Shapr3D.Converter.Ui.ValueConverters
{
    public class EnumVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// The target enum value.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null || parameter == null || !(value is Enum)) 
            {
                return Visibility.Collapsed;
            }

            var currentState = value.ToString();
            var stateStrings = parameter.ToString();
            var found = false;

            foreach (var state in stateStrings.Split(','))
            {
                found = (currentState == state.Trim());

                if (found) 
                {
                    break;
                }
            }

            return found ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
