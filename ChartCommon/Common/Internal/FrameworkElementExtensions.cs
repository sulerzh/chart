using Microsoft.Reporting.Common.Toolkit.Internal;
using Semantic.Reporting.Windows.Common.PivotViewer.Internal;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public static class FrameworkElementExtensions
    {
        public static readonly DependencyProperty ClipToBoundsProperty = AttachedProperty.RegisterAttached("ClipToBounds", typeof(bool), typeof(FrameworkElementExtensions), new PropertyMetadata((PropertyChangedCallback)((d, e) =>
     {
         SizeChangedEventHandler changedEventHandler = (SizeChangedEventHandler)((_s, _e) =>
       {
           FrameworkElement frameworkElement = (FrameworkElement)_s;
           ((RectangleGeometry)frameworkElement.Clip).Rect = new Rect(new Point(), frameworkElement.RenderSize);
       });
         FrameworkElement frameworkElement1 = (FrameworkElement)d;
         if ((bool)e.OldValue)
         {
             frameworkElement1.Clip = (Geometry)null;
             frameworkElement1.SizeChanged -= changedEventHandler;
         }
         if (!(bool)e.NewValue)
             return;
         RectangleGeometry rectangleGeometry = new RectangleGeometry();
         rectangleGeometry.Rect = new Rect(new Point(), frameworkElement1.RenderSize);
         frameworkElement1.SizeChanged += changedEventHandler;
         frameworkElement1.Clip = (Geometry)rectangleGeometry;
     })));

        public static void SetClipToBounds(DependencyObject d, bool value)
        {
            if (d == null)
                return;
            d.SetValue(FrameworkElementExtensions.ClipToBoundsProperty, value);
        }

        public static bool GetClipToBounds(DependencyObject d)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            return (bool)d.GetValue(FrameworkElementExtensions.ClipToBoundsProperty);
        }

        public static void InvalidateSubTree(this FrameworkElement thisObject)
        {
            if (thisObject == null)
                throw new ArgumentNullException("thisObject");
            thisObject.InvalidateArrange();
            thisObject.InvalidateMeasure();
            int childrenCount = VisualTreeHelper.GetChildrenCount((DependencyObject)thisObject);
            for (int childIndex = 0; childIndex < childrenCount; ++childIndex)
                FrameworkElementExtensions.InvalidateSubTree((FrameworkElement)VisualTreeHelper.GetChild((DependencyObject)thisObject, childIndex));
        }

        public static bool IsDescendantOf(this FrameworkElement thisObject, DependencyObject parent)
        {
            return (parent as FrameworkElement).IsAncestorOf((DependencyObject)thisObject);
        }

        public static TType GetAncestorOfType<TType>(this FrameworkElement thisObject)
        {
            TType type = default(TType);
            foreach (object obj in VisualTreeExtensions.GetVisualAncestors((DependencyObject)thisObject))
            {
                if (obj is TType || typeof(TType).IsAssignableFrom(obj.GetType()))
                {
                    type = (TType)obj;
                    break;
                }
            }
            return type;
        }

        public static bool IsMeasureArrangeValid(this FrameworkElement thisObject)
        {
            if (thisObject == null)
                throw new ArgumentNullException("thisObject");
            if (thisObject.ActualHeight > 0.0 && thisObject.ActualWidth > 0.0)
                return FrameworkElementExtensions.IsInVisualTree(thisObject);
            return false;
        }

        public static bool IsInVisualTree(this FrameworkElement thisObject)
        {
            bool flag = false;
            DependencyObject visualTreeRoot = VisualTreeHelpers.GetVisualTreeRoot((DependencyObject)thisObject);
            if (visualTreeRoot == Window.GetWindow((DependencyObject)thisObject))
            {
                flag = true;
            }
            else
            {
                FrameworkElement frameworkElement = visualTreeRoot as FrameworkElement;
                if (frameworkElement != null)
                {
                    Popup popup = frameworkElement.Parent as Popup;
                    if (popup != null && popup.IsOpen)
                        flag = true;
                }
            }
            return flag;
        }

        public static Transform RenderTransformToAncestor(this FrameworkElement element, DependencyObject parent)
        {
            return FrameworkElementExtensions.TransformToAncestorHelper(element, parent, UIElement.RenderTransformProperty);
        }

        public static Transform TrackableRenderTransformToAncestor(this FrameworkElement element, DependencyObject parent)
        {
            return FrameworkElementExtensions.TransformToAncestorHelper(element, parent, TrackableRenderTransform.TransformProperty);
        }

        public static void SetRenderTransform(this FrameworkElement thisObject, Transform transform)
        {
            TrackableRenderTransform.SetTransform((DependencyObject)thisObject, (GeneralTransform)transform);
        }

        public static object TryFindResource(this FrameworkElement thisObject, object resourceKey)
        {
            object requestedResource = (object)null;
            VisualTreeHelpers.ForEachParentAndNode<FrameworkElement>((DependencyObject)thisObject, (Func<FrameworkElement, bool>)(fe =>
          {
              bool flag = true;
              object obj = fe.Resources[resourceKey];
              if (obj != null)
              {
                  requestedResource = obj;
                  flag = false;
              }
              return flag;
          }));
            if (requestedResource == null)
                requestedResource = Application.Current.Resources[resourceKey];
            return requestedResource;
        }

        private static Transform TransformToAncestorHelper(FrameworkElement element, DependencyObject parent, DependencyProperty property)
        {
            Matrix matrix = Matrix.Identity;
            foreach (DependencyObject dependencyObject in VisualTreeExtensions.GetVisualAncestorsAndSelf((DependencyObject)element))
            {
                if (dependencyObject != parent)
                {
                    Transform thisObject = (Transform)dependencyObject.GetValue(property);
                    if (thisObject != null)
                        matrix = MatrixHelper.Multiply(matrix, TransformExtensions.GetMatrix(thisObject));
                }
                else
                    break;
            }
            return (Transform)TransformExtensions.MakeMatrixTransform(matrix);
        }

        public static void FocusDescendant(this FrameworkElement element)
        {
            VisualTreeHelpers.ForEachChildAndNodeDepth<DependencyObject>((DependencyObject)element, (Func<DependencyObject, bool>)(current =>
          {
              bool flag = true;
              Control control = current as Control;
              if (control != null)
                  flag = !control.Focus();
              return flag;
          }));
        }
    }
}
