using System;
using System.Globalization;
using System.Windows.Data;
using PoMo.Client.DataBoundObjects;

namespace PoMo.Client.Views
{
    public class DataBoundObjectColumnSignConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string columnName = (string)values[0];
            DataBoundObject row = (DataBoundObject)values[1];
            return Math.Sign(Convert.ToDecimal(row.GetValue(columnName)));
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}