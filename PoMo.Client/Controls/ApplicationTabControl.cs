using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PoMo.Client.Controls
{
    public sealed class ApplicationTabControl : TabControl
    {
        public static readonly DependencyProperty CloseTabCommandProperty;

        public static readonly DependencyProperty IsLockedProperty;

        public static readonly DependencyProperty MenuItemsProperty;

        private static readonly DependencyPropertyKey _menuItemsPropertyKey;

        static ApplicationTabControl()
        {
            ApplicationTabControl.CloseTabCommandProperty = DependencyProperty.Register(nameof(ApplicationTabControl.CloseTabCommand), typeof(ICommand), typeof(ApplicationTabControl), 
                new PropertyMetadata());
            ApplicationTabControl._menuItemsPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ApplicationTabControl.MenuItems), typeof(ObservableCollection<MenuItem>), typeof(ApplicationTabControl), 
                new PropertyMetadata());
            ApplicationTabControl.MenuItemsProperty = ApplicationTabControl._menuItemsPropertyKey.DependencyProperty;
            ApplicationTabControl.IsLockedProperty = DependencyProperty.Register(nameof(ApplicationTabControl.IsLocked), typeof(bool), typeof(ApplicationTabControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationTabControl), new FrameworkPropertyMetadata(typeof(ApplicationTabControl)));
        }

        public ApplicationTabControl()
        {
            this.SetValue(ApplicationTabControl._menuItemsPropertyKey, new ObservableCollection<MenuItem>());
        }

        public bool IsLocked
        {
            get
            {
                return (bool)this.GetValue(ApplicationTabControl.IsLockedProperty);
            }
            set
            {
                this.SetValue(ApplicationTabControl.IsLockedProperty, value);
            }
        }

        public ICommand CloseTabCommand
        {
            get
            {
                return (ICommand)this.GetValue(ApplicationTabControl.CloseTabCommandProperty);
            }
            set
            {
                this.SetValue(ApplicationTabControl.CloseTabCommandProperty, value);
            }
        }

        public ObservableCollection<MenuItem> MenuItems => (ObservableCollection<MenuItem>)this.GetValue(ApplicationTabControl.MenuItemsProperty);

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
        }
    }
}