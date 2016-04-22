using System;
using System.Globalization;
using System.Windows.Data;

namespace PoMo.Client.Controls
{
    public sealed class ValueEqualityConverter : IValueConverter, IMultiValueConverter
    {
        private static readonly object _false = false;
        private static readonly object _true = true;

        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return ValueEqualityConverter._true;
            }
            object value = values[0];
            for (int index = 1; index < values.Length; index++)
            {
                if (!object.Equals(values[index], value))
                {
                    return ValueEqualityConverter._false;
                }
            }
            return ValueEqualityConverter._true;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return object.Equals(value, parameter) ? ValueEqualityConverter._true : ValueEqualityConverter._false;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}