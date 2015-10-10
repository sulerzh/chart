using Microsoft.Reporting.Common.Toolkit.Internal;
using Semantic.Reporting.Windows.Common.PivotViewer.Internal;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Common.Internal
{
    internal static class VisualTreeHelpers
    {
        static VisualTreeHelpers()
        {
            CompositionTarget.Rendering += new EventHandler(VisualTreeHelpers.CompositionTarget_Rendering);
        }

        internal static IEnumerable<UIElement> GetChildren(this UIElement element)
        {
            Panel panel = element as Panel;
            if (panel != null)
            {
                foreach (UIElement uiElement in panel.Children)
                    yield return uiElement;
            }
            else
            {
                Border border = element as Border;
                if (border != null)
                {
                    yield return border.Child;
                }
                else
                {
                    int count = VisualTreeHelper.GetChildrenCount((DependencyObject)element);
                    for (int i = 0; i < count; ++i)
                        yield return VisualTreeHelper.GetChild((DependencyObject)element, i) as UIElement;
                }
            }
        }

        internal static void InvalidateMeasureInSubtree(this UIElement element)
        {
            if (element == null || element.Visibility == Visibility.Collapsed)
                return;
            element.InvalidateMeasure();
            foreach (UIElement element1 in VisualTreeHelpers.GetChildren(element))
                VisualTreeHelpers.InvalidateMeasureInSubtree(element1);
        }

        internal static void ForEachChildAndNodeBreadth<TNodeType>(DependencyObject node, Func<TNodeType, bool> callback) where TNodeType : class
        {
            foreach (DependencyObject dependencyObject in VisualTreeExtensions.GetVisualDescendantsAndSelf(node))
            {
                TNodeType nodeType = dependencyObject as TNodeType;
                if ((object)nodeType != null && !callback(nodeType))
                    break;
            }
        }

        internal static void ForEachChildAndNodeDepth<TNodeType>(DependencyObject node, Func<TNodeType, bool> callback) where TNodeType : class
        {
            Stack<DependencyObject> stack = new Stack<DependencyObject>();
            stack.Push(node);
            while (stack.Count > 0)
            {
                node = stack.Pop();
                TNodeType nodeType = node as TNodeType;
                if ((object)nodeType != null && !callback(nodeType))
                    break;
                for (int childrenCount = VisualTreeHelper.GetChildrenCount(node); childrenCount > 0; --childrenCount)
                    stack.Push(VisualTreeHelper.GetChild(node, childrenCount - 1));
            }
        }

        internal static void ForEachParentAndNode<TNodeType>(DependencyObject node, Func<TNodeType, bool> callback) where TNodeType : class
        {
            foreach (DependencyObject dependencyObject in VisualTreeExtensions.GetVisualAncestorsAndSelf(node))
            {
                TNodeType nodeType = dependencyObject as TNodeType;
                if ((object)nodeType != null && !callback(nodeType))
                    break;
            }
        }

        internal static void ForEachParent<TNodeType>(DependencyObject node, Func<TNodeType, bool> callback) where TNodeType : class
        {
            VisualTreeHelpers.ForEachParentAndNode<TNodeType>(VisualTreeHelper.GetParent(node), callback);
        }

        internal static DependencyObject GetVisualTreeRoot(DependencyObject element)
        {
            DependencyObject reference = element;
            for (DependencyObject dependencyObject = reference; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(reference))
                reference = dependencyObject;
            return reference;
        }

        internal static IUnparentedPopupProvider GetUnparentedPopupProvider(DependencyObject element)
        {
            IUnparentedPopupProvider unparentedPopupProvider = (IUnparentedPopupProvider)null;
            foreach (DependencyObject dependencyObject in VisualTreeExtensions.GetVisualAncestorsAndSelf(element))
            {
                unparentedPopupProvider = dependencyObject as IUnparentedPopupProvider;
                if (unparentedPopupProvider != null)
                    break;
            }
            return unparentedPopupProvider;
        }

        internal static MatrixTransform GetAnimationTransform(FrameworkElement element)
        {
            Matrix matrix1 = new Matrix();
            Transform thisObject1 = element.TransformToVisual((Visual)Window.GetWindow((DependencyObject)element)) as Transform;
            if (thisObject1 != null)
                matrix1 = MatrixHelper.Multiply(matrix1, TransformExtensions.GetMatrix(thisObject1));
            Transform thisObject2 = FrameworkElementExtensions.RenderTransformToAncestor(element, (DependencyObject)null).Inverse as Transform;
            if (thisObject2 != null)
                matrix1 = MatrixHelper.Multiply(matrix1, TransformExtensions.GetMatrix(thisObject2));
            return new MatrixTransform()
            {
                Matrix = matrix1
            };
        }

        private static void CompositionTarget_Rendering(object sender, EventArgs e)
        {
        }
    }
}
