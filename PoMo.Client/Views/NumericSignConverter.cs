using System;
using System.Globalization;
using System.Windows.Data;

namespace PoMo.Client.Views
{
    public sealed class NumericSignConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.SByte:
                    return Math.Sign((sbyte)value);
                case TypeCode.Int16:
                    return Math.Sign((short)value);
                case TypeCode.Int32:
                    return Math.Sign((int)value);
                case TypeCode.Int64:
                    return Math.Sign((long)value);
                case TypeCode.Single:
                    return Math.Sign((float)value);
                case TypeCode.Double:
                    return Math.Sign((double)value);
                case TypeCode.Decimal:
                    return Math.Sign((decimal)value);
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}