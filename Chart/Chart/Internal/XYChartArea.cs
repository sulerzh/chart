using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    [TemplatePart(Name = "ChartAreaCanvas", Type = typeof(EdgePanel))]
    public class XYChartArea : ChartArea
    {
        private Queue<Tuple<object, Action>> _updateActions = new Queue<Tuple<object, Action>>();
        private ObservableCollectionSupportingInitialization<LegendItem> _legendItems = new ObservableCollectionSupportingInitialization<LegendItem>();
        private XYChartArea.XYSeriesQueue _loadingQueue = new XYChartArea.XYSeriesQueue();
        internal const string UpdateLegendItemsTaskKey = "__UpdateLegendItems__";
        internal const string PercentFormat = "P0";
        public const string DataVirtualizerPropertyName = "DataVirtualizer";
        private const double ZoomRate = 1.2;
        private FlowDirection _currentFlowDirection;
        private Size _availableSize;
        private ObservableCollection<XYSeries> _series;
        private ObservableCollection<Axis> _axes;
        private IXYChartAreaDataVirtualizer _dataVirtualizer;
        private bool _viewExists;

        public Collection<XYSeries> Series
        {
            get
            {
                return (Collection<XYSeries>)this._series;
            }
            set
            {
                throw new NotSupportedException("Not supported!");
            }
        }

        public ObservableCollection<LegendItem> LegendItems
        {
            get
            {
                return (ObservableCollection<LegendItem>)this._legendItems;
            }
        }

        public ObservableCollection<Axis> Axes
        {
            get
            {
                return this._axes;
            }
            set
            {
                throw new NotSupportedException(Properties.Resources.Chart_Axes_SetterNotSupported);
            }
        }

        public IXYChartAreaDataVirtualizer DataVirtualizer
        {
            get
            {
                return this._dataVirtualizer;
            }
            set
            {
                if (this._dataVirtualizer == value)
                    return;
                IXYChartAreaDataVirtualizer oldValue = this._dataVirtualizer;
                this._dataVirtualizer = value;
                this.OnDataVirtualizerPropertyChanged(oldValue, value);
            }
        }

        public override bool IsZoomed
        {
            get
            {
                foreach (Axis axis in (Collection<Axis>)this.Axes)
                {
                    if (axis.Scale.ActualZoom > 1.0)
                        return true;
                }
                return false;
            }
        }

        internal IEnumerable<XYSeries> VisibleSeries
        {
            get
            {
                return Enumerable.Where<XYSeries>((IEnumerable<XYSeries>)this.Series, (Func<XYSeries, bool>)(s => s.Visibility == Visibility.Visible));
            }
        }

        public XYChartArea()
        {
            this.DefaultStyleKey = (object)typeof(XYChartArea);
            this._series = (ObservableCollection<XYSeries>)new UniqueObservableCollection<XYSeries>();
            this.SubscribeToSeriesCollectionChanged();
            this._axes = (ObservableCollection<Axis>)new AxesCollection((ChartArea)this);
            this.SubscribeToAxesCollectionChanged();
            this.LayoutUpdated += (EventHandler)((s, e) => this.OnLayoutUpdated());
        }

        public override void Update()
        {
            this.UpdateSession.BeginUpdates();
            foreach (Axis axis in (Collection<Axis>)this.Axes)
                axis.Update();
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
                series.Update();
            this.UpdateSession.EndUpdates();
        }

        public override void UpdatePlotArea()
        {
            this.UpdateSession.BeginUpdates();
            while (this._updateActions.Count > 0)
                this._updateActions.Dequeue().Item2();
            foreach (IUpdatable element in this.Series)
                this.UpdateSession.Update(element);
            this.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.SelectionPanel.Invalidate()), (object)"SelectionPanel_Invalidate", (string)null);
            this.UpdateSession.EndUpdates();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (availableSize != this._availableSize)
            {
                this.OnAvailableSizeChanging(this._availableSize, availableSize);
                this._availableSize = availableSize;
            }
            return base.MeasureOverride(availableSize);
        }

        protected virtual void OnAvailableSizeChanging(Size currentSize, Size newSize)
        {
            foreach (Axis axis in (Collection<Axis>)this.Axes)
                axis.OnAvailableSizeChanging();
        }

        private bool IsXAxisReverseRequired(XYSeries series)
        {
            if (this.FlowDirection == FlowDirection.RightToLeft && this.Orientation == Orientation.Horizontal)
                return series.XAxis.Scale is NumericScale;
            return false;
        }

        private void OnLayoutUpdated()
        {
            if (this._currentFlowDirection == this.FlowDirection)
                return;
            this._currentFlowDirection = this.FlowDirection;
            foreach (XYSeries series in this.Series)
                series.XAxis.IsReversed = this.IsXAxisReverseRequired(series);
        }

        private void OnDataVirtualizerPropertyChanged(IXYChartAreaDataVirtualizer oldValue, IXYChartAreaDataVirtualizer newValue)
        {
            if (oldValue != null)
            {
                foreach (XYSeries series in this.Series)
                    oldValue.UninitializeSeries(series);
                foreach (Axis axis in (Collection<Axis>)this.Axes)
                {
                    if (axis.Scale != null)
                        oldValue.UninitializeAxisScale(axis, axis.Scale);
                }
            }
            if (newValue != null)
            {
                foreach (XYSeries series in this.Series)
                    newValue.InitializeSeries(series);
                foreach (Axis axis in (Collection<Axis>)this.Axes)
                {
                    if (axis.Scale != null)
                        newValue.InitializeAxisScale(axis, axis.Scale);
                }
                if (!this.IsInitializing)
                    this.SyncSeriesAndAxes();
                this.Invalidate();
            }
            this.OnPropertyChanged("DataVirtualizer");
        }

        public override Point ConvertDataToPlotCoordinate(Axis xAxis, Axis yAxis, object x, object y)
        {
            Point point = new Point(0.0, 0.0);
            if (xAxis.AxisPresenter != null)
                point.X = xAxis.AxisPresenter.ConvertDataToAxisUnits(x) ?? 0.0;
            if (yAxis.AxisPresenter != null)
                point.Y = yAxis.AxisPresenter.ConvertDataToAxisUnits(y) ?? 0.0;
            if (this.Orientation == Orientation.Vertical)
                return new Point(point.Y, point.X);
            return point;
        }

        public override Point ConvertScaleToPlotCoordinate(Axis xAxis, Axis yAxis, double x, double y)
        {
            Point point = new Point(0.0, 0.0);
            if (xAxis.AxisPresenter != null)
                point.X = xAxis.AxisPresenter.ConvertScaleToAxisUnits(x) ?? 0.0;
            if (yAxis.AxisPresenter != null)
                point.Y = yAxis.AxisPresenter.ConvertScaleToAxisUnits(y) ?? 0.0;
            if (this.Orientation == Orientation.Vertical)
                return new Point(point.Y, point.X);
            return point;
        }

        internal override void ResetView()
        {
            this.RemoveView();
            this.CreateView();
            this.UpdateLegendItems();
        }

        private void CreateView()
        {
            if (this._viewExists)
                return;
            this.ReapplyPalette();
            foreach (Axis axis in (Collection<Axis>)this.Axes)
                axis.AxisPresenter.OnAxisAdded();
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
                series.SeriesPresenter.OnSeriesAdded();
            this._viewExists = true;
        }

        private void RemoveView()
        {
            if (!this._viewExists)
                return;
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
                series.SeriesPresenter.OnSeriesRemoved();
            foreach (Axis axis in (Collection<Axis>)this.Axes)
                axis.AxisPresenter.OnAxisRemoved();
            this._viewExists = false;
        }

        public void HideSeries()
        {
            foreach (object layerKey in this.Series)
                this.ChartAreaLayerProvider.SetLayerVisibility(layerKey, Visibility.Collapsed);
            this.ChartAreaLayerProvider.SetLayerVisibility((object)LayerType.SmartLabels, Visibility.Collapsed);
        }

        public void ResetSeries()
        {
            this.UnsubscribeToSeriesCollectionChanged();
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
            {
                this.ChartAreaLayerProvider.RemoveLayer((object)series);
                this.DeactivateChildModel((FrameworkElement)series);
                series.Unbind();
                series.ChartArea = (ChartArea)null;
            }
            this.ChartAreaLayerProvider.RemoveLayer((object)LayerType.SmartLabels);
            this.ReapplyPalette();
            this._series = new ObservableCollection<XYSeries>();
            this.ResetSingletonRegistry();
            this._updateActions.Clear();
            this._legendItems.Clear();
            this.SubscribeToSeriesCollectionChanged();
        }

        internal override void OnMeasureIterationStarts()
        {
            foreach (Axis axis in (Collection<Axis>)this.Axes)
                axis.AxisPresenter.OnMeasureIterationStarts();
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
                series.SeriesPresenter.OnMeasureIterationStarts();
        }

        private void SubscribeToSeriesCollectionChanged()
        {
            this._series.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnSeriesCollectionChanged);
        }

        private void UnsubscribeToSeriesCollectionChanged()
        {
            this._series.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.OnSeriesCollectionChanged);
        }

        private void OnSeriesPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (this.IsInitializing)
                return;
            XYSeries series = sender as XYSeries;
            switch (args.PropertyName)
            {
                case "ActualXDataRange":
                    this.UpdateScaleRangeIfUndefined(series.XAxis, series.ActualXValueType);
                    break;
                case "ActualYDataRange":
                    this.UpdateScaleRangeIfUndefined(series.YAxis, series.ActualYValueType);
                    break;
                case "XValues":
                    this.UpdateScaleValuesIfUndefined(series.XAxis, series.ActualXValueType);
                    break;
                case "YValues":
                    this.UpdateScaleValuesIfUndefined(series.YAxis, series.ActualYValueType);
                    break;
                case "ActualXValueType":
                    this.UpdateScaleValueType(series.XAxis);
                    series.XAxis.IsReversed = this.IsXAxisReverseRequired(series);
                    break;
                case "ActualYValueType":
                    this.UpdateScaleValueType(series.YAxis);
                    break;
                case "Visibility":
                    this.UpdateScaleRangeIfUndefined(series.XAxis, series.ActualXValueType);
                    this.UpdateScaleRangeIfUndefined(series.YAxis, series.ActualYValueType);
                    break;
            }
        }

        private void UpdateScaleRangeIfUndefined(Axis axis, DataValueType valueType)
        {
            if (valueType == DataValueType.Auto)
                return;
            if (axis.Scale.CanProject(valueType))
                axis.Scale.UpdateRangeIfUndefined(this.AggregateRange(axis));
            else
                this.SyncSeriesAndAxes();
        }

        private void UpdateScaleValuesIfUndefined(Axis axis, DataValueType valueType)
        {
            if (valueType == DataValueType.Auto)
                return;
            if (axis.Scale.CanProject(valueType))
                axis.Scale.UpdateValuesIfUndefined(this.AggregateValues(axis));
            else
                this.SyncSeriesAndAxes();
        }

        private void UpdateScaleValueType(Axis axis)
        {
            DataValueType valueType = this.AggregateValueType(axis);
            if (axis != null && axis.Scale != null && axis.Scale.CanProject(valueType))
                axis.Scale.UpdateValueType(valueType);
            else
                this.SyncSeriesAndAxes();
        }

        private void OnSeriesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.ResetView();
            }
            else
            {
                if (e.OldItems != null)
                {
                    foreach (XYSeries series in (IEnumerable)e.OldItems)
                    {
                        this.OnSeriesRemoved(series);
                        this.UpdateSession.SkipUpdate((IUpdatable)series);
                    }
                }
                if (e.NewItems != null)
                {
                    foreach (XYSeries series in (IEnumerable)e.NewItems)
                        this.OnSeriesAdded(series);
                    this.ReapplyPalette();
                }
            }
            this.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.UpdateLegendItems()), (object)"__UpdateLegendItems__", (string)null);
        }

        public void UpdateLegendItems()
        {
            if (!this.IsTemplateApplied)
                return;
            List<LegendItem> list = new List<LegendItem>();
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
            {
                LegendItem legendItem = series.SeriesPresenter.GetLegendItem();
                if (legendItem != null)
                    list.Add(legendItem);
            }
            this._legendItems.ResetIfNecessary((IList<LegendItem>)list);
        }

        private void OnSeriesAdded(XYSeries series)
        {
            series.ChartArea = (ChartArea)this;
            series.PropertyChanged += new PropertyChangedEventHandler(this.OnSeriesPropertyChanged);
            if (!this.IsInitializing)
            {
                this.SyncSeriesAndAxes();
            }
            else
            {
                series.UpdateActualValueTypes();
                series.UpdateActualDataPoints();
                series.UpdateActualDataRange();
                this.GetScale(series, AxisOrientation.X);
                this.GetScale(series, AxisOrientation.Y);
            }
            if (this.IsTemplateApplied)
                series.SeriesPresenter.OnSeriesAdded();
            if (this.DataVirtualizer == null)
                return;
            this.DataVirtualizer.InitializeSeries(series);
            this.LoadVirtualizedData((IEnumerable<XYSeries>)new XYSeries[1]
            {
        series
            });
        }

        private void OnSeriesRemoved(XYSeries series)
        {
            series.PropertyChanged -= new PropertyChangedEventHandler(this.OnSeriesPropertyChanged);
            if (!this.IsInitializing)
                this.SyncSeriesAndAxes();
            if (!this.IsTemplateApplied)
                return;
            series.SeriesPresenter.OnSeriesRemoved();
        }

        private void SubscribeToAxesCollectionChanged()
        {
            this._axes.CollectionChanged += new NotifyCollectionChangedEventHandler(this.OnAxesCollectionChanged);
        }

        private void OnAxesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Axis axis in (IEnumerable)e.OldItems)
                    this.OnAxisRemoved(axis);
            }
            if (e.NewItems == null)
                return;
            foreach (Axis axis in (IEnumerable)e.NewItems)
                this.OnAxisAdded(axis);
        }

        private void OnAxisAdded(Axis axis)
        {
            axis.ChartArea = (ChartArea)this;
            axis.AxisPresenter.OnAxisAdded();
            axis.ScaleChanged += new EventHandler(this.OnAxisScaleChanged);
            axis.ScaleViewChanged += new EventHandler<ScaleViewChangedArgs>(this.OnAxisScaleViewChanged);
            this.UpdateSession.SkipUpdate((IUpdatable)axis);
            if (axis.Scale == null || this.DataVirtualizer == null)
                return;
            this.DataVirtualizer.InitializeAxisScale(axis, axis.Scale);
        }

        private void OnAxisRemoved(Axis axis)
        {
            axis.AxisPresenter.OnAxisRemoved();
            axis.ScaleChanged -= new EventHandler(this.OnAxisScaleChanged);
            axis.ScaleViewChanged -= new EventHandler<ScaleViewChangedArgs>(this.OnAxisScaleViewChanged);
            this.UpdateSession.SkipUpdate((IUpdatable)axis);
            axis.ChartArea = (ChartArea)null;
        }

        internal virtual void OnAxisScaleChanged(object sender, EventArgs e)
        {
            Axis axis = sender as Axis;
            this.UpdateSession.BeginUpdates();
            foreach (XYSeries xySeries in this.Series)
            {
                XYSeriesPresenter presenter = (XYSeriesPresenter)xySeries.SeriesPresenter;
                Action action = (Action)null;
                if (axis == xySeries.XAxis)
                    action = (Action)(() => presenter.OnXScaleChanged());
                else if (axis == xySeries.YAxis)
                    action = (Action)(() => presenter.OnYScaleChanged());
                if (action != null)
                {
                    if (this.ChartAreaPanel != null && this.ChartAreaPanel.IsDirty)
                    {
                        XYChartArea.SeriesAxisKey key = new XYChartArea.SeriesAxisKey()
                        {
                            Series = (Semantic.Reporting.Windows.Chart.Internal.Series)xySeries,
                            Axis = axis
                        };
                        if (EnumerableFunctions.FindIndexOf<Tuple<object, Action>>((IEnumerable<Tuple<object, Action>>)this._updateActions, (Func<Tuple<object, Action>, bool>)(t => key.Equals(t.Item1))) == -1)
                            this._updateActions.Enqueue(new Tuple<object, Action>((object)key, action));
                    }
                    else
                        action();
                }
            }
            this.UpdateSession.EndUpdates();
        }

        internal virtual void OnAxisScaleViewChanged(object sender, ScaleViewChangedArgs e)
        {
            Axis axis = sender as Axis;
            this.UpdateSession.BeginUpdates();
            foreach (XYSeries xySeries in this.Series)
            {
                if (axis == xySeries.XAxis)
                    ((XYSeriesPresenter)xySeries.SeriesPresenter).OnXScaleViewChanged(e.OldRange, e.NewRange);
                else if (axis == xySeries.YAxis)
                    ((XYSeriesPresenter)xySeries.SeriesPresenter).OnYScaleViewChanged(e.OldRange, e.NewRange);
            }
            if (!this.IsInitializing)
                this.LoadVirtualizedData(Enumerable.OfType<XYSeries>((IEnumerable)this.FindSeries(axis)));
            this.UpdateSession.EndUpdates();
        }

        internal override IEnumerable<Semantic.Reporting.Windows.Chart.Internal.Series> FindSeries(Axis axis)
        {
            if (axis.Orientation == AxisOrientation.X)
            {
                foreach (XYSeries xySeries in this.Series)
                {
                    if (xySeries.XAxisName == axis.Name)
                        yield return (Semantic.Reporting.Windows.Chart.Internal.Series)xySeries;
                }
            }
            else
            {
                foreach (XYSeries xySeries in this.Series)
                {
                    if (xySeries.YAxisName == axis.Name)
                        yield return (Semantic.Reporting.Windows.Chart.Internal.Series)xySeries;
                }
            }
        }

        private IEnumerable<Semantic.Reporting.Windows.Chart.Internal.Series> FindSeriesWithDefinedValueType(Axis axis)
        {
            if (axis.Orientation == AxisOrientation.X)
            {
                foreach (XYSeries xySeries in this.Series)
                {
                    if (xySeries.XAxisName == axis.Name && xySeries.ActualXValueType != DataValueType.Auto)
                        yield return (Semantic.Reporting.Windows.Chart.Internal.Series)xySeries;
                }
            }
            else
            {
                foreach (XYSeries xySeries in this.Series)
                {
                    if (xySeries.YAxisName == axis.Name && xySeries.ActualYValueType != DataValueType.Auto)
                        yield return (Semantic.Reporting.Windows.Chart.Internal.Series)xySeries;
                }
            }
        }

        internal IEnumerable<XYSeries> FindClusterSeries(XYSeries series)
        {
            return Enumerable.Where<XYSeries>((IEnumerable<XYSeries>)this.Series, (Func<XYSeries, bool>)(item =>
          {
              if (item.ClusterKey == series.ClusterKey)
                  return item.Visibility == Visibility.Visible;
              return false;
          }));
        }

        internal override IEnumerable<Semantic.Reporting.Windows.Chart.Internal.Series> GetSeries()
        {
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
                yield return series;
        }

        public Scale GetScale(XYSeries series, AxisOrientation orientation)
        {
            Axis axis = this.GetAxis(series, orientation);
            DataValueType valueType = orientation == AxisOrientation.X ? series.ActualXValueType : series.ActualYValueType;
            if (valueType != DataValueType.Auto)
            {
                if (axis.Scale != null && !axis.Scale.CanProject(valueType))
                {
                    bool flag = false;
                    foreach (XYSeries xySeries in this.FindSeriesWithDefinedValueType(axis))
                    {
                        if (xySeries != series && xySeries.DataPoints.Count > 0)
                        {
                            DataValueType dataValueType = orientation == AxisOrientation.X ? xySeries.ActualXValueType : xySeries.ActualYValueType;
                            if (valueType != dataValueType && dataValueType != DataValueType.Auto)
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        axis.Scale = (Scale)null;
                    }
                    else
                    {
                        axis = this.FindAxis(valueType, orientation) ?? this.CreateAxis((string)null, orientation);
                        series.XAxisName = axis.Name;
                    }
                }
            }
            else
                valueType = DataValueType.Float;
            if (axis.Scale == null)
            {
                axis.Scale = Scale.CreateScaleByType(valueType);
                if (this.DataVirtualizer != null)
                    this.DataVirtualizer.InitializeAxisScale(axis, axis.Scale);
            }
            return axis.Scale;
        }

        internal Axis GetAxis(XYSeries series, AxisOrientation orientation)
        {
            Axis axis;
            if (orientation == AxisOrientation.X)
            {
                axis = this.GetAxis(series.XAxisName, orientation);
                series.XAxisName = axis.Name;
            }
            else
            {
                axis = this.GetAxis(series.YAxisName, orientation);
                series.YAxisName = axis.Name;
            }
            return axis;
        }

        internal Axis GetAxis(string axisName, AxisOrientation orientation)
        {
            return this.FindAxis(axisName, orientation) ?? this.CreateAxis(axisName, orientation);
        }

        private Axis FindAxis(DataValueType valueType, AxisOrientation orientation)
        {
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                if (axis.Orientation == orientation && axis.Scale.ValueType == valueType)
                    return axis;
            }
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                if (axis.Orientation == orientation && axis.Scale.CanProject(valueType))
                    return axis;
            }
            return (Axis)null;
        }

        private Axis FindAxis(string axisName, AxisOrientation orientation)
        {
            if (string.IsNullOrEmpty(axisName))
            {
                foreach (Axis axis in (Collection<Axis>)this.Axes)
                {
                    if (axis.Orientation == orientation)
                        return axis;
                }
            }
            else
            {
                foreach (Axis axis in (Collection<Axis>)this.Axes)
                {
                    if (axisName == axis.Name)
                        return axis;
                }
            }
            return (Axis)null;
        }

        private Axis CreateAxis(string axisName, AxisOrientation orientation)
        {
            if (string.IsNullOrEmpty(axisName))
                axisName = XamlShims.NewFrameworkElementName();
            Axis axis = Axis.CreateAxis(axisName, orientation);
            this.Axes.Add(axis);
            return axis;
        }

        internal override bool CanRemoveAxis(Axis axis)
        {
            foreach (XYSeries xySeries in this.Series)
            {
                if (xySeries.XAxis == axis || xySeries.YAxis == axis)
                    return false;
            }
            return true;
        }

        private void VerifyPercentScaleLabelFormat(Axis axis, bool hasHundredPercentStackSeries)
        {
            if (hasHundredPercentStackSeries)
            {
                if (!string.Equals(axis.Scale.LabelDefinition.Format, "{0}", StringComparison.Ordinal))
                    return;
                axis.Scale.LabelDefinition.Format = "P0";
            }
            else
            {
                if (!string.Equals(axis.Scale.LabelDefinition.Format, "P0", StringComparison.Ordinal))
                    return;
                axis.Scale.LabelDefinition.Format = "{0}";
            }
        }

        public override void SyncSeriesAndAxes()
        {
            this.BeginInitCore();
            foreach (Semantic.Reporting.Windows.Chart.Internal.Series series in this.Series)
                series.UpdateRelatedSeries();
            foreach (XYSeries xySeries in this.Series)
            {
                xySeries.UpdateActualValueTypes();
                xySeries.UpdateActualDataPoints();
                xySeries.UpdateActualDataRange();
            }
            List<Axis> list = new List<Axis>();
            foreach (XYSeries series in this.Series)
            {
                this.GetScale(series, AxisOrientation.X);
                this.GetScale(series, AxisOrientation.Y);
                list.Add(series.XAxis);
                list.Add(series.YAxis);
            }
            int index = 0;
            while (index < this.Axes.Count)
            {
                Axis axis = this.Axes[index];
                if (axis.IsAutoCreated && !list.Contains(axis))
                    this.Axes.RemoveAt(index);
                else
                    ++index;
            }
            this.UpdateSession.BeginUpdates();
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                IEnumerable<Semantic.Reporting.Windows.Chart.Internal.Series> series = this.FindSeries(axis);
                if (axis.Scale != null && Enumerable.Any<Semantic.Reporting.Windows.Chart.Internal.Series>(series))
                {
                    if (axis.Orientation == AxisOrientation.Y)
                        this.VerifyPercentScaleLabelFormat(axis, Enumerable.FirstOrDefault<StackedColumnSeries>(Enumerable.OfType<StackedColumnSeries>((IEnumerable)series), (Func<StackedColumnSeries, bool>)(s => s.ActualIsHundredPercent)) != null);
                    axis.Scale.BeginInit();
                    axis.Scale.UpdateValueType(this.AggregateValueType(axis));
                    axis.Scale.UpdateRangeIfUndefined(this.AggregateRange(axis));
                    axis.Scale.UpdateValuesIfUndefined(this.AggregateValues(axis));
                    axis.Scale.UpdateDefaults(this.AggregateScaleDefaults(axis));
                    axis.Scale.EndInit();
                }
                else if (axis.Scale == null)
                    axis.Scale = Scale.CreateScaleByType(DataValueType.Integer);
            }
            this.UpdateSession.EndUpdates();
            this.EndInitCore();
        }

        private void LoadVirtualizedData(IEnumerable<XYSeries> series)
        {
            IList<XYSeries> series1 = this._loadingQueue.Add(series);
            if (this.DataVirtualizer != null && series1.Count != 0)
                this.DataVirtualizer.UpdateSeriesForCurrentView((IEnumerable<XYSeries>)series1);
            this._loadingQueue.Remove(series1);
        }

        internal override void ZoomPlotArea(bool isZoomIn)
        {
            this.ZoomPlotArea(isZoomIn, 0.5, 0.5);
        }

        protected override void ZoomPlotArea(MouseWheelEventArgs e)
        {
            Point position = e.GetPosition((IInputElement)this.PlotAreaPanel);
            if (!this.IsMouseZoomEnabled)
                return;
            this.ZoomPlotArea(e.Delta > 0, position.X / this.PlotAreaPanel.ActualWidth, (this.PlotAreaPanel.ActualHeight - position.Y) / this.PlotAreaPanel.ActualHeight);
        }

        private void ZoomPlotArea(bool isZoomIn, double xCenterValue, double yCenterValue)
        {
            double delta = isZoomIn ? 1.2 : 5.0 / 6.0;
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                if (axis.ActualIsZoomEnabled)
                {
                    if (this.IsHorizontalAxis(axis))
                        axis.Scale.ZoomBy(xCenterValue, delta);
                    else
                        axis.Scale.ZoomBy(yCenterValue, delta);
                    axis.ShowScrollZoomBar = DoubleHelper.GreaterWithPrecision(axis.Scale.ActualZoom, 1.0);
                }
            }
        }

        protected override void DragPlotArea(MouseEventArgs oldArgs, MouseEventArgs newArgs)
        {
            Point position1 = oldArgs.GetPosition((IInputElement)this.PlotAreaPanel);
            Point position2 = newArgs.GetPosition((IInputElement)this.PlotAreaPanel);
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                double offset = !this.IsHorizontalAxis(axis) ? (position2.Y - position1.Y) / this.PlotAreaPanel.ActualHeight : (position1.X - position2.X) / this.PlotAreaPanel.ActualWidth;
                if (axis.AxisPresenter.ActualIsScaleReversed)
                    offset *= -1.0;
                axis.Scale.ScrollBy(offset);
            }
        }

        internal override void ScrollPlotArea(bool isForward, bool isHorizontalScroll)
        {
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                bool flag = this.IsHorizontalAxis(axis);
                double position = isForward ? 1.0 : 0.0;
                if (flag && isHorizontalScroll)
                    axis.Scale.ScrollToPercent(position);
                else if (!flag && !isHorizontalScroll)
                {
                    if (this.Orientation == Orientation.Horizontal)
                        position = isForward ? 0.0 : 1.0;
                    axis.Scale.ScrollToPercent(position);
                }
            }
        }

        internal override void ScrollPlotArea(int numOfView, bool isHorizontalScroll)
        {
            foreach (Axis axis in (Collection<Axis>)this.Axes)
            {
                bool flag = this.IsHorizontalAxis(axis);
                if (flag && isHorizontalScroll)
                    axis.Scale.ScrollBy((double)numOfView);
                else if (!flag && !isHorizontalScroll)
                {
                    if (this.Orientation == Orientation.Horizontal)
                        numOfView *= -1;
                    axis.Scale.ScrollBy((double)numOfView);
                }
            }
        }

        private bool IsHorizontalAxis(Axis axis)
        {
            if (axis.Orientation == AxisOrientation.X && this.Orientation == Orientation.Horizontal)
                return true;
            if (axis.Orientation == AxisOrientation.Y)
                return this.Orientation == Orientation.Vertical;
            return false;
        }

        internal virtual IEnumerable<Range<IComparable>> AggregateRange(Axis axis)
        {
            if (axis.Orientation == AxisOrientation.X)
            {
                foreach (XYSeries xySeries in this.VisibleSeries)
                {
                    if (xySeries.XAxisName == axis.Name)
                        yield return xySeries.ActualXDataRange;
                }
            }
            else
            {
                foreach (XYSeries xySeries in this.VisibleSeries)
                {
                    if (xySeries.YAxisName == axis.Name)
                        yield return xySeries.ActualYDataRange;
                }
            }
        }

        internal virtual IEnumerable<object> AggregateValues(Axis axis)
        {
            if (axis.Orientation == AxisOrientation.X)
            {
                foreach (XYSeries xySeries in this.VisibleSeries)
                {
                    if (xySeries.XAxisName == axis.Name)
                    {
                        foreach (object obj in xySeries.XValues)
                            yield return obj;
                    }
                }
            }
            else
            {
                foreach (XYSeries xySeries in this.VisibleSeries)
                {
                    if (xySeries.YAxisName == axis.Name)
                    {
                        foreach (object obj in xySeries.YValues)
                            yield return obj;
                    }
                }
            }
        }

        internal virtual IEnumerable<object> AggregateXValues(IEnumerable<XYSeries> series)
        {
            foreach (XYSeries xySeries in series)
            {
                if (xySeries.Visibility == Visibility.Visible)
                {
                    foreach (object obj in xySeries.XValues)
                        yield return obj;
                }
            }
        }

        internal virtual IEnumerable<object> AggregateYValues(IEnumerable<XYSeries> series)
        {
            foreach (XYSeries xySeries in series)
            {
                if (xySeries.Visibility == Visibility.Visible)
                {
                    foreach (object obj in xySeries.YValues)
                        yield return obj;
                }
            }
        }

        internal virtual DataValueType AggregateValueType(Axis axis)
        {
            DataValueType a = DataValueType.Auto;
            if (axis.Orientation == AxisOrientation.X)
            {
                foreach (XYSeries xySeries in this.VisibleSeries)
                {
                    if (xySeries.XAxisName == axis.Name)
                        a = ValueHelper.CombineDataValueTypes(a, xySeries.ActualXValueType);
                }
                return a;
            }
            foreach (XYSeries xySeries in this.VisibleSeries)
            {
                if (xySeries.YAxisName == axis.Name)
                    a = ValueHelper.CombineDataValueTypes(a, xySeries.ActualYValueType);
            }
            return a;
        }

        internal virtual ScaleDefaults AggregateScaleDefaults(Axis axis)
        {
            ScaleDefaults scaleDefaults = new ScaleDefaults();
            if (axis.Orientation == AxisOrientation.X)
            {
                foreach (XYSeries xySeries in this.Series)
                {
                    if (xySeries.XAxisName == axis.Name)
                        scaleDefaults += xySeries.XScaleDefaults;
                }
                return scaleDefaults;
            }
            foreach (XYSeries xySeries in this.Series)
            {
                if (xySeries.YAxisName == axis.Name)
                    scaleDefaults += xySeries.YScaleDefaults;
            }
            return scaleDefaults;
        }

        private class XYSeriesQueue
        {
            private HashSet<XYSeries> _series = new HashSet<XYSeries>();

            public IList<XYSeries> Add(IEnumerable<XYSeries> series)
            {
                List<XYSeries> list = new List<XYSeries>();
                foreach (XYSeries xySeries in series)
                {
                    if (!this._series.Contains(xySeries))
                    {
                        this._series.Add(xySeries);
                        list.Add(xySeries);
                    }
                }
                return (IList<XYSeries>)list;
            }

            public void Remove(IList<XYSeries> series)
            {
                foreach (XYSeries xySeries in (IEnumerable<XYSeries>)series)
                    this._series.Remove(xySeries);
            }
        }

        private class SeriesAxisKey
        {
            public Semantic.Reporting.Windows.Chart.Internal.Series Series { get; set; }

            public Axis Axis { get; set; }

            public override bool Equals(object obj)
            {
                XYChartArea.SeriesAxisKey seriesAxisKey = obj as XYChartArea.SeriesAxisKey;
                if (seriesAxisKey == null)
                    return base.Equals(obj);
                if (seriesAxisKey.Axis == this.Axis)
                    return seriesAxisKey.Series == this.Series;
                return false;
            }

            public override int GetHashCode()
            {
                if (this.Series == null || this.Axis == null)
                    return this.GetType().GetHashCode();
                return this.Axis.GetHashCode() ^ this.Series.GetHashCode();
            }
        }
    }
}
