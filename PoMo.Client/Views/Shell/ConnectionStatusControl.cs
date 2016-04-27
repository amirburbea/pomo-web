using System.Windows;
using System.Windows.Controls;

namespace PoMo.Client.Views.Shell
{
    public sealed class ConnectionStatusControl : Control
    {
        public static readonly DependencyProperty ConnectionStatusProperty = DependencyProperty.Register(nameof(ConnectionStatusControl.ConnectionStatus), typeof(ConnectionStatus), typeof(ConnectionStatusControl));

        static ConnectionStatusControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ConnectionStatusControl), new FrameworkPropertyMetadata(typeof(ConnectionStatusControl)));
        }

        public ConnectionStatus ConnectionStatus
        {
            get
            {
                return (ConnectionStatus)this.GetValue(ConnectionStatusControl.ConnectionStatusProperty);
            }
            set
            {
                this.SetValue(ConnectionStatusControl.ConnectionStatusProperty, value);
            }
        }
    }
}