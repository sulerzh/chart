using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class SeriesTooltipPresenter : SeriesAttachedPresenter
    {
        public SeriesTooltipPresenter(SeriesPresenter seriesPresenter)
          : base(seriesPresenter)
        {
        }

        internal override void OnCreateView(DataPoint dataPoint)
        {
        }

        internal override void OnRemoveView(DataPoint dataPoint)
        {
            this.ClearToolTip(dataPoint);
        }

        internal override void OnUpdateView(DataPoint dataPoint)
        {
            if (this.SeriesPresenter.IsDataPointVisible(dataPoint) && dataPoint.ToolTipContent != null)
            {
                this.AddToolTip(dataPoint);
            }
            else
            {
                if (this.SeriesPresenter.IsDataPointVisible(dataPoint) && dataPoint.ToolTipContent != null)
                    return;
                this.ClearToolTip(dataPoint);
            }
        }

        internal override void OnSeriesRemoved()
        {
        }

        protected virtual IEnumerable<FrameworkElement> GetDataPointViews(DataPoint dataPoint)
        {
            if (dataPoint.View != null)
            {
                if (dataPoint.View.MainView != null)
                    yield return dataPoint.View.MainView;
                if (dataPoint.View.HighlightView != null)
                    yield return dataPoint.View.HighlightView;
                if (dataPoint.View.MarkerView != null)
                    yield return dataPoint.View.MarkerView;
            }
        }

        private void AddToolTip(DataPoint dataPoint)
        {
            foreach (DependencyObject dependencyObject in this.GetDataPointViews(dataPoint))
                SeriesTooltipPresenter.AddToolTip(dependencyObject);
        }

        private void ClearToolTip(DataPoint dataPoint)
        {
            if (dataPoint.View == null)
                return;
            if (dataPoint.View.MainView != null)
                SeriesTooltipPresenter.ClearToolTip((DependencyObject)dataPoint.View.MainView);
            if (dataPoint.View.MarkerView == null)
                return;
            SeriesTooltipPresenter.ClearToolTip((DependencyObject)dataPoint.View.MarkerView);
        }

        private static void AddToolTip(DependencyObject obj)
        {
            if (ToolTipService.GetToolTip(obj) is ToolTip)
                return;
            ToolTip toolTip = new ToolTip();
            toolTip.SetBinding(ContentControl.ContentProperty, (BindingBase)new Binding("ToolTipContent"));
            toolTip.SetBinding(FrameworkElement.StyleProperty, (BindingBase)new Binding("ToolTipStyle"));
            ToolTipService.SetToolTip(obj, (object)toolTip);
        }

        internal static void ClearToolTip(DependencyObject obj)
        {
            ToolTip toolTip = ToolTipService.GetToolTip(obj) as ToolTip;
            if (toolTip == null)
                return;
            ToolTipService.SetToolTip(obj, (object)null);
            toolTip.DataContext = (object)null;
            toolTip.Content = (object)null;
        }
    }
}
