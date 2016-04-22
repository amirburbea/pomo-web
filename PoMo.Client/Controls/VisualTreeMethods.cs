using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace PoMo.Client.Controls
{
    internal static class VisualTreeMethods
    {
        public static T FindVisualTreeAncestor<T>(this DependencyObject d)
            where T : DependencyObject
        {
            return (T)d.FindVisualTreeAncestor(typeof(T));
        }

        public static DependencyObject FindVisualTreeAncestor(this DependencyObject child, Type ancestorType)
        {
            Run run = child as Run;
            if (run != null)
            {
                child = run.Parent;
            }
            while (true)
            {
                DependencyObject parent;
                if (child == null || (parent = VisualTreeHelper.GetParent(child)) == null)
                {
                    return null;
                }
                if (ancestorType.IsInstanceOfType(parent))
                {
                    return parent;
                }
                child = parent;
            }
        }

        public static IEnumerable<DependencyObject> GetVisualTreeChildren(this DependencyObject parent)
        {
            return parent == null ?
                Enumerable.Empty<DependencyObject>() :
                Enumerable.Range(0, VisualTreeHelper.GetChildrenCount(parent)).Select(index => VisualTreeHelper.GetChild(parent, index));
        }

        public static IEnumerable<TDescendent> GetVisualTreeDescendents<TDescendent>(this DependencyObject dependencyObject)
                            where TDescendent : DependencyObject
        {
            return dependencyObject?.GetVisualTreeDescendents(typeof(TDescendent)).Cast<TDescendent>() ?? Enumerable.Empty<TDescendent>();
        }

        public static IEnumerable<DependencyObject> GetVisualTreeDescendents(this DependencyObject dependencyObject, Type descendentType)
        {
            if (dependencyObject == null || descendentType == null)
            {
                yield break;
            }
            foreach (DependencyObject child in dependencyObject.GetVisualTreeChildren())
            {
                if (descendentType.IsInstanceOfType(child))
                {
                    yield return child;
                }
                foreach (DependencyObject grandChild in child.GetVisualTreeDescendents(descendentType))
                {
                    yield return grandChild;
                }
            }
        }
    }
}