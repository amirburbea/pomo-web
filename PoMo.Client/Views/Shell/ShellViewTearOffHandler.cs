using System.Windows;
using System.Windows.Controls;
using PoMo.Client.Controls;
using PoMo.Common.Windsor;

namespace PoMo.Client.Views.Shell
{
    internal sealed class ShellViewTearOffHandler : ITabTearOffHandler
    {
        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.RegisterAttached("IsLocked", typeof(bool), typeof(ShellViewTearOffHandler),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private readonly IFactory<ShellView> _shellViewFactory;

        public ShellViewTearOffHandler(IFactory<ShellView> shellViewFactory)
        {
            this._shellViewFactory = shellViewFactory;
        }

        public static bool GetIsLocked(TabControl tabControl)
        {
            return tabControl != null && (bool)tabControl.GetValue(ShellViewTearOffHandler.IsLockedProperty);
        }

        public static void SetIsLocked(TabControl tabControl, bool isLocked)
        {
            tabControl?.SetValue(ShellViewTearOffHandler.IsLockedProperty, isLocked);
        }

        bool ITabTearOffHandler.AllowReorder(object item, TabControl tabControl, int sourceIndex, int insertionIndex)
        {
            return true;
        }

        bool ITabTearOffHandler.AllowTargetedDrop(object item, TabControl sourceTabControl, int sourceIndex, TabControl targetTabControl, int insertionIndex)
        {
            return true;
        }

        bool ITabTearOffHandler.AllowTargetlessDrop(object item, TabControl sourceTabControl, int sourceIndex, Point dropLocation)
        {
            return true;
        }

        void ITabTearOffHandler.HandleReorder(object item, TabControl tabControl, int sourceIndex, int insertionIndex)
        {
            int tabIndex = insertionIndex - (insertionIndex > sourceIndex ? 1 : 0);
            if (tabIndex == sourceIndex)
            {
                return;
            }
            ShellView window = Window.GetWindow(tabControl) as ShellView;
            if (window == null)
            {
                return;
            }
            using (window.CreateIgnoreSelectionChangedScope())
            {
                tabControl.Items.RemoveAt(sourceIndex);
                tabControl.Items.Insert(tabIndex, item);
                tabControl.SelectedIndex = tabIndex;
            }
        }

        void ITabTearOffHandler.HandleTargetedDrop(object item, TabControl sourceTabControl, int sourceIndex, TabControl targetTabControl, int insertionIndex)
        {
            sourceTabControl.Items.RemoveAt(sourceIndex);
            targetTabControl.Items.Insert(insertionIndex, item);
            targetTabControl.SelectedIndex = insertionIndex;
            if (sourceTabControl.Items.Count != 0)
            {
                return;
            }
            ShellView sourceWindow = Window.GetWindow(sourceTabControl) as ShellView;
            if (sourceWindow == null)
            {
                return;
            }
            sourceWindow.Close();
            this._shellViewFactory.Release(sourceWindow);
        }

        void ITabTearOffHandler.HandleTargetlessDrop(object item, TabControl sourceTabControl, int sourceIndex, Point dropLocation)
        {
            ShellView sourceWindow = Window.GetWindow(sourceTabControl) as ShellView;
            if (sourceWindow == null)
            {
                return;
            }
            if (sourceTabControl.Items.Count == 1)
            {
                sourceWindow.Left = dropLocation.X;
                sourceWindow.Top = dropLocation.Y;
                sourceWindow.Activate();
            }
            else
            {
                ShellView targetWindow = this._shellViewFactory.Create();
                targetWindow.Left = dropLocation.X;
                targetWindow.Top = dropLocation.Y;
                targetWindow.Height = sourceWindow.WindowState == WindowState.Normal ? sourceWindow.ActualHeight : sourceWindow.RestoreBounds.Height;
                targetWindow.Width = sourceWindow.WindowState == WindowState.Normal ? sourceWindow.ActualWidth : sourceWindow.RestoreBounds.Width;
                sourceTabControl.Items.RemoveAt(sourceIndex);
                targetWindow.Show();
                targetWindow.Activate();
                targetWindow.TabControl.SelectedIndex = targetWindow.TabControl.Items.Add(item);
            }
        }

        bool ITabTearOffHandler.IsDragAllowed(object item, TabControl tabControl, int sourceIndex)
        {
            return !ShellViewTearOffHandler.GetIsLocked(tabControl);
        }
    }
}