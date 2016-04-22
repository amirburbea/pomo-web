using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PoMo.Common.Windsor;
using TaskDialogInterop;

namespace PoMo.Client.Shell
{
    partial class ShellView
    {
        public static readonly RoutedCommand CloseTabCommand = new RoutedCommand(nameof(ShellView.CloseTabCommand), typeof(ShellView));

        public static readonly RoutedCommand CreatePortfolioViewCommand = new RoutedCommand(nameof(ShellView.CreatePortfolioViewCommand), typeof(ShellView));
        
        static ShellView()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(ShellView),
                new CommandBinding(
                    ApplicationCommands.Close,
                    (sender, e) => ((Window)sender).Close()
                )
            );
            CommandManager.RegisterClassCommandBinding(
                typeof(ShellView),
                new CommandBinding(
                    ShellView.CreatePortfolioViewCommand,
                    (sender, e) => ((ShellView)sender).CreateTab(e.Parameter),
                    (sender, e) => e.CanExecute = !ShellView.IsTabOpen(e.Parameter)
                )
            );
            CommandManager.RegisterClassCommandBinding(
                typeof(ShellView),
                new CommandBinding(
                    ShellView.CloseTabCommand,
                    ShellView.CloseTabCommand_Executed
                )
            );
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

        protected override void OnClosing(CancelEventArgs e)
        {
            bool isMainWindow = Application.Current.MainWindow.Equals(this);
            if (isMainWindow)
            {
                List<ShellView> otherWindows = Application.Current.Windows.OfType<ShellView>().Where(view => !view.Equals(this)).ToList();
                if (otherWindows.Count == 0)
                {
                    this.CloseAllTabs();
                }
                else
                {
                    TaskDialogResult result = TaskDialog.Show(new TaskDialogOptions
                    {
                        Title = this.Title,
                        MainInstruction = string.Concat(
                            "Closing the main window will also close all child windows as well (you currently have ",
                            otherWindows.Count != 1 ? otherWindows.Count + " child windows" : "one child window",
                            " open)."
                        ),
                        MainIcon = VistaTaskDialogIcon.Warning,
                        AllowDialogCancellation = true,
                        CommandButtons = new[]
                        {
                            "&Proceed\nAll windows will be closed and the application will exit.",
                            "&Cancel\nThe application will continue running."
                        }
                    });
                    if (!result.CommandButtonResult.HasValue || result.CommandButtonResult.Value == 1)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        foreach (ShellView view in otherWindows)
                        {
                            view.CloseAllTabs();
                            view.Close();
                        }
                        this.CloseAllTabs();
                    }
                }
            }
            else if (this.TabControl.Items.Count != 0)
            {
                string numTabsOpen = this.TabControl.Items.Count != 1 ? this.TabControl.Items.Count + " tabs" : "one tab";
                TaskDialogResult result = TaskDialog.Show(new TaskDialogOptions
                {
                    Title = this.Title,
                    MainInstruction = string.Concat(
                        "You have attempted to close a child window with ",
                        numTabsOpen,
                        " open.  You must decide on a course of action."
                    ),
                    MainIcon = VistaTaskDialogIcon.Warning,
                    AllowDialogCancellation = true,
                    CommandButtons = new[]
                    {
                        "&Close Tabs\nThe window will close as will its associated tabs.",
                        "&Move Tabs\nYour tabs will be moved to the main window and this window will close.",
                        "&Cancel\nNo action will be taken."
                    }
                });
                if (!result.CommandButtonResult.HasValue || result.CommandButtonResult.Value == 2)
                {
                    e.Cancel = true;
                }
                else if (result.CommandButtonResult.Value == 0)
                {
                    this.CloseAllTabs();
                }
                else
                {
                    ShellView mainWindow = (ShellView)Application.Current.MainWindow;
                    while (this.TabControl.Items.Count != 0)
                    {
                        object item = this.TabControl.Items[0];
                        this.TabControl.Items.RemoveAt(0);
                        mainWindow.TabControl.Items.Add(item);
                    }
                    mainWindow.TabControl.SelectedIndex = mainWindow.TabControl.Items.Count - 1;
                }
            }
            base.OnClosing(e);
        }

        private static void CloseTabCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ShellView shellView = (ShellView)sender;
            shellView.CloseTab((TabItem)e.Parameter);
            if (shellView.TabControl.Items.Count == 0 && !shellView.Equals(Application.Current.MainWindow))
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
                this.CloseTab((TabItem)this.TabControl.Items[index]);
            }
        }

        private void CloseTab(TabItem item)
        {
            this.TabControl.Items.Remove(item);
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
        }
    }
}