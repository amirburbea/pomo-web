using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PoMo.Client.Controls
{
    internal static class ControlMethods
    {
        public static UIElement GetClosestItemContainerToPosition(ItemsControl itemsControl, Point point, Orientation orientation)
        {
            Type itemContainerType = ControlMethods.GetItemContainerType(itemsControl);
            if (itemContainerType == null)
            {
                return null;
            }
            // Build a line for hit tests.
            LineGeometry line;
            switch (orientation)
            {
                case Orientation.Horizontal:
                    line = new LineGeometry(new Point(0, point.Y), new Point(itemsControl.RenderSize.Width, point.Y));
                    break;
                default:
                    line = new LineGeometry(new Point(point.X, 0), new Point(point.X, itemsControl.RenderSize.Height));
                    break;
            }
            List<UIElement> containers = new List<UIElement>();
            VisualTreeHelper.HitTest(
                itemsControl,
                null,
                hitTestResult =>
                {
                    UIElement itemContainer = hitTestResult.VisualHit.FindVisualTreeAncestor(itemContainerType) as UIElement;
                    if (itemContainer != null && itemsControl.Equals(ItemsControl.ItemsControlFromItemContainer(itemContainer)))
                    {
                        containers.Add(itemContainer);
                    }
                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(line)
            );
            switch (containers.Count)
            {
                case 0:
                    return null;
                case 1:
                    return containers[0];
            }
            // Find closest item to the point clicked.
            UIElement closest = null;
            double closestDistance = double.MaxValue;
            foreach (UIElement container in containers)
            {
                Point transform = container.TransformToAncestor(itemsControl).Transform(default(Point));
                double distance = Math.Abs(orientation == Orientation.Horizontal ? point.X - transform.X : point.Y - transform.Y);
                if (distance < closestDistance)
                {
                    closest = container;
                    closestDistance = distance;
                }
            }
            return closest;
        }

        public static UIElement GetItemContainer(this ItemsControl itemsControl, UIElement child)
        {
            Type itemContainerType = ControlMethods.GetItemContainerType(itemsControl);
            if (itemContainerType == null)
            {
                return null;
            }
            for (UIElement element = VisualTreeHelper.GetParent(child) as UIElement; element != null && !itemsControl.Equals(element); element = VisualTreeHelper.GetParent(element) as UIElement)
            {
                if (itemContainerType.IsInstanceOfType(element) && itemsControl.Equals(ItemsControl.ItemsControlFromItemContainer(element)))
                {
                    return element;
                }
            }
            return null;
        }

        public static UIElement GetItemContainerAtPosition(this ItemsControl itemsControl, Point position)
        {
            UIElement child = itemsControl.InputHitTest(position) as UIElement;
            return child == null ? null : itemsControl.GetItemContainer(child);
        }

        public static Orientation GetOrientation(this Panel panel)
        {
            if (panel == null)
            {
                return 0;
            }
            WrapPanel wrapPanel = panel as WrapPanel;
            if (wrapPanel != null)
            {
                return wrapPanel.Orientation;
            }
            TabPanel tabPanel = panel as TabPanel;
            if (tabPanel == null)
            {
                return panel.HasLogicalOrientationPublic ? panel.LogicalOrientationPublic : Orientation.Vertical;
            }
            TabControl tabControl = tabPanel.FindVisualTreeAncestor<TabControl>();
            return tabControl != null && tabControl.TabStripPlacement != Dock.Left && tabControl.TabStripPlacement != Dock.Right ? Orientation.Horizontal : Orientation.Vertical;
        }

        public static Panel GetPanel(ItemsControl itemsControl, UIElement container = null)
        {
            if (itemsControl == null)
            {
                return null;
            }
            if (itemsControl is TabControl)
            {
                TabPanel tabPanel = itemsControl.GetVisualTreeDescendents<TabPanel>().FirstOrDefault();
                if (tabPanel != null)
                {
                    return tabPanel;
                }
            }
            DependencyObject itemsPresenter = null;
            if (itemsControl.Items.Count != 0)
            {
                DependencyObject d = container ?? itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
                if (d == null)
                {
                    return null;
                }
                for (d = VisualTreeHelper.GetParent(d); d != null && !itemsControl.Equals(d); d = VisualTreeHelper.GetParent(d))
                {
                    if ((itemsPresenter = d as ItemsPresenter) != null)
                    {
                        break;
                    }
                }
            }
            return itemsPresenter != null ? VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel : null;
        }

        private static Type GetItemContainerType(ItemsControl itemsControl)
        {
            if (itemsControl == null)
            {
                return null;
            }
            if (itemsControl is ListView)
            {
                return typeof(ListViewItem);
            }
            if (itemsControl is ListBox)
            {
                return typeof(ListBoxItem);
            }
            if (itemsControl is TabControl)
            {
                return typeof(TabItem);
            }
            if (itemsControl is ComboBox)
            {
                return typeof(ComboBoxItem);
            }
            if (itemsControl is TreeView)
            {
                return typeof(TreeViewItem);
            }
            if (itemsControl.ItemContainerStyle != null)
            {
                return itemsControl.ItemContainerStyle.TargetType;
            }
            return itemsControl.Items.Count == 0 ? null : itemsControl.ItemContainerGenerator.ContainerFromIndex(0).GetType();
        }
    }
}