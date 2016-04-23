using System.Windows;
using System.Windows.Controls;
using PoMo.Client.Controls;
using PoMo.Common.Windsor;

namespace PoMo.Client.Shell
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
            ShellView shellView = tabControl.FindVisualTreeAncestor<ShellView>();
            //using (shellView.CreateIgnoreSelectionChangedScope())
            {
                tabControl.Items.RemoveAt(sourceIndex);
                tabControl.Items.Insert(tabIndex, item);
                tabControl.SelectedIndex = tabIndex;
            }
        }

        void ITabTearOffHandler.HandleTargetedDrop(object item, TabControl sourceTabControl, int sourceIndex, TabControl targetTabControl, int insertionIndex)
        {
            sourceTabControl.Items.Remove(item);
            targetTabControl.Items.Insert(insertionIndex, item);
            targetTabControl.SelectedIndex = insertionIndex;
            if (sourceTabControl.Items.Count != 0)
            {
                return;
            }
            ShellView view = sourceTabControl.FindVisualTreeAncestor<ShellView>();
            view.Close();
            this._shellViewFactory.Release(view);
        }

        void ITabTearOffHandler.HandleTargetlessDrop(object item, TabControl sourceTabControl, int sourceIndex, Point dropLocation)
        {
            ShellView shellView = sourceTabControl.FindVisualTreeAncestor<ShellView>();
            if (sourceTabControl.Items.Count == 1)
            {
                shellView.Left = dropLocation.X;
                shellView.Top = dropLocation.Y;
                shellView.Activate();
            }
            else
            {
                ShellView spawnView = this._shellViewFactory.Resolve();
                spawnView.Left = dropLocation.X;
                spawnView.Top = dropLocation.Y;
                spawnView.Height = shellView.WindowState == WindowState.Normal ? shellView.ActualHeight : shellView.RestoreBounds.Height;
                spawnView.Width = shellView.WindowState == WindowState.Normal ? shellView.ActualWidth : shellView.RestoreBounds.Width;
                sourceTabControl.Items.Remove(item);
                spawnView.Show();
                spawnView.Activate();
                spawnView.TabControl.SelectedIndex = spawnView.TabControl.Items.Add(item);
            }
        }

        bool ITabTearOffHandler.IsDragAllowed(object item, TabControl tabControl, int sourceIndex)
        {
            return !ShellViewTearOffHandler.GetIsLocked(tabControl);
        }
    }
}