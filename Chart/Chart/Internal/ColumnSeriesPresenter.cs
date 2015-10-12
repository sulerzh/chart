using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class ColumnSeriesPresenter : XYSeriesPresenter
    {
        private const double MaxScreeenCoordinate = 1E+20;
        private PanelElementPool<FrameworkElement, DataPoint> _dataPointElementPool;

        private PanelElementPool<FrameworkElement, DataPoint> DataPointElementPool
        {
            get
            {
                if (this._dataPointElementPool == null)
                {
                    this._dataPointElementPool = new PanelElementPool<FrameworkElement, DataPoint>(this.RootPanel, new Func<FrameworkElement>(this.CreateViewElement), new Action<FrameworkElement, DataPoint>(this.UpdateViewElement), new Action<FrameworkElement>(this.ResetViewElement));
                    this._dataPointElementPool.MaxElementCount = 100;
                }
                return this._dataPointElementPool;
            }
        }

        public double PointWidth { get; set; }

        public double PointClusterOffset { get; set; }

        public ColumnSeriesPresenter(XYSeries series)
          : base(series)
        {
            this.IsRootPanelClipped = true;
            this.DefaultSimplifiedRenderingThreshold = 200;
        }

        private FrameworkElement CreateViewElement()
        {
            if (this.IsSimplifiedRenderingModeEnabled)
                return (FrameworkElement)new Rectangle();
            return (FrameworkElement)new BarControl();
        }

        private void UpdateViewElement(FrameworkElement element, DataPoint dataPoint)
        {
            element.DataContext = (object)dataPoint;
            this.BindViewToDataPoint(dataPoint, element, (string)null);
        }

        private void ResetViewElement(FrameworkElement element)
        {
            SeriesTooltipPresenter.ClearToolTip((DependencyObject)element);
            element.DataContext = (object)null;
        }

        internal override SeriesMarkerPresenter CreateMarkerPresenter()
        {
            return new SeriesMarkerPresenter((SeriesPresenter)this);
        }

        internal override SeriesLabelPresenter CreateLabelPresenter()
        {
            return (SeriesLabelPresenter)new ColumnSeriesLabelPresenter((SeriesPresenter)this);
        }

        public override void InvalidateSeries()
        {
            if (this.ChartArea != null)
                this.ChartArea.UpdateSession.ExecuteOnceBeforeUpdating((Action)(() => this.CalculateRelatedSeriesPointWidth()), (object)new Tuple<string, Axis>("__CalculatePointWidth__", this.Series.XAxis));
            base.InvalidateSeries();
        }

        protected override void UpdateView()
        {
            if (this.ChartArea != null)
                this.ChartArea.UpdateSession.ExecuteOnceDuringUpdating((Action)(() => this.CalculateRelatedSeriesPointWidth()), (object)new Tuple<string, Axis>("__CalculatePointWidth__", this.Series.XAxis));
            base.UpdateView();
        }

        protected override void UpdateRelatedSeriesPresenters()
        {
            this.ChartArea.UpdateSession.BeginUpdates();
            if (this.XYChartArea != null)
                EnumerableFunctions.ForEachWithIndex<XYSeries>(Enumerable.Where<XYSeries>((IEnumerable<XYSeries>)this.XYChartArea.Series, (Func<XYSeries, bool>)(item =>
              {
                  if (item.GetType() == this.Series.GetType())
                      return item != this.Series;
                  return false;
              })), (Action<XYSeries, int>)((item, index) => this.ChartArea.UpdateSession.Update((IUpdatable)item)));
            base.UpdateRelatedSeriesPresenters();
            this.ChartArea.UpdateSession.EndUpdates();
        }

        protected override FrameworkElement CreateViewElement(DataPoint dataPoint)
        {
            if (this.DataPointElementPool.InUseElementCount == 0)
            {
                this.IsSimplifiedRenderingModeCheckRequired = true;
                this.CheckSimplifiedRenderingMode();
            }
            return this.DataPointElementPool.Get(dataPoint);
        }

        protected override void RemoveView(DataPoint dataPoint)
        {
            if (dataPoint.View != null && dataPoint.View.MainView != null)
            {
                this.DataPointElementPool.Release(dataPoint.View.MainView);
                dataPoint.View.MainView = (FrameworkElement)null;
                this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.DataPointElementPool.AdjustPoolSize()), (object)new Tuple<Series, string>((Series)this.Series, "__AdjustDataPointElementPoolSize__"), (string)null);
            }
            base.RemoveView(dataPoint);
        }

        protected override void BindViewToDataPoint(DataPoint dataPoint, FrameworkElement view, string valueName)
        {
            IAppearanceProvider appearanceProvider = (IAppearanceProvider)dataPoint;
            if (appearanceProvider != null)
            {
                BarControl barControl = view as BarControl;
                if (barControl != null)
                {
                    if (valueName == "Fill" || valueName == null)
                        barControl.Background = appearanceProvider.Fill;
                    if (valueName == "Stroke" || valueName == null)
                        barControl.BorderBrush = appearanceProvider.Stroke;
                    if (valueName == "StrokeThickness" || valueName == null)
                        barControl.BorderThickness = new Thickness(appearanceProvider.StrokeThickness);
                    if (valueName == "Opacity" || valueName == "ActualOpacity" || valueName == null)
                        barControl.Opacity = dataPoint.ActualOpacity;
                    if (valueName == "Effect" || valueName == "ActualEffect" || valueName == null)
                        barControl.Effect = dataPoint.ActualEffect;
                }
                else
                {
                    Shape shape = view as Shape;
                    if (shape != null)
                    {
                        if (valueName == "Fill" || valueName == null)
                            shape.Fill = dataPoint.Fill;
                        if (valueName == "Opacity" || valueName == "ActualOpacity" || valueName == null)
                            shape.Opacity = dataPoint.ActualOpacity;
                        if (valueName == "Effect" || valueName == "ActualEffect" || valueName == null)
                            shape.Effect = dataPoint.ActualEffect is ShaderEffect ? dataPoint.ActualEffect : (Effect)null;
                    }
                }
            }
            DataPointView dataPointView = dataPoint != null ? dataPoint.View : (DataPointView)null;
            if (dataPointView == null)
                return;
            this.LabelPresenter.BindViewToDataPoint(dataPoint, (FrameworkElement)dataPointView.LabelView, valueName);
            this.MarkerPresenter.BindViewToDataPoint(dataPoint, dataPointView.MarkerView, valueName);
        }

        internal virtual double GetYOffsetInAxisUnits(XYDataPoint dataPoint, Point valuePoint, Point basePoint)
        {
            return 0.0;
        }

        internal virtual Point GetPositionInAxisUnits(XYDataPoint dataPointXY)
        {
            return new Point(this.Series.XAxis.AxisPresenter.ConvertScaleToAxisUnits(dataPointXY.XValueInScaleUnits) ?? 0.0, this.Series.YAxis.AxisPresenter.ConvertScaleToAxisUnits(dataPointXY.YValueInScaleUnits) ?? 0.0);
        }

        internal virtual bool CanAdjustHeight()
        {
            return true;
        }

        protected override void UpdateView(DataPoint dataPoint)
        {
            if (!this.IsDataPointViewVisible(dataPoint))
                return;
            DateTime now = DateTime.Now;
            XYDataPoint xyDataPoint = dataPoint as XYDataPoint;
            if (xyDataPoint != null && this.CanGraph(xyDataPoint))
            {
                DataPointView view = dataPoint.View;
                if (view != null)
                {
                    FrameworkElement mainView = view.MainView;
                    if (mainView != null)
                    {
                        bool flag = this.ChartArea.Orientation != Orientation.Horizontal;
                        RectOrientation rectOrientation = RectOrientation.BottomTop;
                        Point positionInAxisUnits = this.GetPositionInAxisUnits(xyDataPoint);
                        Point point1 = new Point(Math.Round(positionInAxisUnits.X), Math.Round(positionInAxisUnits.Y));
                        object crossingPosition = this.Series.YAxis.Scale.ActualCrossingPosition;
                        Point basePoint = new Point(positionInAxisUnits.X, this.Series.YAxis.AxisPresenter.ConvertDataToAxisUnits(crossingPosition) ?? 0.0);
                        Point point2 = new Point(Math.Round(basePoint.X), Math.Round(basePoint.Y));
                        double num1 = point1.X + Math.Round(this.PointClusterOffset);
                        double num2 = this.MinMaxScreenCoordinates(positionInAxisUnits.Y);
                        double num3 = Math.Round(this.PointWidth);
                        double height = this.MinMaxScreenCoordinates(basePoint.Y - positionInAxisUnits.Y);
                        if (ValueHelper.Compare(xyDataPoint.YValue as IComparable, crossingPosition as IComparable) != 0 && Math.Abs(height) < 2.0 && this.CanAdjustHeight())
                        {
                            height = basePoint.Y - positionInAxisUnits.Y >= 0.0 ? 2.0 : -2.0;
                            num2 = point2.Y - height;
                        }
                        if (height < 0.0)
                        {
                            rectOrientation = RectOrientation.TopBottom;
                            height = Math.Abs(height);
                            num2 -= height;
                        }
                        double num4 = this.MinMaxScreenCoordinates(this.GetYOffsetInAxisUnits(xyDataPoint, positionInAxisUnits, basePoint));
                        double num5 = Math.Round(num2 - num4);
                        double num6 = this.AdjustColumnHeight(height);
                        if (flag)
                        {
                            if (rectOrientation == RectOrientation.BottomTop)
                                rectOrientation = RectOrientation.RightLeft;
                            else if (rectOrientation == RectOrientation.TopBottom)
                                rectOrientation = RectOrientation.LeftRight;
                            Canvas.SetLeft((UIElement)mainView, num5);
                            Canvas.SetTop((UIElement)mainView, num1);
                            mainView.Width = num6;
                            mainView.Height = num3;
                            view.AnchorRect = new Rect(num5, num1, num6, num3);
                            view.AnchorPoint = rectOrientation != RectOrientation.RightLeft ? new Point(num5 + num6, num1 + this.PointWidth / 2.0) : new Point(num5, num1 + this.PointWidth / 2.0);
                        }
                        else
                        {
                            Canvas.SetLeft((UIElement)mainView, num1);
                            Canvas.SetTop((UIElement)mainView, num5);
                            mainView.Width = num3;
                            mainView.Height = num6;
                            view.AnchorRect = new Rect(num1, num5, num3, num6);
                            view.AnchorPoint = rectOrientation != RectOrientation.BottomTop ? new Point(num1 + this.PointWidth / 2.0, num5 + num6) : new Point(num1 + this.PointWidth / 2.0, num5);
                        }
                        BarControl barControl = mainView as BarControl;
                        if (barControl != null)
                            barControl.Orientation = rectOrientation;
                        view.AnchorRectOrientation = rectOrientation;
                    }
                }
            }
            base.UpdateView(dataPoint);
            if (this.ChartArea == null)
                return;
            this.ChartArea.UpdateSession.AddCounter("ColumnSeriesPresenter.UpdateView", DateTime.Now - now);
        }

        private double MinMaxScreenCoordinates(double value)
        {
            return Math.Max(Math.Min(value, 1E+20), -1E+20);
        }

        protected virtual double AdjustColumnHeight(double height)
        {
            return Math.Ceiling(height);
        }

        internal override bool CanGraph(XYDataPoint dataPointXY)
        {
            if (ValueHelper.CanGraph(dataPointXY.XValueInScaleUnits) && ValueHelper.CanGraph(dataPointXY.YValueInScaleUnits) && DoubleHelper.GreaterOrEqualWithPrecision(dataPointXY.XValueInScaleUnits, 0.0))
                return DoubleHelper.LessOrEqualWithPrecision(dataPointXY.XValueInScaleUnits, 1.0);
            return false;
        }

        protected override void OnRemoved()
        {
            if (this._dataPointElementPool != null)
                this._dataPointElementPool.Clear();
            base.OnRemoved();
        }

        internal virtual Dictionary<object, List<Series>> GroupSeriesByClusters(IList<XYSeries> clusterSeries)
        {
            Dictionary<object, List<Series>> clusterGroups = new Dictionary<object, List<Series>>();
            foreach (ColumnSeries columnSeries1 in (IEnumerable<XYSeries>)clusterSeries)
            {
                List<Series> list = new List<Series>();
                if (columnSeries1.ClusterGroupKey == null)
                {
                    list.Add((Series)columnSeries1);
                    clusterGroups.Add((object)new Tuple<ColumnSeries>(columnSeries1), list);
                }
                else if (!clusterGroups.ContainsKey(columnSeries1.ClusterGroupKey))
                {
                    foreach (ColumnSeries columnSeries2 in (IEnumerable<XYSeries>)clusterSeries)
                    {
                        if (ValueHelper.AreEqual(columnSeries2.ClusterGroupKey, columnSeries1.ClusterGroupKey))
                            list.Add((Series)columnSeries2);
                    }
                    clusterGroups.Add(columnSeries1.ClusterGroupKey, list);
                }
            }
            IList<StackedColumnSeries> list1 = (IList<StackedColumnSeries>)Enumerable.ToList<StackedColumnSeries>(Enumerable.OfType<StackedColumnSeries>((IEnumerable)Enumerable.Where<XYSeries>((IEnumerable<XYSeries>)this.XYChartArea.Series, (Func<XYSeries, bool>)(s => s.Visibility == Visibility.Visible))));
            foreach (List<Series> list2 in clusterGroups.Values)
            {
                List<Series> groupSeries = list2;
                EnumerableFunctions.ForEach<Series>(Enumerable.Where<Series>((IEnumerable<Series>)groupSeries.ToArray(), (Func<Series, bool>)(s => s is StackedColumnSeries)), (Action<Series>)(s => groupSeries.Remove(s)));
            }
            EnumerableFunctions.ForEach<KeyValuePair<object, List<Series>>>(Enumerable.Where<KeyValuePair<object, List<Series>>>((IEnumerable<KeyValuePair<object, List<Series>>>)Enumerable.ToArray<KeyValuePair<object, List<Series>>>((IEnumerable<KeyValuePair<object, List<Series>>>)clusterGroups), (Func<KeyValuePair<object, List<Series>>, bool>)(item => item.Value.Count == 0)), (Action<KeyValuePair<object, List<Series>>>)(item => clusterGroups.Remove(item.Key)));
            foreach (StackedColumnSeries series1 in (IEnumerable<StackedColumnSeries>)list1)
            {
                List<Series> list2 = new List<Series>();
                Tuple<DataValueType, DataValueType, bool, object> seriesKey = StackedColumnSeriesPresenter.GetSeriesKey(series1);
                if (!clusterGroups.ContainsKey((object)seriesKey) && seriesKey.Item1 != DataValueType.Auto)
                {
                    foreach (StackedColumnSeries series2 in (IEnumerable<StackedColumnSeries>)list1)
                    {
                        if (StackedColumnSeriesPresenter.GetSeriesKey(series2).Equals((object)seriesKey))
                            list2.Add((Series)series2);
                    }
                    clusterGroups.Add((object)seriesKey, list2);
                }
            }
            return clusterGroups;
        }

        internal void CalculateRelatedSeriesPointWidth()
        {
            if (this.ChartArea == null)
                return;
            DateTime now = DateTime.Now;
            IList<XYSeries> clusterSeries = (IList<XYSeries>)Enumerable.ToList<XYSeries>(this.XYChartArea.FindClusterSeries(this.Series));
            double clusterSize = this.Series.XAxis.AxisPresenter.GetClusterSize((XYSeriesPresenter)this);
            Dictionary<object, List<Series>> dictionary = this.GroupSeriesByClusters(clusterSeries);
            int count = dictionary.Count;
            double val1_1 = double.MaxValue;
            foreach (ColumnSeries columnSeries in (IEnumerable<XYSeries>)clusterSeries)
                val1_1 = Math.Min(val1_1, columnSeries.PointGapRelativeWidth);
            double val1_2 = double.MaxValue;
            foreach (ColumnSeries columnSeries in (IEnumerable<XYSeries>)clusterSeries)
            {
                if (columnSeries.PointWidth.HasValue)
                    val1_2 = Math.Min(val1_2, columnSeries.PointWidth.Value);
            }
            if (val1_2 == double.MaxValue)
                val1_2 = clusterSize * (1.0 - val1_1) / (double)count;
            foreach (ColumnSeries columnSeries in (IEnumerable<XYSeries>)clusterSeries)
            {
                if (columnSeries.PointMaximumWidth.HasValue && val1_2 > columnSeries.PointMaximumWidth.Value)
                    val1_2 = columnSeries.PointMaximumWidth.Value;
            }
            foreach (ColumnSeries columnSeries in (IEnumerable<XYSeries>)clusterSeries)
            {
                if (columnSeries.PointMinimumWidth.HasValue && val1_2 < columnSeries.PointMinimumWidth.Value)
                    val1_2 = columnSeries.PointMinimumWidth.Value;
            }
            if (val1_2 < 1.0)
                val1_2 = 1.0;
            int num1 = 0;
            double num2 = val1_2 * (double)count;
            foreach (List<Series> list in dictionary.Values)
            {
                foreach (ColumnSeries columnSeries in list)
                {
                    ColumnSeriesPresenter columnSeriesPresenter = (ColumnSeriesPresenter)columnSeries.SeriesPresenter;
                    columnSeriesPresenter.PointWidth = val1_2;
                    columnSeriesPresenter.PointClusterOffset = -num2 / 2.0 + (double)num1 * val1_2;
                    if (count > 1)
                        columnSeriesPresenter.PointClusterOffset -= (double)num1 * ((double)count * val1_2 - num2) / (double)(count - 1);
                    if (columnSeries.PointGapRelativeWidth > val1_1)
                    {
                        double num3 = columnSeriesPresenter.PointWidth - clusterSize * (1.0 - columnSeries.PointGapRelativeWidth) / (double)count;
                        if (columnSeriesPresenter.PointWidth - num3 < 1.0)
                            num3 = columnSeriesPresenter.PointWidth - 1.0;
                        columnSeriesPresenter.PointWidth -= num3;
                        columnSeriesPresenter.PointClusterOffset += num3 / 2.0;
                    }
                }
                ++num1;
            }
            this.ChartArea.UpdateSession.AddCounter("ColumnSeriesPresenter.CalculateRelatedSeriesPointWidth", DateTime.Now - now);
        }

        public override AxisMargin GetSeriesMarginInfo(AutoBool isAxisMarginVisible)
        {
            if (isAxisMarginVisible == AutoBool.False)
                return AxisMargin.Empty;
            return base.GetSeriesMarginInfo(AutoBool.True);
        }

        internal override bool CheckSimplifiedRenderingMode()
        {
            bool flag = base.CheckSimplifiedRenderingMode();
            if (flag)
            {
                List<DataPoint> list = Enumerable.ToList<DataPoint>(Enumerable.Where<DataPoint>(this.VisibleDataPoints, (Func<DataPoint, bool>)(dataPoint =>
               {
                   if (dataPoint.View != null)
                       return dataPoint.View.MainView != null;
                   return false;
               })));
                list.ForEach((Action<DataPoint>)(dataPoint =>
               {
                   this.DataPointElementPool.Release(dataPoint.View.MainView);
                   this.OnViewRemoved(dataPoint);
                   dataPoint.View.MainView = (FrameworkElement)null;
                   dataPoint.View = (DataPointView)null;
               }));
                this.DataPointElementPool.Clear();
                list.ForEach((Action<DataPoint>)(dataPoint => this.CreateView(dataPoint)));
            }
            return flag;
        }

        internal override FrameworkElement GetLegendSymbol()
        {
            FrameworkElement viewElement = this.CreateViewElement();
            XYDataPoint xyDataPoint = Enumerable.FirstOrDefault<XYDataPoint>(Enumerable.Where<XYDataPoint>(Enumerable.OfType<XYDataPoint>((IEnumerable)this.Series.DataPoints), (Func<XYDataPoint, bool>)(p =>
          {
              if (!p.ActualIsEmpty)
                  return p.IsVisible;
              return false;
          })));
            if (xyDataPoint != null)
            {
                this.BindViewToDataPoint((DataPoint)xyDataPoint, viewElement, (string)null);
            }
            else
            {
                xyDataPoint = this.Series.CreateDataPoint() as XYDataPoint;
                xyDataPoint.Series = (Series)this.Series;
                if (this.Series.ItemsBinder != null)
                    this.Series.ItemsBinder.Bind((DataPoint)xyDataPoint, this.Series.DataContext);
                this.BindViewToDataPoint((DataPoint)xyDataPoint, viewElement, (string)null);
                xyDataPoint.Series = (Series)null;
            }
            viewElement.Opacity = xyDataPoint.Opacity;
            viewElement.Effect = xyDataPoint.Effect;
            viewElement.Width = 20.0;
            viewElement.Height = 12.0;
            return viewElement;
        }
    }
}
