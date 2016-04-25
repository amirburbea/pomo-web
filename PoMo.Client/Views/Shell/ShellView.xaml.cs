using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PoMo.Client.Controls;
using PoMo.Client.Views.FirmSummary;
using PoMo.Client.Views.Positions;
using PoMo.Common.DataObjects;
using PoMo.Common.Windsor;

namespace PoMo.Client.Views.Shell
{
    partial class ShellView
    {
        public static readonly RoutedCommand CloseTabCommand = new RoutedCommand(nameof(ShellView.CloseTabCommand), typeof(ShellView));
        public static readonly RoutedCommand CreateFirmSummaryViewCommand = new RoutedCommand(nameof(ShellView.CreateFirmSummaryViewCommand), typeof(ShellView));
        public static readonly RoutedCommand CreateViewCommand = new RoutedCommand(nameof(ShellView.CreateViewCommand), typeof(ShellView));

        internal static readonly object FirmSummaryHeader = new object();

        private readonly IFactory<FirmSummaryView> _firmSummaryViewFactory;
        private readonly IFactory<FirmSummaryViewModel> _firmSummaryViewModelFactory;
        private readonly IFactory<PositionsView> _positionsViewFactory;
        private readonly IFactory<PortfolioModel, PositionsViewModel> _positionsViewModelFactory;
        private int _ignoreSelectionChangedCounter;

        static ShellView()
        {
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ApplicationCommands.Close, ShellView.CloseCommand_Executed));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CreateFirmSummaryViewCommand, ShellView.CreateFirmSummaryViewCommand_Executed, ShellView.CreateFirmSummaryViewCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CreateViewCommand, ShellView.CreateViewCommand_Executed, ShellView.CreateViewCommand_CanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(ShellView), new CommandBinding(ShellView.CloseTabCommand, ShellView.CloseTabCommand_Executed));
        }

        public ShellView(
            IFactory<FirmSummaryView> firmSummaryViewFactory,
            IFactory<FirmSummaryViewModel> firmSummaryViewModelFactory,
            IFactory<PositionsView> positionsViewFactory,
            IFactory<PortfolioModel, PositionsViewModel> positionsViewModelFactory,
            ITabTearOffHandler tabTearOffHandler,
            ShellViewModel shellViewModel
        )
        {
            this.InitializeComponent();
            this._firmSummaryViewFactory = firmSummaryViewFactory;
            this._firmSummaryViewModelFactory = firmSummaryViewModelFactory;
            this._positionsViewFactory = positionsViewFactory;
            this._positionsViewModelFactory = positionsViewModelFactory;
            this.TabTearOffHandler = tabTearOffHandler;
            this.DataContext = shellViewModel;
            this.Loaded += this.ShellView_Loaded;
        }

        internal ShellViewModel ViewModel => this.DataContext as ShellViewModel;

        public ITabTearOffHandler TabTearOffHandler
        {
            get;
        }

        internal IDisposable CreateIgnoreSelectionChangedScope()
        {
            return new IgnoreSelectionChangedScope(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            PropertyChangedEventManager.RemoveHandler(this.ViewModel, this.ViewModel_ConnectionStatusChanged, nameof(ShellViewModel.ConnectionStatus));
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
            shellView.CloseTab(shellView.TabControl.ItemContainerGenerator.IndexFromContainer((DependencyObject)e.Parameter));
            if (shellView.TabControl.Items.Count == 0)
            {
                shellView.Close();
            }
        }

        private static void CreateFirmSummaryViewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !ShellView.IsFirmSummaryTabOpen();
        }

        private static void CreateFirmSummaryViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((ShellView)sender).CreateFirmSummaryTab();
        }

        private static void CreateViewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !ShellView.IsTabOpen((PortfolioModel)e.Parameter);
        }

        private static void CreateViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((ShellView)sender).CreateTab((PortfolioModel)e.Parameter);
        }

        private static IEnumerable<TViewModel> GetOpenTabs<TViewModel>()
        {
            return Application.Current.Windows.OfType<ShellView>()
                .SelectMany(shellView => shellView.TabControl.Items.Cast<TabItem>(), (shellView, tabItem) => tabItem.DataContext)
                .OfType<TViewModel>();
        }

        private static bool IsFirmSummaryTabOpen()
        {
            return ShellView.GetOpenTabs<FirmSummaryViewModel>().Any();
        }

        private static bool IsTabOpen(PortfolioModel portfolio)
        {
            return ShellView.GetOpenTabs<PositionsViewModel>().Any(viewModel => viewModel.Portfolio.Id == portfolio.Id);
        }

        private void CloseAllTabs()
        {
            using (this.CreateIgnoreSelectionChangedScope())
            {
                for (int index = this.TabControl.Items.Count - 1; index != -1; index--)
                {
                    this.CloseTab(index);
                }
            }
        }

        private void CloseTab(int index)
        {
            TabItem item = (TabItem)this.TabControl.Items[index];
            this.TabControl.Items.RemoveAt(index);
            PositionsViewModel positionsViewModel = item.DataContext as PositionsViewModel;
            if (positionsViewModel != null)
            {
                this._positionsViewFactory.Release((PositionsView)item.Content);
                this._positionsViewModelFactory.Release(positionsViewModel);
                return;
            }
            FirmSummaryViewModel firmSummaryViewModel = item.DataContext as FirmSummaryViewModel;
            if (firmSummaryViewModel == null)
            {
                return;
            }
            this._firmSummaryViewFactory.Release((FirmSummaryView)item.Content);
            this._firmSummaryViewModelFactory.Release(firmSummaryViewModel);
        }

        private void CreateFirmSummaryTab()
        {
            this.TabControl.SelectedIndex = this.TabControl.Items.Add(new TabItem
            {
                DataContext = this._firmSummaryViewModelFactory.Create(),
                Header = FirmSummaryModel.Instance,
                HeaderTemplate = (DataTemplate)this.TryFindResource(FontWeights.SemiBold),
                Content = this._firmSummaryViewFactory.Create()
            });
        }

        private void CreateTab(PortfolioModel portfolio)
        {
            this.TabControl.SelectedIndex = this.TabControl.Items.Add(new TabItem
            {
                DataContext = this._positionsViewModelFactory.Create(portfolio),
                Header = portfolio,
                Content = this._positionsViewFactory.Create()
            });
        }

        private void ShellView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.ShellView_Loaded;
            if (Application.Current.Windows.OfType<ShellView>().All(view => view.Equals(this)))
            {
                this.CreateFirmSummaryTab();
            }
            PropertyChangedEventManager.AddHandler(this.ViewModel, this.ViewModel_ConnectionStatusChanged, nameof(ShellViewModel.ConnectionStatus));
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._ignoreSelectionChangedCounter != 0 || this.ViewModel.ConnectionStatus != ConnectionStatus.Connected)
            {
                return;
            }
            if (e.RemovedItems.Count != 0)
            {
                (((FrameworkElement)e.RemovedItems[0]).DataContext as ISubscriber)?.Unsubscribe();
            }
            if (e.AddedItems.Count != 0)
            {
                (((FrameworkElement)e.AddedItems[0]).DataContext as ISubscriber)?.Subscribe();
            }
        }

        private void ViewModel_ConnectionStatusChanged(object sender, EventArgs e)
        {
            TabItem tabItem = (TabItem)this.TabControl.SelectedItem;
            if (tabItem != null && this.ViewModel.ConnectionStatus == ConnectionStatus.Connected)
            {
                ((ISubscriber)tabItem.DataContext).Subscribe();
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