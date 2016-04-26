using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PoMo.Client.Controls
{
    public static class TabTearOffBehavior
    {
        public static readonly DependencyProperty HandlerProperty;
        public static readonly DependencyProperty IsDraggingOverProperty;

        private static readonly DependencyPropertyKey _isDraggingOverPropertyKey;
        private static readonly DependencyProperty _maintainZIndexProperty;
        private static readonly List<Window> _windows;
        private static readonly DependencyProperty _zIndexProperty = DependencyProperty.RegisterAttached("ZIndex", typeof(int), typeof(TabTearOffBehavior));

        private static TabItem _activeTabItem;
        private static InsertionAdorner _adorner;
        private static Point _dragStartPosition;
        private static Window _dragWindow;

        static TabTearOffBehavior()
        {
            TabTearOffBehavior._windows = new List<Window>();
            TabTearOffBehavior._maintainZIndexProperty = DependencyProperty.RegisterAttached("MaintainZIndex", typeof(bool), typeof(TabTearOffBehavior), new PropertyMetadata(TabTearOffBehavior.MaintainZIndexProperty_PropertyChanged));
            TabTearOffBehavior._isDraggingOverPropertyKey = DependencyProperty.RegisterAttachedReadOnly("IsDraggingOver", typeof(bool), typeof(TabTearOffBehavior), new PropertyMetadata(null));
            TabTearOffBehavior.IsDraggingOverProperty = TabTearOffBehavior._isDraggingOverPropertyKey.DependencyProperty;
            TabTearOffBehavior.HandlerProperty = DependencyProperty.RegisterAttached("Handler", typeof(ITabTearOffHandler), typeof(TabTearOffBehavior), new PropertyMetadata(TabTearOffBehavior.HandlerProperty_PropertyChanged));
        }

        public static ITabTearOffHandler GetHandler(TabControl tabControl)
        {
            return (ITabTearOffHandler)tabControl?.GetValue(TabTearOffBehavior.HandlerProperty);
        }

        public static bool GetIsDraggingOver(TabControl tabControl)
        {
            return tabControl != null && (bool)tabControl.GetValue(TabTearOffBehavior.IsDraggingOverProperty);
        }

        public static void SetHandler(TabControl tabControl, ITabTearOffHandler handler)
        {
            tabControl?.SetValue(TabTearOffBehavior.HandlerProperty, handler);
        }

        internal static int DetermineInsertionIndex(ItemsControl tabControl, MouseEventArgs mouseEventArgs, Orientation panelOrientation)
        {
            Point tabControlPosition = mouseEventArgs.GetPosition(tabControl);
            UIElement container = tabControl.GetItemContainerAtPosition(tabControlPosition) ?? ControlMethods.GetClosestItemContainerToPosition(tabControl, tabControlPosition, panelOrientation);
            if (container == null)
            {
                return 0;
            }
            int index = tabControl.ItemContainerGenerator.IndexFromContainer(container);
            Point containerPosition = mouseEventArgs.GetPosition(container);
            if (panelOrientation == Orientation.Vertical && containerPosition.Y > container.RenderSize.Height / 2d || 
                panelOrientation == Orientation.Horizontal && containerPosition.X > container.RenderSize.Width / 2d)
            {
                index++;
            }
            return index;
        }

        internal static void SetIsDraggingOver(DependencyObject tabControl, bool isDraggingOver)
        {
            tabControl.SetValue(TabTearOffBehavior._isDraggingOverPropertyKey, isDraggingOver);
        }

        private static void AssignZIndices()
        {
            for (int index = 0; index < TabTearOffBehavior._windows.Count; index++)
            {
                TabTearOffBehavior._windows[index].SetZIndex(index);
            }
        }

        private static Rectangle CreateRectangle(Visual sourceVisual)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(sourceVisual);
            if (bounds.IsEmpty)
            {
                return new Rectangle();
            }
            return new Rectangle
            {
                Width = bounds.Width,
                Height = bounds.Height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Fill = new VisualBrush(sourceVisual)
            };
        }

        private static TabControl FindTargetTabControl(Window dropWindow, out ITabTearOffHandler handler)
        {
            handler = null;
            foreach (TabControl tabControl in dropWindow.GetVisualTreeDescendents<TabControl>())
            {
                if ((handler = TabTearOffBehavior.GetHandler(tabControl)) != null)
                {
                    return tabControl;
                }
            }
            return null;
        }

        private static object GetContent(Control tabControl)
        {
            return tabControl.Template.FindName("ContentPanel", tabControl) ?? tabControl.Template.FindName("PART_SelectedContentHost", tabControl);
        }

        private static bool GetMaintainZIndex(Window window)
        {
            return (bool)window.GetValue(TabTearOffBehavior._maintainZIndexProperty);
        }

        private static Window GetWindowMouseIsOver(MouseEventArgs args)
        {
            return Application.Current.Windows
                .Cast<Window>()
                .Where(window => window.WindowState != WindowState.Minimized && TabTearOffBehavior.GetMaintainZIndex(window))
                .OrderBy(TabTearOffBehavior.GetZIndex)
                .FirstOrDefault(window => VisualTreeHelper.GetDescendantBounds(window).Contains(args.GetPosition(window)));
        }

        private static int GetZIndex(Window window)
        {
            return (int)window.GetValue(TabTearOffBehavior._zIndexProperty);
        }

        private static void HandlerProperty_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TabControl tabControl = d as TabControl;
            if (tabControl == null)
            {
                return;
            }
            if (e.NewValue == null && e.OldValue != null)
            {
                WeakEventManager<TabControl, MouseButtonEventArgs>.RemoveHandler(tabControl, nameof(TabControl.PreviewMouseLeftButtonDown), TabTearOffBehavior.TabControl_PreviewMouseLeftButtonDown);
                if (!tabControl.IsLoaded)
                {
                    tabControl.Loaded -= TabTearOffBehavior.TabControl_Loaded;
                }
                else
                {
                    TabTearOffBehavior.SetWindowMaintainZIndex(tabControl, false);
                }
            }
            else if (e.NewValue != null && e.OldValue == null)
            {
                WeakEventManager<TabControl, MouseButtonEventArgs>.AddHandler(tabControl, nameof(TabControl.PreviewMouseLeftButtonDown), TabTearOffBehavior.TabControl_PreviewMouseLeftButtonDown);
                if (!tabControl.IsLoaded)
                {
                    tabControl.Loaded += TabTearOffBehavior.TabControl_Loaded;
                }
                else
                {
                    TabTearOffBehavior.SetWindowMaintainZIndex(tabControl, true);
                }
            }
        }

        private static bool IsMouseInDropLocation(MouseEventArgs args, TabControl tabControl, UIElement panel)
        {
            Rect panelBounds = VisualTreeHelper.GetDescendantBounds(panel);
            // panelBounds may be empty if there are no tabs in the tab control.
            if (panelBounds.IsEmpty || panelBounds.Contains(args.GetPosition(panel)))
            {
                return true;
            }
            panelBounds = new Rect(panel.TranslatePoint(panelBounds.TopLeft, tabControl), panelBounds.Size);
            Rect tabControlBounds = VisualTreeHelper.GetDescendantBounds(tabControl);
            Rect bounds = tabControl.TabStripPlacement == Dock.Top || tabControl.TabStripPlacement == Dock.Bottom ?
                new Rect(tabControlBounds.Left, panelBounds.Top, tabControlBounds.Width, panelBounds.Height) :
                new Rect(panelBounds.Left, tabControlBounds.Top, panelBounds.Width, tabControlBounds.Height);
            return bounds.Contains(args.GetPosition(tabControl));
        }

        private static bool IsSufficientDragMove(Point point)
        {
            return Math.Abs(point.X - TabTearOffBehavior._dragStartPosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(point.Y - TabTearOffBehavior._dragStartPosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }

        private static void MaintainZIndexProperty_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = d as Window;
            if (window == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                window.Activated += TabTearOffBehavior.Window_Activated;
                window.Closed += TabTearOffBehavior.Window_Closed;
                TabTearOffBehavior._windows.Add(window);
            }
            else if (TabTearOffBehavior._windows.Remove(window))
            {
                window.ClearValue(TabTearOffBehavior._zIndexProperty);
                window.Activated -= TabTearOffBehavior.Window_Activated;
                window.Closed -= TabTearOffBehavior.Window_Closed;
            }
            TabTearOffBehavior.AssignZIndices();
        }

        private static void SetMaintainZIndex(DependencyObject window, bool maintainZIndex)
        {
            window.SetValue(TabTearOffBehavior._maintainZIndexProperty, maintainZIndex);
        }

        private static void SetWindowMaintainZIndex(DependencyObject d, bool maintainZIndex)
        {
            Window window = Window.GetWindow(d);
            if (window == null)
            {
                throw new ArgumentException("TabControl is not in a window");
            }
            TabTearOffBehavior.SetMaintainZIndex(window, maintainZIndex);
        }

        private static void SetZIndex(this Window window, int zIndex)
        {
            window.SetValue(TabTearOffBehavior._zIndexProperty, zIndex);
        }

        private static void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            tabControl.Loaded -= TabTearOffBehavior.TabControl_Loaded;
            TabTearOffBehavior.SetWindowMaintainZIndex(tabControl, true);
        }

        private static void TabControl_MouseLeaveOrLostCapture(object sender, MouseEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            tabControl.PreviewMouseLeftButtonUp -= TabTearOffBehavior.TabControl_PreviewMouseLeftButtonUp;
            tabControl.PreviewMouseMove -= TabTearOffBehavior.TabControl_PreviewMouseMove;
            tabControl.MouseLeave -= TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
            tabControl.LostMouseCapture -= TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
            if (TabTearOffBehavior._adorner != null)
            {
                TabTearOffBehavior._adorner.Detach();
                TabTearOffBehavior._adorner = null;
            }
            if (TabTearOffBehavior._dragWindow != null)
            {
                TabTearOffBehavior._dragWindow.Close();
                TabTearOffBehavior._dragWindow = null;
            }
            TabTearOffBehavior._dragStartPosition = default(Point);
            TabTearOffBehavior._activeTabItem = null;
            e.Handled = true;
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event of the TabControl.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private static void TabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            TabTearOffBehavior._activeTabItem = null;
            TabTearOffBehavior._dragStartPosition = default(Point);
            Point position = e.GetPosition(tabControl);
            HitTestResult result = VisualTreeHelper.HitTest(tabControl, position);
            if (result == null)
            {
                return;
            }
            TabItem tabItem = null;
            for (DependencyObject obj = result.VisualHit; obj != null; obj = VisualTreeHelper.GetParent(obj))
            {
                ButtonBase button = obj as ButtonBase;
                if (button != null)
                {
                    return;
                }
                if ((tabItem = obj as TabItem) != null)
                {
                    break;
                }
            }
            if (tabItem == null ||
                !tabControl.Equals(ItemsControl.ItemsControlFromItemContainer(tabItem)) ||
                !TabTearOffBehavior.GetHandler(tabControl).IsDragAllowed(tabControl.ItemContainerGenerator.ItemFromContainer(tabItem), tabControl, tabControl.ItemContainerGenerator.IndexFromContainer(tabItem)))
            {
                return;
            }
            TabTearOffBehavior._activeTabItem = tabItem;
            TabTearOffBehavior._dragStartPosition = position;
            tabControl.PreviewMouseMove += TabTearOffBehavior.TabControl_PreviewMouseMove;
            tabControl.PreviewMouseLeftButtonUp += TabTearOffBehavior.TabControl_PreviewMouseLeftButtonUp;
            tabControl.MouseLeave += TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
            tabControl.LostMouseCapture += TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
        }

        private static void TabControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TabControl sourceTabControl = (TabControl)sender;
            sourceTabControl.PreviewMouseLeftButtonUp -= TabTearOffBehavior.TabControl_PreviewMouseLeftButtonUp;
            sourceTabControl.PreviewMouseMove -= TabTearOffBehavior.TabControl_PreviewMouseMove;
            sourceTabControl.MouseLeave -= TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
            sourceTabControl.LostMouseCapture -= TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
            if (sourceTabControl.IsMouseCaptured)
            {
                TabItem tabItem = TabTearOffBehavior._activeTabItem;
                Point dragWindowLocation = new Point
                {
                    X = TabTearOffBehavior._dragWindow.Left,
                    Y = TabTearOffBehavior._dragWindow.Top
                };
                sourceTabControl.ReleaseMouseCapture();
                if (TabTearOffBehavior._adorner != null)
                {
                    TabTearOffBehavior._adorner.Detach();
                    TabTearOffBehavior._adorner = null;
                }
                TabTearOffBehavior._dragWindow.Close();
                TabTearOffBehavior._dragWindow = null;
                TabTearOffBehavior._activeTabItem = null;
                object item = sourceTabControl.ItemContainerGenerator.ItemFromContainer(tabItem);
                int sourceIndex = sourceTabControl.ItemContainerGenerator.IndexFromContainer(tabItem);
                Window dropWindow = TabTearOffBehavior.GetWindowMouseIsOver(e);
                TabControl targetTabControl;
                Panel targetPanel;
                ITabTearOffHandler targetHandler;
                if (dropWindow != null &&
                    TabTearOffBehavior.IsMouseInDropLocation(e, targetTabControl = TabTearOffBehavior.FindTargetTabControl(dropWindow, out targetHandler), targetPanel = ControlMethods.GetPanel(targetTabControl)))
                {
                    int insertionIndex = TabTearOffBehavior.DetermineInsertionIndex(targetTabControl, e, targetPanel.GetOrientation());
                    if (!object.ReferenceEquals(sourceTabControl, targetTabControl))
                    {
                        if (targetHandler.AllowTargetedDrop(item, sourceTabControl, sourceIndex, targetTabControl, insertionIndex))
                        {
                            targetHandler.HandleTargetedDrop(item, sourceTabControl, sourceIndex, targetTabControl, insertionIndex);
                        }
                    }
                    else if (sourceTabControl.Items.Count != 1 &&
                        insertionIndex != sourceIndex &&
                        targetHandler.AllowReorder(item, sourceTabControl, sourceIndex, insertionIndex))
                    {
                        targetHandler.HandleReorder(item, sourceTabControl, sourceIndex, insertionIndex);
                    }
                }
                else
                {
                    ITabTearOffHandler sourceHandler = TabTearOffBehavior.GetHandler(sourceTabControl);
                    if (sourceHandler.AllowTargetlessDrop(item, sourceTabControl, sourceIndex, dragWindowLocation))
                    {
                        sourceHandler.HandleTargetlessDrop(item, sourceTabControl, sourceIndex, dragWindowLocation);
                    }
                }
            }
            e.Handled = true;
        }

        private static void TabControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            TabControl tabControl = (TabControl)sender;
            Point point = e.GetPosition(tabControl);
            if (tabControl.IsMouseCaptured)
            {
                TabTearOffBehavior.UpdateDragWindowLocation(tabControl);
                Window dropWindow = TabTearOffBehavior.GetWindowMouseIsOver(e);
                if (dropWindow == null)
                {
                    return;
                }
                if (!dropWindow.IsActive)
                {
                    dropWindow.Activate();
                }
                ITabTearOffHandler targetHandler;
                TabControl targetTabControl = TabTearOffBehavior.FindTargetTabControl(dropWindow, out targetHandler);
                Panel panel = ControlMethods.GetPanel(targetTabControl);
                if (TabTearOffBehavior.IsMouseInDropLocation(e, targetTabControl, panel))
                {
                    if (TabTearOffBehavior._adorner == null || !TabTearOffBehavior._adorner.AdornedElement.Equals(targetTabControl))
                    {
                        if (TabTearOffBehavior._adorner != null)
                        {
                            TabTearOffBehavior._adorner.Detach();
                        }
                        TabTearOffBehavior._adorner = new InsertionAdorner(targetTabControl, panel.GetOrientation());
                    }
                    TabTearOffBehavior._adorner.UpdateLocation(e);
                }
                else if (TabTearOffBehavior._adorner != null)
                {
                    TabTearOffBehavior._adorner.Detach();
                    TabTearOffBehavior._adorner = null;
                }
            }
            else if (TabTearOffBehavior.IsSufficientDragMove(point))
            {
                Rectangle contentRectangle = TabTearOffBehavior.CreateRectangle((Visual)TabTearOffBehavior.GetContent(tabControl));
                Rectangle tabItemRectangle = TabTearOffBehavior.CreateRectangle(TabTearOffBehavior._activeTabItem);
                Panel.SetZIndex(tabItemRectangle, 10);
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = tabControl.TabStripPlacement == Dock.Right || tabControl.TabStripPlacement == Dock.Left ? Orientation.Horizontal : Orientation.Vertical,
                    Children =
                    {
                        tabItemRectangle
                    }
                };
                stackPanel.Children.Insert(tabControl.TabStripPlacement == Dock.Right || tabControl.TabStripPlacement == Dock.Bottom ? 0 : 1, contentRectangle);
                TranslateTransform translateTransform = new TranslateTransform();
                switch (tabControl.TabStripPlacement)
                {
                    case Dock.Bottom:
                        translateTransform.Y = -1d;
                        break;
                    case Dock.Left:
                        translateTransform.X = 1d;
                        break;
                    case Dock.Right:
                        translateTransform.X = -1d;
                        break;
                    case Dock.Top:
                        translateTransform.Y = 1d;
                        break;
                }
                tabItemRectangle.RenderTransform = translateTransform;
                TabTearOffBehavior._dragWindow = new Window
                {
                    Topmost = true,
                    AllowsTransparency = true,
                    WindowStyle = WindowStyle.None,
                    IsHitTestVisible = false,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Background = Brushes.Transparent,
                    Content = new Border
                    {
                        Child = stackPanel,
                        Opacity = 0.75
                    }
                };
                TabTearOffBehavior.UpdateDragWindowLocation(tabControl);
                TabTearOffBehavior._dragWindow.Show();
                tabControl.MouseLeave -= TabTearOffBehavior.TabControl_MouseLeaveOrLostCapture;
                tabControl.CaptureMouse();
                e.Handled = true;
            }
        }

        private static void UpdateDragWindowLocation(DependencyObject tabControl)
        {
            Window window = Window.GetWindow(tabControl);
            Point? targetPoint = PresentationSource.FromVisual(window)?.CompositionTarget?.TransformFromDevice.Transform(window.PointToScreen(new Point(0, 0)));
            if (!targetPoint.HasValue)
            {
                return;
            }
            Point point = Mouse.GetPosition(window);
            TabTearOffBehavior._dragWindow.Left = targetPoint.Value.X + point.X;
            TabTearOffBehavior._dragWindow.Top = targetPoint.Value.Y + point.Y;
        }

        private static void Window_Activated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            int index = TabTearOffBehavior._windows.IndexOf(window);
            if (index == 0)
            {
                return;
            }
            TabTearOffBehavior._windows.RemoveAt(index);
            TabTearOffBehavior._windows.Insert(0, window);
            TabTearOffBehavior.AssignZIndices();
        }

        private static void Window_Closed(object sender, EventArgs e)
        {
            TabTearOffBehavior.SetMaintainZIndex((Window)sender, false);
        }

        private sealed class InsertionAdorner : Adorner
        {
            private const double Size = 7d;

            private static readonly Pen _arrowBorderPen = InsertionAdorner.CreatePen();
            private static readonly Brush _arrowBrush = InsertionAdorner.CreateBrush();
            private static readonly PathGeometry _arrowGeometry = InsertionAdorner.CreateArrow();

            private readonly Orientation _orientation;
            private AdornerLayer _adornerLayer;
            private MouseEventArgs _mouseEventArgs;

            public InsertionAdorner(TabControl adornedElement, Orientation orientation)
                : base(adornedElement)
            {
                if (!adornedElement.IsLoaded)
                {
                    adornedElement.Loaded += this.AdornedElement_Loaded;
                }
                else
                {
                    if ((this._adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement)) != null)
                    {
                        this._adornerLayer.Add(this);
                    }
                    adornedElement.Unloaded += this.AdornedElement_Unloaded;
                }
                this._orientation = orientation;
                TabTearOffBehavior.SetIsDraggingOver(adornedElement, true);
                this.IsHitTestVisible = false;
            }

            public new TabControl AdornedElement => (TabControl)base.AdornedElement;

            public void Detach()
            {
                TabTearOffBehavior.SetIsDraggingOver(this.AdornedElement, false);
                this.AdornedElement.Loaded -= this.AdornedElement_Loaded;
                this.AdornedElement.Unloaded -= this.AdornedElement_Unloaded;
                if (this._adornerLayer == null)
                {
                    return;
                }
                this._adornerLayer.Remove(this);
                this._adornerLayer = null;
            }

            public void UpdateLocation(MouseEventArgs args)
            {
                this._mouseEventArgs = args;
                this.InvalidateVisual();
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                int insertionIndex = TabTearOffBehavior.DetermineInsertionIndex(this.AdornedElement, this._mouseEventArgs, this._orientation);
                ITabTearOffHandler handler = TabTearOffBehavior.GetHandler(this.AdornedElement);
                TabControl activeTabControl = TabTearOffBehavior._activeTabItem == null ? null : (TabControl)ItemsControl.ItemsControlFromItemContainer(TabTearOffBehavior._activeTabItem);
                if (activeTabControl == null ||
                    !this.AdornedElement.Equals(activeTabControl) && !handler.AllowTargetedDrop(activeTabControl.ItemContainerGenerator.ItemFromContainer(TabTearOffBehavior._activeTabItem), activeTabControl, activeTabControl.ItemContainerGenerator.IndexFromContainer(TabTearOffBehavior._activeTabItem), this.AdornedElement, insertionIndex) ||
                    this.AdornedElement.Equals(activeTabControl) && !handler.AllowReorder(activeTabControl.ItemContainerGenerator.ItemFromContainer(TabTearOffBehavior._activeTabItem), activeTabControl, activeTabControl.ItemContainerGenerator.IndexFromContainer(TabTearOffBehavior._activeTabItem), insertionIndex))
                {
                    return;
                }
                int index = Math.Min(insertionIndex, this.AdornedElement.Items.Count - 1);
                UIElement container;
                if ((container = (UIElement)this.AdornedElement.ItemContainerGenerator.ContainerFromIndex(index)) == null)
                {
                    return;
                }
                Rect itemRect = new Rect(container.TranslatePoint(default(Point), this.AdornedElement), container.RenderSize);
                Point point1, point2;
                double rotation;
                if (this._orientation == Orientation.Vertical)
                {
                    if (insertionIndex == this.AdornedElement.Items.Count)
                    {
                        itemRect.Y += container.RenderSize.Height;
                    }
                    point1 = new Point(itemRect.X - InsertionAdorner.Size, itemRect.Y);
                    point2 = new Point(itemRect.Right + InsertionAdorner.Size, itemRect.Y);
                    rotation = 0;
                }
                else
                {
                    if (insertionIndex == this.AdornedElement.Items.Count)
                    {
                        itemRect.X += container.RenderSize.Width;
                    }
                    point1 = new Point(itemRect.X, itemRect.Y - InsertionAdorner.Size);
                    point2 = new Point(itemRect.X, itemRect.Bottom + InsertionAdorner.Size);
                    rotation = 90d;
                }
                InsertionAdorner.DrawArrow(drawingContext, point1, rotation);
                InsertionAdorner.DrawArrow(drawingContext, point2, 180d + rotation);
            }

            private static PathGeometry CreateArrow()
            {
                PathFigure figure = new PathFigure { StartPoint = new Point(InsertionAdorner.Size, 0) };
                InsertionAdorner.CreateLineSegments(
                    figure,
                    new Point(0, -InsertionAdorner.Size),
                    new Point(0, -InsertionAdorner.Size / 3d),
                    new Point(-InsertionAdorner.Size, -InsertionAdorner.Size / 3d),
                    new Point(-InsertionAdorner.Size, InsertionAdorner.Size / 3d),
                    new Point(0, InsertionAdorner.Size / 3d),
                    new Point(0, InsertionAdorner.Size),
                    new Point(InsertionAdorner.Size, 0)
                );
                PathGeometry arrow = new PathGeometry { Figures = { figure } };
                figure.Freeze();
                arrow.Freeze();
                return arrow;
            }

            private static Brush CreateBrush()
            {
                RadialGradientBrush brush = new RadialGradientBrush(Colors.Black, Colors.White);
                brush.Freeze();
                return brush;
            }

            private static void CreateLineSegments(PathFigure figure, params Point[] points)
            {
                foreach (LineSegment segment in (points ?? Enumerable.Empty<Point>()).Select(point => new LineSegment(point, true)))
                {
                    figure.Segments.Add(segment);
                    segment.Freeze();
                }
            }

            private static Pen CreatePen()
            {
                Pen pen = new Pen(Brushes.Black, 0.75);
                pen.Freeze();
                return pen;
            }

            private static void DrawArrow(DrawingContext drawingContext, Point origin, double rotation)
            {
                drawingContext.PushTransform(new TranslateTransform(origin.X, origin.Y));
                drawingContext.PushTransform(new RotateTransform(rotation));
                drawingContext.DrawGeometry(InsertionAdorner._arrowBrush, InsertionAdorner._arrowBorderPen, InsertionAdorner._arrowGeometry);
                drawingContext.Pop();
                drawingContext.Pop();
            }

            private void AdornedElement_Loaded(object sender, RoutedEventArgs e)
            {
                if ((this._adornerLayer = AdornerLayer.GetAdornerLayer(this.AdornedElement)) != null)
                {
                    this._adornerLayer.Add(this);
                }
                this.AdornedElement.Loaded -= this.AdornedElement_Loaded;
                this.AdornedElement.Unloaded += this.AdornedElement_Unloaded;
            }

            private void AdornedElement_Unloaded(object sender, RoutedEventArgs e)
            {
                if (this._adornerLayer != null)
                {
                    this._adornerLayer.Remove(this);
                    this._adornerLayer = null;
                }
                this.AdornedElement.Loaded += this.AdornedElement_Loaded;
                this.AdornedElement.Unloaded -= this.AdornedElement_Unloaded;
            }
        }
    }
}