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

        public static readonly RoutedCommand CreatePortfolioViewCommand = new RoutedCommand(nameof(ShellView.CreatePortfolioViewCommand), typeof(ShellView));

        public static readonly DependencyProperty PreTabContentProperty = DependencyProperty.RegisterAttached("PreTabContent", typeof(object), typeof(ShellView),
            new PropertyMetadata());

        private int _ignoreSelectionChangedCounter;

        static ShellView()
        {
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ApplicationCommands.Close, (sender, e) => ((Window)sender).Close()));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CreatePortfolioViewCommand, (sender, e) => ((ShellView)sender).CreateTab(e.Parameter), (sender, e) => e.CanExecute = !ShellView.IsTabOpen(e.Parameter)));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CloseTabCommand, ShellView.CloseTabCommand_Executed));
        }

        public ShellView(ITabTearOffHandler tabTearOffHandler)
        {
            this.TabTearOffHandler = tabTearOffHandler;
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
        }

        public static object GetPreTabContent(TabControl tabControl)
        {
            return tabControl?.GetValue(ShellView.PreTabContentProperty);
        }

        public static void SetPreTabContent(TabControl tabControl, object preTabContent)
        {
            tabControl?.SetValue(ShellView.PreTabContentProperty, preTabContent);
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