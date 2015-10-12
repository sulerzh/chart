using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class StackedColumnSeriesLabelPresenter : SeriesLabelPresenter
    {
        internal override bool IsDataPointVisibilityUsesXAxisOnly
        {
            get
            {
                return true;
            }
        }

        public StackedColumnSeriesLabelPresenter(SeriesPresenter seriesPresenter)
          : base(seriesPresenter)
        {
        }

        internal override void OnUpdateView(DataPoint dataPoint)
        {
            base.OnUpdateView(dataPoint);
            if (dataPoint.View == null || dataPoint.View.LabelView == null)
                return;
            StackedColumnSeriesPresenter columnSeriesPresenter = this.SeriesPresenter as StackedColumnSeriesPresenter;
            ContentPositions validContentPositions = ContentPositions.InsideCenter;
            if (columnSeriesPresenter.IsStackTopSeries() && !columnSeriesPresenter.IsHundredPercent())
                validContentPositions |= ContentPositions.OutsideEnd;
            AnchorPanel.SetValidContentPositions((UIElement)dataPoint.View.LabelView, validContentPositions);
            AnchorPanel.SetContentPosition((UIElement)dataPoint.View.LabelView, ContentPositions.InsideCenter);
            AnchorPanel.SetAnchorMargin((UIElement)dataPoint.View.LabelView, 0.0);
            ColumnSeriesLabelPresenter.SetLabelMaxMovingDistance((XYChartArea)this.SeriesPresenter.Series.ChartArea, (UIElement)dataPoint.View.LabelView);
        }

        internal override void AdjustDataPointLabelVisibilityRating(LabelVisibilityManager.DataPointRange range, Dictionary<XYDataPoint, double> dataPointRanks)
        {
            if (range.DataPoints.Count <= 0)
                return;
            XYDataPoint xyDataPoint = range.DataPoints[0] as XYDataPoint;
            foreach (XYDataPoint key in range.DataPoints)
            {
                if (ValueHelper.Compare(key.XValue as IComparable, xyDataPoint.XValue as IComparable) == 0)
                {
                    if (dataPointRanks.ContainsKey(key))
                        dataPointRanks[key] = 100.0;
                    else
                        dataPointRanks.Add(key, 100.0);
                }
            }
        }

        internal override void BindViewToDataPoint(DataPoint dataPoint, FrameworkElement view, string valueName)
        {
            base.BindViewToDataPoint(dataPoint, view, valueName);
            StackedColumnSeries stackedColumnSeries = dataPoint.Series as StackedColumnSeries;
            StackedColumnDataPoint stackedColumnDataPoint = dataPoint as StackedColumnDataPoint;
            LabelControl labelControl = view as LabelControl;
            if (labelControl == null || stackedColumnDataPoint == null || (stackedColumnSeries == null || !stackedColumnSeries.ActualIsHundredPercent) || !(valueName == "ActualLabelContent") && valueName != null)
                return;
            double yvaluePercent = stackedColumnDataPoint.YValuePercent;
            if (Math.Abs(yvaluePercent) < 0.005)
                labelControl.Content = (object)null;
            else
                labelControl.Content = (object)yvaluePercent.ToString("P0", (IFormatProvider)CultureInfo.CurrentCulture);
        }
    }
}
