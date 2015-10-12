﻿using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class StackedColumnSeriesPresenter : ColumnSeriesPresenter
    {
        private static Guid _nullKey = Guid.NewGuid();
        internal const double HundredPercentScaleRange = 1.0;
        private bool _suppressCounterpartInvalidationFlag;
        private Dictionary<object, Range<IComparable>> _dataPointsByXValueRanges;
        private Tuple<DataValueType, DataValueType, bool, object> _aquiredStackingKey;
        private List<XYSeries> _conterpartSeries;
        private List<XYSeries> _conterpartSeriesAll;
        private Dictionary<object, List<XYDataPoint>> _counterpartDataPointsAll;

        internal Dictionary<object, Range<IComparable>> DataPointsByXValueRanges
        {
            get
            {
                this.ReleaseDataPointsByXValueRanges();
                if (this._dataPointsByXValueRanges == null)
                {
                    this._aquiredStackingKey = StackedColumnSeriesPresenter.GetSeriesKey(this.Series as StackedColumnSeries);
                    this._dataPointsByXValueRanges = (Dictionary<object, Range<IComparable>>)this.ChartArea.SingletonRegistry.GetSingleton((object)this._aquiredStackingKey, (Func<object>)(() => (object)new Dictionary<object, Range<IComparable>>()), (Action<object>)null);
                }
                return this._dataPointsByXValueRanges;
            }
        }

        public StackedColumnSeriesPresenter(StackedColumnSeries series)
          : base((XYSeries)series)
        {
        }

        internal override SeriesLabelPresenter CreateLabelPresenter()
        {
            return (SeriesLabelPresenter)new StackedColumnSeriesLabelPresenter((SeriesPresenter)this);
        }

        private void ReleaseDataPointsByXValueRanges()
        {
            if (this._aquiredStackingKey == StackedColumnSeriesPresenter.GetSeriesKey(this.Series as StackedColumnSeries))
                return;
            if (this._dataPointsByXValueRanges != null)
                this.ChartArea.SingletonRegistry.ReleaseSingleton((object)this._aquiredStackingKey);
            this._dataPointsByXValueRanges = (Dictionary<object, Range<IComparable>>)null;
        }

        internal bool IsHundredPercent()
        {
            return this.IsHundredPercent((Series)this.Series);
        }

        private bool IsHundredPercent(Series series)
        {
            StackedColumnSeries stackedColumnSeries = series as StackedColumnSeries;
            if (stackedColumnSeries == null)
                return true;
            if (stackedColumnSeries != null)
                return stackedColumnSeries.ActualIsHundredPercent;
            return false;
        }

        private bool IsHundredPercent(XYDataPoint dataPoint)
        {
            return this.IsHundredPercent(dataPoint.Series);
        }

        private double TransformYValueInScaleUnits(XYDataPoint dataPoint, double yValueInScaleUnits)
        {
            if (!this.IsHundredPercent(dataPoint))
                return yValueInScaleUnits;
            Range<IComparable> pointYvalueRange = this.GetDataPointYValueRange(dataPoint);
            double val2 = (double)ValueHelper.ConvertValue((object)pointYvalueRange.Minimum, DataValueType.Float);
            double num = Math.Max(0.0, (double)ValueHelper.ConvertValue((object)pointYvalueRange.Maximum, DataValueType.Float)) - Math.Min(0.0, val2);
            NumericScale numericScale = this.Series.YAxis.Scale as NumericScale;
            if (num == 0.0)
                return this.Series.YAxis.Scale.ProjectDataValue(this.Series.YAxis.Scale.ActualCrossingPosition);
            Range<double> fromRange = new Range<double>(numericScale.Project(0.0), numericScale.Project(num));
            Range<double> targetRange = new Range<double>(numericScale.Project(0.0), numericScale.Project(1.0));
            return RangeHelper.Project(fromRange, yValueInScaleUnits, targetRange);
        }

        internal Range<IComparable> ConvertToPercentRange(Range<IComparable> range)
        {
            if (((StackedColumnSeries)this.Series).ActualIsHundredPercent && range.HasData)
            {
                Range<double> range1 = new Range<double>(Math.Min(0.0, ValueHelper.ToDouble((object)range.Minimum)), Math.Max(0.0, ValueHelper.ToDouble((object)range.Maximum)));
                double num1 = range1.Maximum + Math.Abs(range1.Minimum);
                double num2 = (double)Math.Sign(range1.Minimum);
                double num3 = (double)Math.Sign(range1.Maximum);
                if (num1 != 0.0)
                {
                    num2 = Math.Abs(range1.Minimum) / num1 * (double)Math.Sign(range1.Minimum) * 1.0;
                    num3 = Math.Abs(range1.Maximum) / num1 * (double)Math.Sign(range1.Maximum) * 1.0;
                }
                else if (num2 == num3)
                {
                    num3 += num2 <= 0.0 ? 1.0 : 0.0;
                    num2 -= num2 > 0.0 ? 1.0 : 0.0;
                }
                range = new Range<IComparable>((IComparable)(num2 * 0.99), (IComparable)(num3 * 0.99));
            }
            return range;
        }

        internal Range<IComparable> GetDataPointYValueRange(object xValue, List<XYSeries> series)
        {
            if (this.Series.ActualYValueType == DataValueType.Auto)
                return Range<IComparable>.Empty;
            List<IComparable> list = new List<IComparable>();
            object obj = new object();
            Dictionary<object, List<IComparable>> dictionary = new Dictionary<object, List<IComparable>>();
            object index1 = ValueHelper.ConvertValue(xValue, this.Series.ActualXValueType);
            foreach (ColumnSeries columnSeries in Enumerable.OfType<ColumnSeries>((IEnumerable)series))
            {
                foreach (XYDataPoint xyDataPoint in columnSeries.DataPointsByXValue[index1])
                {
                    if (xyDataPoint.YValue != null)
                    {
                        IComparable comparable = ValueHelper.ConvertValue(xyDataPoint.YValue, this.Series.ActualYValueType) as IComparable;
                        object key = columnSeries.ClusterGroupKey ?? obj;
                        if (!dictionary.ContainsKey(key))
                            dictionary[key] = new List<IComparable>((IEnumerable<IComparable>)new IComparable[1]
                            {
                comparable
                            });
                        else
                            dictionary[key].Add(comparable);
                    }
                }
            }
            IComparable comparable1 = (IComparable)null;
            IComparable comparable2 = (IComparable)null;
            IComparable comparable3 = ValueHelper.ConvertValue((this.Series.YAxis == null || this.Series.YAxis.Scale == null ? (object)0.0 : this.Series.YAxis.Scale.ActualCrossingPosition) ?? (object)0.0, this.Series.ActualYValueType) as IComparable;
            list.Add(comparable3);
            foreach (object index2 in dictionary.Keys)
            {
                if (index2 == obj)
                {
                    IComparable comparable4 = (IComparable)null;
                    IComparable comparable5 = (IComparable)null;
                    foreach (IComparable comparable6 in dictionary[index2])
                    {
                        if (comparable6.CompareTo((object)comparable3) > 0)
                            comparable4 = comparable4 == null ? comparable6 : ValueHelper.AddComparableValues(comparable4, comparable6);
                        else
                            comparable5 = comparable5 == null ? comparable6 : ValueHelper.AddComparableValues(comparable5, comparable6);
                    }
                    if (comparable4 != null)
                        list.Add(comparable4);
                    if (comparable5 != null)
                        list.Add(comparable5);
                }
                else
                {
                    Range<IComparable> range = RangeEnumerableExtensions.GetRange<IComparable>((IEnumerable<IComparable>)dictionary[index2]);
                    if (range.HasData)
                    {
                        IComparable comparable4 = range.Minimum;
                        IComparable comparable5 = range.Maximum;
                        IComparable comparable6 = comparable3;
                        if (comparable6.GetType() != comparable4.GetType())
                            comparable6 = Convert.ChangeType((object)comparable6, comparable4.GetType(), (IFormatProvider)CultureInfo.InvariantCulture) as IComparable;
                        if (comparable4.CompareTo((object)comparable6) > 0)
                            comparable4 = comparable6;
                        if (comparable5.CompareTo((object)comparable6) < 0)
                            comparable5 = comparable6;
                        comparable1 = comparable1 == null ? comparable5 : ValueHelper.AddComparableValues(comparable1, comparable5);
                        comparable2 = comparable2 == null ? comparable4 : ValueHelper.AddComparableValues(comparable2, comparable4);
                        if (comparable1 != null)
                            list.Add(ValueHelper.ConvertValue((object)comparable1, this.Series.ActualYValueType) as IComparable);
                        if (comparable2 != null)
                            list.Add(ValueHelper.ConvertValue((object)comparable2, this.Series.ActualYValueType) as IComparable);
                    }
                }
            }
            return RangeEnumerableExtensions.GetRange<IComparable>((IEnumerable<IComparable>)list);
        }

        internal Range<IComparable> GetDataPointYValueRange(XYDataPoint p)
        {
            object index = ValueHelper.ConvertValue(p.XValue, this.Series.ActualXValueType);
            Range<IComparable> pointYvalueRange;
            if (!this.DataPointsByXValueRanges.TryGetValue(index, out pointYvalueRange))
            {
                pointYvalueRange = this.GetDataPointYValueRange(index, this.GetCounterpartDataSeries(true));
                this.DataPointsByXValueRanges[index] = pointYvalueRange;
            }
            return pointYvalueRange;
        }

        internal static Tuple<DataValueType, DataValueType, bool, object> GetSeriesKey(StackedColumnSeries series)
        {
            DataValueType dataValueType = series.ActualYValueType;
            if (dataValueType == DataValueType.Auto)
            {
                if (series.ChartArea != null)
                {
                    XYSeries xySeries = (XYSeries)Enumerable.FirstOrDefault<ColumnSeries>(Enumerable.OfType<ColumnSeries>((IEnumerable)series.ChartArea.GetSeries()), (Func<ColumnSeries, bool>)(s => s.ActualYValueType != DataValueType.Auto));
                    if (xySeries != null)
                        dataValueType = xySeries.ActualYValueType;
                }
                if (dataValueType == DataValueType.Auto && series.ActualYDataRange.HasData)
                {
                    dataValueType = ValueHelper.GetDataValueType((object)series.ActualYDataRange.Minimum);
                }
                else
                {
                    series.UpdateActualValueTypes();
                    dataValueType = series.ActualYValueType;
                }
            }
            if (dataValueType == DataValueType.Integer)
                dataValueType = DataValueType.Float;
            return new Tuple<DataValueType, DataValueType, bool, object>(dataValueType, series.ActualXValueType, series.IsHundredPercent, series.GroupingKey);
        }

        protected override void UpdateView()
        {
            this.InvalidateCounterpartSeriesAndDataPoints();
            base.UpdateView();
        }

        internal void InvalidateCounterpartSeriesAndDataPoints()
        {
            this._conterpartSeries = (List<XYSeries>)null;
            this._conterpartSeriesAll = (List<XYSeries>)null;
            this._counterpartDataPointsAll = (Dictionary<object, List<XYDataPoint>>)null;
        }

        internal List<XYSeries> GetCounterpartDataSeries(bool all)
        {
            if (all)
            {
                if (this._conterpartSeriesAll == null)
                    this._conterpartSeriesAll = new List<XYSeries>(this.GetCounterpartDataSeriesImpl(all));
                return this._conterpartSeriesAll;
            }
            if (this._conterpartSeries == null)
                this._conterpartSeries = new List<XYSeries>(this.GetCounterpartDataSeriesImpl(all));
            return this._conterpartSeries;
        }

        private IEnumerable<XYSeries> GetCounterpartDataSeriesImpl(bool all)
        {
            if (this.XYChartArea != null)
            {
                Dictionary<object, List<Series>> seriesClusters = this.GroupSeriesByClusters((IList<XYSeries>)Enumerable.ToArray<StackedColumnSeries>(Enumerable.Where<StackedColumnSeries>(Enumerable.OfType<StackedColumnSeries>((IEnumerable)this.XYChartArea.Series), (Func<StackedColumnSeries, bool>)(s => s.Visibility == Visibility.Visible))));
                KeyValuePair<object, List<Series>> currentPair = Enumerable.FirstOrDefault<KeyValuePair<object, List<Series>>>((IEnumerable<KeyValuePair<object, List<Series>>>)seriesClusters, (Func<KeyValuePair<object, List<Series>>, bool>)(pair => pair.Value.Contains((Series)this.Series)));
                HashSet<object> clusterGroupKeys = new HashSet<object>();
                if (currentPair.Value != null)
                {
                    foreach (StackedColumnSeries stackedColumnSeries in Enumerable.OfType<StackedColumnSeries>((IEnumerable)currentPair.Value))
                    {
                        if (all || stackedColumnSeries != this.Series)
                        {
                            if (stackedColumnSeries.ClusterGroupKey == null)
                                yield return (XYSeries)stackedColumnSeries;
                            else if (clusterGroupKeys.Add(stackedColumnSeries.ClusterGroupKey))
                                yield return (XYSeries)stackedColumnSeries;
                        }
                        else
                            break;
                    }
                }
            }
        }

        internal List<XYDataPoint> GetCounterpartDataPoints(object xValue, bool all = false)
        {
            return this.GetCounterpartDatapointsFromDictionary(ref this._counterpartDataPointsAll, xValue, all);
        }

        private List<XYDataPoint> GetCounterpartDatapointsFromDictionary(ref Dictionary<object, List<XYDataPoint>> dictionary, object xValue, bool all)
        {
            List<XYDataPoint> list = (List<XYDataPoint>)null;
            object key = xValue ?? (object)StackedColumnSeriesPresenter._nullKey;
            if (this._counterpartDataPointsAll == null)
                this._counterpartDataPointsAll = new Dictionary<object, List<XYDataPoint>>();
            if (!this._counterpartDataPointsAll.TryGetValue(key, out list))
            {
                list = Enumerable.ToList<XYDataPoint>(this.GetCounterpartDataPointsImpl(xValue, true));
                this._counterpartDataPointsAll.Add(key, list);
            }
            return list;
        }

        private IEnumerable<XYDataPoint> GetCounterpartDataPointsImpl(object xValue, bool all)
        {
            foreach (XYSeries xySeries in this.GetCounterpartDataSeries(all))
            {
                foreach (XYDataPoint xyDataPoint in Enumerable.OfType<XYDataPoint>((IEnumerable)xySeries.DataPointsByXValue[xValue]))
                    yield return xyDataPoint;
            }
        }

        internal override double GetYOffsetInAxisUnits(XYDataPoint dataPoint, Point valuePoint, Point basePoint)
        {
            object xValue = ValueHelper.ConvertValue(dataPoint.XValue, ((XYSeries)dataPoint.Series).ActualXValueType);
            bool flag = valuePoint.Y < basePoint.Y;
            double num1 = 0.0;
            double num2 = 0.0;
            object clusterGroupKey = ((ColumnSeries)this.Series).ClusterGroupKey;
            foreach (XYDataPoint dataPointXY in Enumerable.Where<XYDataPoint>((IEnumerable<XYDataPoint>)this.GetCounterpartDataPoints(xValue, false), (Func<XYDataPoint, bool>)(p => this.IsDataPointViewVisible((DataPoint)p))))
            {
                ColumnSeries columnSeries = dataPointXY.Series as ColumnSeries;
                if (clusterGroupKey != null && columnSeries != null)
                {
                    if (ValueHelper.AreEqual(columnSeries.ClusterGroupKey, clusterGroupKey))
                        break;
                }
                Point positionInAxisUnits = this.GetPositionInAxisUnits(dataPointXY);
                if (flag)
                {
                    if (positionInAxisUnits.Y < basePoint.Y)
                        num1 += basePoint.Y - positionInAxisUnits.Y;
                }
                else if (positionInAxisUnits.Y > basePoint.Y)
                    num2 += basePoint.Y - positionInAxisUnits.Y;
            }
            if (!flag)
                return num2;
            return num1;
        }

        internal override Point GetPositionInAxisUnits(XYDataPoint dataPoint)
        {
            double x = this.Series.XAxis.AxisPresenter.ConvertScaleToAxisUnits(dataPoint.XValueInScaleUnits) ?? 0.0;
            double num = this.TransformYValueInScaleUnits(dataPoint, dataPoint.YValueInScaleUnits);
            StackedColumnDataPoint stackedColumnDataPoint = dataPoint as StackedColumnDataPoint;
            if (stackedColumnDataPoint != null)
                stackedColumnDataPoint.YValuePercent = num;
            double y = this.Series.YAxis.AxisPresenter.ConvertScaleToAxisUnits(num) ?? 0.0;
            return new Point(x, y);
        }

        internal bool IsStackTopSeries()
        {
            List<XYSeries> counterpartDataSeries = this.GetCounterpartDataSeries(true);
            return counterpartDataSeries.Count > 0 && counterpartDataSeries[counterpartDataSeries.Count - 1] == this.Series;
        }

        internal override bool CanAdjustHeight()
        {
            return !this.IsHundredPercent() && this.GetCounterpartDataSeries(true).Count == 1;
        }

        protected override double AdjustColumnHeight(double height)
        {
            if (!this.IsHundredPercent() && this.GetCounterpartDataSeries(true).Count == 1)
                return Math.Round(height);
            return Math.Ceiling(height);
        }

        protected override void OnSeriesDataPointValueChanged(DataPoint dataPoint, string valueName, object oldValue, object newValue)
        {
            object obj = (object)null;
            bool flag = false;
            switch (valueName)
            {
                case "XValue":
                    obj = ValueHelper.ConvertValue(oldValue, this.Series.ActualXValueType);
                    flag = true;
                    break;
                case "YValue":
                    obj = ValueHelper.ConvertValue(((XYDataPoint)dataPoint).XValue, this.Series.ActualXValueType);
                    break;
                case "YValueInScaleUnits":
                    if (this.ChartArea != null && this.ChartArea.IsTemplateApplied)
                    {
                        obj = ValueHelper.ConvertValue(((XYDataPoint)dataPoint).XValue, this.Series.ActualXValueType);
                        if (!this._suppressCounterpartInvalidationFlag)
                        {
                            EnumerableFunctions.ForEach<XYDataPoint>(Enumerable.Where<XYDataPoint>((IEnumerable<XYDataPoint>)this.GetCounterpartDataPoints(obj, true), (Func<XYDataPoint, bool>)(p => p != dataPoint)), (Action<XYDataPoint>)(p => p.Update()));
                            break;
                        }
                        break;
                    }
                    break;
            }
            if (obj != null && this.ChartArea != null)
                this.DataPointsByXValueRanges.Remove(obj);
            if (flag && this.ChartArea != null && (!this.ChartArea.IsDirty && !this.ChartArea.UpdateSession.IsUpdating))
                this.ChartArea.Invalidate();
            base.OnSeriesDataPointValueChanged(dataPoint, valueName, oldValue, newValue);
        }

        protected override void OnSeriesModelPropertyChanged(string propertyName)
        {
            switch (propertyName)
            {
                case "ActualXValueType":
                case "ActualYValueType":
                case "ActualIsHundredPercent":
                case "GroupingKey":
                case "ClusterGroupKey":
                    if (this.ChartArea != null)
                    {
                        this.DataPointsByXValueRanges.Clear();
                        break;
                    }
                    break;
            }
            base.OnSeriesModelPropertyChanged(propertyName);
        }

        public override void OnSeriesRemoved()
        {
            if (this.ChartArea != null)
                this.DataPointsByXValueRanges.Clear();
            this.ReleaseDataPointsByXValueRanges();
            base.OnSeriesRemoved();
        }

        public override void OnSeriesAdded()
        {
            if (this.ChartArea != null)
                this.DataPointsByXValueRanges.Clear();
            base.OnSeriesAdded();
        }

        internal override void OnXScaleChanged()
        {
            this.SuppressCounterpartSeriesInvalidationAndExecute((Action)(() => base.OnXScaleChanged()));
        }

        internal override void OnYScaleChanged()
        {
            this.SuppressCounterpartSeriesInvalidationAndExecute((Action)(() =>
           {
               if (this.ChartArea != null)
                   this.DataPointsByXValueRanges.Clear();
               base.OnYScaleChanged();
           }));
        }

        internal override void OnMeasureIterationStarts()
        {
            base.OnMeasureIterationStarts();
            this.InvalidateCounterpartSeriesAndDataPoints();
        }

        private void SuppressCounterpartSeriesInvalidationAndExecute(Action action)
        {
            this._suppressCounterpartInvalidationFlag = true;
            try
            {
                action();
            }
            finally
            {
                this._suppressCounterpartInvalidationFlag = false;
            }
        }
    }
}
