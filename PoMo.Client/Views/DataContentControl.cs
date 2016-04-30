using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PoMo.Client.Views
{
    public sealed class DataContentControl : ContentControl
    {
        public static readonly DependencyProperty IsWaitingProperty = DependencyProperty.Register(nameof(DataContentControl.IsWaiting), typeof(bool), typeof(DataContentControl));

        public static readonly IMultiValueConverter CircleLocationConverter = new CircleLocationMultiValueConverter();

        public static readonly IValueConverter CircleOpacityConverter = new CircleOpacityValueConverter();

        static DataContentControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataContentControl), new FrameworkPropertyMetadata(typeof(DataContentControl)));
        }

        public bool IsWaiting
        {
            get
            {
                return (bool)this.GetValue(DataContentControl.IsWaitingProperty);
            }
            set
            {
                this.SetValue(DataContentControl.IsWaitingProperty, value);
            }
        }

        private sealed class CircleLocationMultiValueConverter : IMultiValueConverter
        {
            object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                string text;
                if (values == null || values.Length != 2 || (text = parameter as string) == null)
                {
                    return DependencyProperty.UnsetValue;
                }
                double width = Convert.ToDouble(values[0]);
                int index = Convert.ToInt32(values[1]);
                Func<double, double> function = text == "Top" ? (Func<double, double>)Math.Cos : Math.Sin;
                return (width * 2.5) + function(Math.PI + index * (Math.PI / 5d)) * (width * 2.5);
            }

            object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class CircleOpacityValueConverter : IValueConverter
        {
            object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                double index;
                return double.TryParse(value.ToString(), out index) ? 1d - index / 10d : DependencyProperty.UnsetValue;
            }

            object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}