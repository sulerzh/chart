using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Semantic.Reporting.Common.Toolkit.Internal
{
    public static class VisualTreeExtensions
    {
        public static IEnumerable<DependencyObject> GetVisualAncestors(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return Enumerable.Skip<DependencyObject>(VisualTreeExtensions.GetVisualAncestorsAndSelfIterator(element), 1);
        }

        public static IEnumerable<DependencyObject> GetVisualAncestorsAndSelf(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return VisualTreeExtensions.GetVisualAncestorsAndSelfIterator(element);
        }

        private static IEnumerable<DependencyObject> GetVisualAncestorsAndSelfIterator(DependencyObject element)
        {
            for (DependencyObject obj = element; obj != null; obj = VisualTreeHelper.GetParent(obj))
                yield return obj;
        }

        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; ++i)
                yield return VisualTreeHelper.GetChild(element, i);
        }

        public static IEnumerable<DependencyObject> GetVisualChildrenAndSelf(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return VisualTreeExtensions.GetVisualChildrenAndSelfIterator(element);
        }

        private static IEnumerable<DependencyObject> GetVisualChildrenAndSelfIterator(this DependencyObject element)
        {
            yield return element;
            int count = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < count; ++i)
                yield return VisualTreeHelper.GetChild(element, i);
        }

        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return Enumerable.Skip<DependencyObject>(VisualTreeExtensions.GetVisualDescendantsAndSelfIterator(element), 1);
        }

        public static IEnumerable<DependencyObject> GetVisualDescendantsAndSelf(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return VisualTreeExtensions.GetVisualDescendantsAndSelfIterator(element);
        }

        private static IEnumerable<DependencyObject> GetVisualDescendantsAndSelfIterator(DependencyObject element)
        {
            Queue<DependencyObject> remaining = new Queue<DependencyObject>();
            remaining.Enqueue(element);
            while (remaining.Count > 0)
            {
                DependencyObject obj = remaining.Dequeue();
                yield return obj;
                foreach (DependencyObject dependencyObject in VisualTreeExtensions.GetVisualChildren(obj))
                    remaining.Enqueue(dependencyObject);
            }
        }

        public static IEnumerable<DependencyObject> GetVisualSiblings(this DependencyObject element)
        {
            return Enumerable.Where<DependencyObject>(VisualTreeExtensions.GetVisualSiblingsAndSelf(element), (Func<DependencyObject, bool>)(p => p != element));
        }

        public static IEnumerable<DependencyObject> GetVisualSiblingsAndSelf(this DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            DependencyObject parent = VisualTreeHelper.GetParent(element);
            if (parent != null)
                return VisualTreeExtensions.GetVisualChildren(parent);
            return Enumerable.Empty<DependencyObject>();
        }

        public static Rect? GetBoundsRelativeTo(this FrameworkElement element, UIElement otherElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (otherElement == null)
                throw new ArgumentNullException("otherElement");
            try
            {
                GeneralTransform generalTransform = element.TransformToVisual((Visual)otherElement);
                if (generalTransform != null)
                {
                    Point result1;
                    if (generalTransform.TryTransform(new Point(), out result1))
                    {
                        Point result2;
                        if (generalTransform.TryTransform(new Point(element.ActualWidth, element.ActualHeight), out result2))
                            return new Rect?(new Rect(result1, result2));
                    }
                }
            }
            catch (ArgumentException ex)
            {
            }
            return new Rect?();
        }

        public static void InvokeOnLayoutUpdated(this FrameworkElement element, Action action)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (action == null)
                throw new ArgumentNullException("action");
            EventHandler handler = (EventHandler)null;
            handler = (EventHandler)((s, e) =>
           {
               element.LayoutUpdated -= handler;
               action();
           });
            element.LayoutUpdated += handler;
        }

        internal static IEnumerable<FrameworkElement> GetLogicalChildren(this FrameworkElement parent)
        {
            Popup popup = parent as Popup;
            if (popup != null)
            {
                FrameworkElement popupChild = popup.Child as FrameworkElement;
                if (popupChild != null)
                    yield return popupChild;
            }
            ItemsControl itemsControl = parent as ItemsControl;
            if (itemsControl != null)
            {
                foreach (FrameworkElement frameworkElement in Enumerable.OfType<FrameworkElement>((IEnumerable)Enumerable.Select<int, DependencyObject>(Enumerable.Range(0, itemsControl.Items.Count), (Func<int, DependencyObject>)(index => itemsControl.ItemContainerGenerator.ContainerFromIndex(index)))))
                    yield return frameworkElement;
            }
            Queue<FrameworkElement> queue = new Queue<FrameworkElement>(Enumerable.OfType<FrameworkElement>((IEnumerable)VisualTreeExtensions.GetVisualChildren((DependencyObject)parent)));
            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                if (element.Parent == parent || element is UserControl)
                {
                    yield return element;
                }
                else
                {
                    foreach (FrameworkElement frameworkElement in Enumerable.OfType<FrameworkElement>((IEnumerable)VisualTreeExtensions.GetVisualChildren((DependencyObject)element)))
                        queue.Enqueue(frameworkElement);
                }
            }
        }

        internal static IEnumerable<FrameworkElement> GetLogicalDescendents(this FrameworkElement parent)
        {
            return FunctionalProgramming.TraverseBreadthFirst<FrameworkElement>(parent, (Func<FrameworkElement, IEnumerable<FrameworkElement>>)(node => VisualTreeExtensions.GetLogicalChildren(node)), (Func<FrameworkElement, bool>)(node => true));
        }
    }
}
