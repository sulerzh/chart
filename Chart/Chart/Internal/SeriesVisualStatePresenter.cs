﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class SeriesVisualStatePresenter : SeriesAttachedPresenter
    {
        internal static readonly DependencyProperty IsViewProcessedProperty = DependencyProperty.RegisterAttached("IsViewProcessed", typeof(bool), typeof(SeriesVisualStatePresenter), new PropertyMetadata((object)false));
        internal const string IsViewProcessedPropertyName = "IsViewProcessed";

        public SeriesVisualStatePresenter(SeriesPresenter seriesPresenter)
          : base(seriesPresenter)
        {
        }

        internal static bool GetIsViewProcessed(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (bool)element.GetValue(SeriesVisualStatePresenter.IsViewProcessedProperty);
        }

        internal static void SetIsViewProcessed(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(SeriesVisualStatePresenter.IsViewProcessedProperty, value);
        }

        internal static FrameworkElement GetDataPointView(DataPoint dataPoint)
        {
            FrameworkElement frameworkElement = (FrameworkElement)null;
            if (dataPoint.View != null)
            {
                if (dataPoint.View.MainView != null)
                    frameworkElement = dataPoint.View.MainView;
                else if (dataPoint.View.MarkerView != null)
                    frameworkElement = dataPoint.View.MarkerView;
            }
            return frameworkElement;
        }

        internal static DataPoint GetDataPointFromSelectedElement(DependencyObject element, DependencyObject rootElement)
        {
            DataPoint dataPoint = (DataPoint)null;
            DependencyObject dependencyObject = element;
            if (dependencyObject != null)
            {
                do
                {
                    dataPoint = DataPoint.GetDataPoint(dependencyObject);
                    dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
                }
                while (dataPoint == null && dependencyObject != null && dependencyObject != rootElement);
            }
            return dataPoint;
        }

        internal override void OnCreateView(DataPoint dataPoint)
        {
        }

        internal override void OnUpdateView(DataPoint dataPoint)
        {
            FrameworkElement element = SeriesVisualStatePresenter.GetDataPointView(dataPoint);
            if (element != null && DataPoint.GetDataPoint((DependencyObject)element) != dataPoint)
            {
                DataPoint.SetDataPoint((DependencyObject)element, dataPoint);
                if (element is Control)
                {
                    element.MouseEnter += (MouseEventHandler)((s, e) => VisualStateManager.GoToState((FrameworkElement)(element as Control), "MouseOver", true));
                    element.MouseLeave += (MouseEventHandler)((s, e) => VisualStateManager.GoToState((FrameworkElement)(element as Control), "Normal", true));
                }
            }
            FrameworkElement frameworkElement = dataPoint.View == null || dataPoint.View.LabelView == null ? (FrameworkElement)null : (FrameworkElement)dataPoint.View.LabelView;
            if (frameworkElement == null || DataPoint.GetDataPoint((DependencyObject)frameworkElement) == dataPoint)
                return;
            DataPoint.SetDataPoint((DependencyObject)frameworkElement, dataPoint);
        }

        internal override void OnRemoveView(DataPoint dataPoint)
        {
            FrameworkElement element = SeriesVisualStatePresenter.GetDataPointView(dataPoint);
            if (!(element is Control) || DataPoint.GetDataPoint((DependencyObject)element) != dataPoint)
                return;
            element.MouseEnter -= (MouseEventHandler)((s, e) => VisualStateManager.GoToState((FrameworkElement)(element as Control), "MouseOver", true));
            element.MouseLeave -= (MouseEventHandler)((s, e) => VisualStateManager.GoToState((FrameworkElement)(element as Control), "Normal", true));
        }

        internal override void OnSeriesRemoved()
        {
        }
    }
}
