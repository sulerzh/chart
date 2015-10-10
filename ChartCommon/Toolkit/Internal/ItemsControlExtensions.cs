using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Reporting.Common.Toolkit.Internal
{
    public static class ItemsControlExtensions
    {
        public static Panel GetItemsHost(this ItemsControl control)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            DependencyObject reference = control.ItemContainerGenerator.ContainerFromIndex(0);
            if (reference != null)
                return VisualTreeHelper.GetParent(reference) as Panel;
            FrameworkElement parent = Enumerable.FirstOrDefault<DependencyObject>(VisualTreeExtensions.GetVisualChildren((DependencyObject)control)) as FrameworkElement;
            if (parent != null)
            {
                ItemsPresenter itemsPresenter = Enumerable.FirstOrDefault<ItemsPresenter>(Enumerable.OfType<ItemsPresenter>((IEnumerable)VisualTreeExtensions.GetLogicalDescendents(parent)));
                if (itemsPresenter != null && VisualTreeHelper.GetChildrenCount((DependencyObject)itemsPresenter) > 0)
                    return VisualTreeHelper.GetChild((DependencyObject)itemsPresenter, 0) as Panel;
            }
            return (Panel)null;
        }

        public static ScrollViewer GetScrollHost(this ItemsControl control)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            Panel itemsHost = ItemsControlExtensions.GetItemsHost(control);
            if (itemsHost == null)
                return (ScrollViewer)null;
            return Enumerable.FirstOrDefault<ScrollViewer>(Enumerable.OfType<ScrollViewer>((IEnumerable)Enumerable.Where<DependencyObject>(VisualTreeExtensions.GetVisualAncestors((DependencyObject)itemsHost), (Func<DependencyObject, bool>)(c => c != control))));
        }

        public static IEnumerable<DependencyObject> GetContainers(this ItemsControl control)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            return ItemsControlExtensions.GetContainersIterator<DependencyObject>(control);
        }

        public static IEnumerable<TContainer> GetContainers<TContainer>(this ItemsControl control) where TContainer : DependencyObject
        {
            if (control == null)
                throw new ArgumentNullException("control");
            return ItemsControlExtensions.GetContainersIterator<TContainer>(control);
        }

        private static IEnumerable<TContainer> GetContainersIterator<TContainer>(ItemsControl control) where TContainer : DependencyObject
        {
            return Enumerable.Select<KeyValuePair<object, TContainer>, TContainer>(ItemsControlExtensions.GetItemsAndContainers<TContainer>(control), (Func<KeyValuePair<object, TContainer>, TContainer>)(p => p.Value));
        }

        public static IEnumerable<KeyValuePair<object, DependencyObject>> GetItemsAndContainers(this ItemsControl control)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            return ItemsControlExtensions.GetItemsAndContainersIterator<DependencyObject>(control);
        }

        public static IEnumerable<KeyValuePair<object, TContainer>> GetItemsAndContainers<TContainer>(this ItemsControl control) where TContainer : DependencyObject
        {
            if (control == null)
                throw new ArgumentNullException("control");
            return ItemsControlExtensions.GetItemsAndContainersIterator<TContainer>(control);
        }

        private static IEnumerable<KeyValuePair<object, TContainer>> GetItemsAndContainersIterator<TContainer>(ItemsControl control) where TContainer : DependencyObject
        {
            int count = control.Items.Count;
            for (int i = 0; i < count; ++i)
            {
                DependencyObject container = control.ItemContainerGenerator.ContainerFromIndex(i);
                if (container != null)
                    yield return new KeyValuePair<object, TContainer>(control.Items[i], container as TContainer);
            }
        }

        internal static bool CanAddItem(this ItemsControl that, object item)
        {
            if (that.ItemsSource == null)
                return true;
            return CollectionHelper.CanInsert(that.ItemsSource, item);
        }

        internal static bool CanRemoveItem(this ItemsControl that)
        {
            if (that.ItemsSource == null)
                return true;
            if (!CollectionHelper.IsReadOnly(that.ItemsSource))
                return that.ItemsSource is INotifyCollectionChanged;
            return false;
        }

        internal static void InsertItem(this ItemsControl that, int index, object item)
        {
            if (that.ItemsSource == null)
                that.Items.Insert(index, item);
            else
                CollectionHelper.Insert(that.ItemsSource, index, item);
        }

        internal static void AddItem(this ItemsControl that, object item)
        {
            if (that.ItemsSource == null)
                ItemsControlExtensions.InsertItem(that, that.Items.Count, item);
            else
                CollectionHelper.Add(that.ItemsSource, item);
        }

        internal static void RemoveItem(this ItemsControl that, object item)
        {
            if (that.ItemsSource == null)
                that.Items.Remove(item);
            else
                CollectionHelper.Remove(that.ItemsSource, item);
        }

        internal static void RemoveItemAtIndex(this ItemsControl that, int index)
        {
            if (that.ItemsSource == null)
                that.Items.RemoveAt(index);
            else
                CollectionHelper.RemoveAt(that.ItemsSource, index);
        }

        internal static int GetItemCount(this ItemsControl that)
        {
            if (that.ItemsSource == null)
                return that.Items.Count;
            return CollectionHelper.Count(that.ItemsSource);
        }
    }
}
