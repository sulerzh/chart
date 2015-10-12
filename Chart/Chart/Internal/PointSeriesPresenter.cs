using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Media.Effects;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class PointSeriesPresenter : XYSeriesPresenter
    {
        public PointSeriesPresenter(XYSeries series)
          : base(series)
        {
        }

        internal override SeriesMarkerPresenter CreateMarkerPresenter()
        {
            return (SeriesMarkerPresenter)new PointSeriesMarkerPresenter((SeriesPresenter)this);
        }

        internal override SeriesLabelPresenter CreateLabelPresenter()
        {
            return (SeriesLabelPresenter)new PointSeriesLabelPresenter((SeriesPresenter)this);
        }

        protected override FrameworkElement CreateViewElement(DataPoint dataPoint)
        {
            return (FrameworkElement)null;
        }

        protected override void UpdateView(DataPoint dataPoint)
        {
            if (this.IsDataPointViewVisible(dataPoint))
            {
                XYDataPoint xyDataPoint = dataPoint as XYDataPoint;
                if (xyDataPoint != null && dataPoint.View != null)
                {
                    Point point = this.ChartArea.ConvertScaleToPlotCoordinate(this.Series.XAxis, this.Series.YAxis, xyDataPoint.XValueInScaleUnits, xyDataPoint.YValueInScaleUnits);
                    point.X = Math.Round(point.X);
                    point.Y = Math.Round(point.Y);
                    dataPoint.View.AnchorPoint = point;
                }
            }
            base.UpdateView(dataPoint);
        }

        protected override void BindViewToDataPoint(DataPoint dataPoint, FrameworkElement view, string valueName)
        {
            DataPointView dataPointView = dataPoint != null ? dataPoint.View : (DataPointView)null;
            if (dataPointView == null)
                return;
            this.LabelPresenter.BindViewToDataPoint(dataPoint, (FrameworkElement)dataPointView.LabelView, valueName);
            this.MarkerPresenter.BindViewToDataPoint(dataPoint, dataPointView.MarkerView, valueName);
        }

        internal override FrameworkElement GetLegendSymbol()
        {
            DataPoint dataPoint1 = (DataPoint)Enumerable.FirstOrDefault<XYDataPoint>(Enumerable.Where<XYDataPoint>(Enumerable.OfType<XYDataPoint>((IEnumerable)this.Series.DataPoints), (Func<XYDataPoint, bool>)(p =>
         {
             if (!p.ActualIsEmpty)
                 return p.IsVisible;
             return false;
         })));
            FrameworkElement viewElement = this.MarkerPresenter.CreateViewElement();
            if (dataPoint1 != null)
            {
                this.MarkerPresenter.BindViewToDataPoint(dataPoint1, viewElement, (string)null);
                viewElement.Opacity = dataPoint1.Opacity;
                foreach (UIElement uiElement in Enumerable.Where<XYDataPoint>(Enumerable.OfType<XYDataPoint>((IEnumerable)this.Series.DataPoints), (Func<XYDataPoint, bool>)(p => !p.ActualIsEmpty)))
                {
                    if (uiElement.Effect != viewElement.Effect)
                    {
                        viewElement.ClearValue(UIElement.EffectProperty);
                        break;
                    }
                }
            }
            else
            {
                DataPoint dataPoint2 = (DataPoint)(this.Series.CreateDataPoint() as XYDataPoint);
                dataPoint2.Series = (Series)this.Series;
                if (dataPoint2 is BubbleDataPoint)
                    ((BubbleDataPoint)dataPoint2).SizeValueInScaleUnits = dataPoint2.MarkerSize;
                if (this.Series != null && this.Series.ItemsBinder != null && this.Series.DataContext != null)
                    this.Series.ItemsBinder.Bind(dataPoint2, this.Series.DataContext);
                this.MarkerPresenter.BindViewToDataPoint(dataPoint2, viewElement, (string)null);
                dataPoint2.Series = (Series)null;
            }
            viewElement.Effect = (Effect)null;
            double num = Math.Min(20.0, 12.0);
            viewElement.Width = num;
            viewElement.Height = num;
            return viewElement;
        }
    }
}
