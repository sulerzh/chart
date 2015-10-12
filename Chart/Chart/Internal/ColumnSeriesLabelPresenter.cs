using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class ColumnSeriesLabelPresenter : SeriesLabelPresenter
    {
        internal override bool IsDataPointVisibilityUsesXAxisOnly
        {
            get
            {
                return true;
            }
        }

        public ColumnSeriesLabelPresenter(SeriesPresenter seriesPresenter)
          : base(seriesPresenter)
        {
        }

        internal override void BindViewToDataPoint(DataPoint dataPoint, FrameworkElement view, string valueName)
        {
            base.BindViewToDataPoint(dataPoint, view, valueName);
            LabelControl labelControl = view as LabelControl;
            if (labelControl == null)
                return;
            ColumnDataPoint columnDataPoint = dataPoint as ColumnDataPoint;
            if (columnDataPoint == null || !(valueName == "LabelPosition") && valueName != null)
                return;
            ContentPositions alignment;
            switch (columnDataPoint.LabelPosition)
            {
                case ColumnLabelPosition.InsideCenter:
                    alignment = ContentPositions.InsideCenter;
                    break;
                case ColumnLabelPosition.InsideBase:
                    alignment = ContentPositions.InsideBase;
                    break;
                case ColumnLabelPosition.InsideEnd:
                    alignment = ContentPositions.InsideEnd;
                    break;
                case ColumnLabelPosition.OutsideBase:
                    alignment = ContentPositions.OutsideBase;
                    break;
                case ColumnLabelPosition.OutsideEnd:
                    alignment = ContentPositions.OutsideEnd;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
            AnchorPanel.SetContentPosition((UIElement)labelControl, alignment);
        }

        internal override void OnUpdateView(DataPoint dataPoint)
        {
            base.OnUpdateView(dataPoint);
            if (dataPoint.View == null || dataPoint.View.LabelView == null)
                return;
            AnchorPanel.SetValidContentPositions((UIElement)dataPoint.View.LabelView, ContentPositions.InsideCenter | ContentPositions.InsideBase | ContentPositions.InsideEnd | ContentPositions.OutsideBase | ContentPositions.OutsideEnd);
            ColumnSeriesLabelPresenter.SetLabelMaxMovingDistance((XYChartArea)this.SeriesPresenter.Series.ChartArea, (UIElement)dataPoint.View.LabelView);
        }

        internal static void SetLabelMaxMovingDistance(XYChartArea chartArea, UIElement element)
        {
            double maximumMovingDistance = 0.0;
            if (chartArea.Orientation == Orientation.Horizontal)
                maximumMovingDistance = 40.0;
            AnchorPanel.SetMaximumMovingDistance(element, maximumMovingDistance);
        }
    }
}
