using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PoMo.Client.Controls;

namespace PoMo.Client.Shell
{
    partial class ShellView
    {
        public static readonly RoutedCommand CloseTabCommand = new RoutedCommand(nameof(ShellView.CloseTabCommand), typeof(ShellView));

        public static readonly RoutedCommand CreateViewCommand = new RoutedCommand(nameof(ShellView.CreateViewCommand), typeof(ShellView));

        private int _ignoreSelectionChangedCounter;

        static ShellView()
        {
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ApplicationCommands.Close, ShellView.CloseCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CreateViewCommand, ShellView.CreateViewCommand_Executed, ShellView.CreateViewCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CloseTabCommand, ShellView.CloseTabCommand_Executed));
        }

        public ShellView()
        {
            this.InitializeComponent();
        }

        public new ShellViewModel DataContext
        {
            get
            {
                return base.DataContext as ShellViewModel;
            }
            set
            {
                base.DataContext = value;
            }
        }

        public ITabTearOffHandler TabTearOffHandler
        {
            get;
            set;
        }

        internal IDisposable CreateIgnoreSelectionChangedScope()
        {
            return new IgnoreSelectionChangedScope(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (Application.Current.MainWindow.Equals(this))
            {
                ShellView other = Application.Current.Windows.OfType<ShellView>().FirstOrDefault(view => !view.Equals(this));
                if (other != null)
                {
                    Application.Current.MainWindow = other;
                }
            }
            this.CloseAllTabs();
            base.OnClosing(e);
        }

        private static void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((Window)sender).Close();
        }

        private static void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShellView shellView = (ShellView)sender;
            TabItem tabItem = (TabItem)e.Parameter;
            shellView.CloseTab(tabItem, shellView.TabControl.ItemContainerGenerator.IndexFromContainer(tabItem));
            if (shellView.TabControl.Items.Count == 0)
            {
                shellView.Close();
            }
        }

        private static void CreateViewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !ShellView.IsTabOpen(e.Parameter);
        }

        private static void CreateViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((ShellView)sender).CreateTab(e.Parameter);
        }

        private static bool IsTabOpen(object portfolio)
        {
            return Application.Current.Windows.OfType<ShellView>().SelectMany(view => view.TabControl.Items.Cast<TabItem>()).Any(tabItem => object.Equals(tabItem.DataContext, portfolio));
        }

        private void CloseAllTabs()
        {
            for (int index = this.TabControl.Items.Count - 1; index != -1; index--)
            {
                this.CloseTab((TabItem)this.TabControl.Items[index], index);
            }
        }

        private void CloseTab(TabItem item, int index)
        {
            this.TabControl.Items.RemoveAt(index);
        }

        private void CreateTab(object parameter)
        {
            TabItem item = new TabItem
            {
                DataContext = parameter,
                Header = parameter,
                Content = parameter
            };

            this.TabControl.Items.Add(item);
            this.TabControl.SelectedItem = item;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._ignoreSelectionChangedCounter != 0)
            {
                return;
            }
        }

        private sealed class IgnoreSelectionChangedScope : IDisposable
        {
            private readonly ShellView _shellView;
            private bool _isDisposed;

            public IgnoreSelectionChangedScope(ShellView shellView)
            {
                this._shellView = shellView;
                Interlocked.Increment(ref this._shellView._ignoreSelectionChangedCounter);
            }

            public void Dispose()
            {
                if (this._isDisposed)
                {
                    return;
                }
                Interlocked.Decrement(ref this._shellView._ignoreSelectionChangedCounter);
                this._isDisposed = true;
            }
        }
    }
}