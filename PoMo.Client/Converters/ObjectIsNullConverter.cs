using System;
using System.Globalization;
using System.Windows.Data;

namespace PoMo.Client.Converters
{
    public sealed class ObjectIsNullConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}