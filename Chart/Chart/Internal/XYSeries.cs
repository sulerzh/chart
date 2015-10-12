﻿using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public abstract class XYSeries : Series
    {
        public static readonly DependencyProperty XValueTypeProperty = DependencyProperty.Register("XValueType", typeof(DataValueType), typeof(XYSeries), new PropertyMetadata((object)DataValueType.Auto, new PropertyChangedCallback(XYSeries.OnXValueTypeChanged)));
        public static readonly DependencyProperty YValueTypeProperty = DependencyProperty.Register("YValueType", typeof(DataValueType), typeof(XYSeries), new PropertyMetadata((object)DataValueType.Auto, new PropertyChangedCallback(XYSeries.OnYValueTypeChanged)));
        public static readonly DependencyProperty XDataRangeProperty = DependencyProperty.Register("XDataRange", typeof(Range<IComparable>?), typeof(XYSeries), new PropertyMetadata((object)null, new PropertyChangedCallback(XYSeries.OnXDataRangePropertyChanged)));
        public static readonly DependencyProperty YDataRangeProperty = DependencyProperty.Register("YDataRange", typeof(Range<IComparable>?), typeof(XYSeries), new PropertyMetadata((object)null, new PropertyChangedCallback(XYSeries.OnYDataRangePropertyChanged)));
        private DataPoint[] _dataPointsCollectionShadow = new DataPoint[0];
        private string _xAxisName = string.Empty;
        private string _yAxisName = string.Empty;
        internal const string XValueTypePropertyName = "XValueType";
        internal const string ActualXValueTypePropertyName = "ActualXValueType";
        internal const string YValueTypePropertyName = "YValueType";
        internal const string ActualYValueTypePropertyName = "ActualYValueType";
        internal const string XAxisNamePropertyName = "XAxisName";
        internal const string YAxisNamePropertyName = "YAxisName";
        internal const string XDataRangePropertyName = "XDataRange";
        internal const string YDataRangePropertyName = "YDataRange";
        internal const string ActualXDataRangePropertyName = "ActualXDataRange";
        internal const string ActualYDataRangePropertyName = "ActualYDataRange";
        internal const string XValuesPropertyName = "XValues";
        internal const string YValuesPropertyName = "YValues";
        private OrderedMultipleDictionary<object, DataPoint> _dataPointsByXValue;
        private Binding _xValueBinding;
        private DataValueType _actualXValueType;
        private DataValueType _actualYValueType;
        private Binding _yValueBinding;
        private Axis _xAxis;
        private Axis _yAxis;
        private Range<IComparable> _actualXDataRange;
        private Range<IComparable> _actualYDataRange;
        private IEnumerable<object> _xValues;
        private IEnumerable<object> _yValues;

        internal OrderedMultipleDictionary<object, DataPoint> DataPointsByXValue
        {
            get
            {
                if (this._dataPointsByXValue == null)
                    this.AddDataPointsByXValue((IEnumerable<DataPoint>)this.DataPoints);
                return this._dataPointsByXValue;
            }
        }

        public Binding XValueBinding
        {
            get
            {
                return this._xValueBinding;
            }
            set
            {
                if (value == this._xValueBinding)
                    return;
                this._xValueBinding = value;
                EnumerableFunctions.ForEachWithIndex<DataPoint>((IEnumerable<DataPoint>)this.DataPoints, (Action<DataPoint, int>)((item, index) => item.UpdateBinding()));
            }
        }

        public string XValuePath
        {
            get
            {
                if (this.XValueBinding == null)
                    return (string)null;
                return this.XValueBinding.Path.Path;
            }
            set
            {
                if (value == null)
                    this.XValueBinding = (Binding)null;
                else
                    this.XValueBinding = new Binding(value);
            }
        }

        public DataValueType XValueType
        {
            get
            {
                return (DataValueType)this.GetValue(XYSeries.XValueTypeProperty);
            }
            set
            {
                this.SetValue(XYSeries.XValueTypeProperty, (object)value);
            }
        }

        public DataValueType ActualXValueType
        {
            get
            {
                return this._actualXValueType;
            }
            protected set
            {
                if (value == this._actualXValueType)
                    return;
                this._actualXValueType = value;
                this.OnActualXValueTypeChanged(value);
            }
        }

        public DataValueType YValueType
        {
            get
            {
                return (DataValueType)this.GetValue(XYSeries.YValueTypeProperty);
            }
            set
            {
                this.SetValue(XYSeries.YValueTypeProperty, (object)value);
            }
        }

        public DataValueType ActualYValueType
        {
            get
            {
                return this._actualYValueType;
            }
            protected set
            {
                if (value == this._actualYValueType)
                    return;
                this._actualYValueType = value;
                this.YAggregator = this.CreateAggregator(value);
                this.OnPropertyChanged("ActualYValueType");
            }
        }

        public Binding YValueBinding
        {
            get
            {
                return this._yValueBinding;
            }
            set
            {
                if (value == this._yValueBinding)
                    return;
                this._yValueBinding = value;
                EnumerableFunctions.ForEachWithIndex<DataPoint>((IEnumerable<DataPoint>)this.DataPoints, (Action<DataPoint, int>)((item, index) => item.UpdateBinding()));
            }
        }

        public string YValuePath
        {
            get
            {
                if (this.YValueBinding == null)
                    return (string)null;
                return this.YValueBinding.Path.Path;
            }
            set
            {
                if (value == null)
                    this.YValueBinding = (Binding)null;
                else
                    this.YValueBinding = new Binding(value);
            }
        }

        public override IItemsBinder<DataPoint> ItemsBinder
        {
            get
            {
                return base.ItemsBinder;
            }
            set
            {
                base.ItemsBinder = value;
                this.ActualXValueType = DataValueType.Auto;
                this.ActualYValueType = DataValueType.Auto;
            }
        }

        internal override int ClusterKey
        {
            get
            {
                return new Tuple<string, Axis>("__DefaultAxisMargin__", this.XAxis).GetHashCode();
            }
        }

        public string XAxisName
        {
            get
            {
                return this._xAxisName;
            }
            set
            {
                if (!(this._xAxisName != value))
                    return;
                this._xAxisName = value;
                this._xAxis = (Axis)null;
                this.OnPropertyChanged("XAxisName");
            }
        }

        public Axis XAxis
        {
            get
            {
                if (this._xAxis == null)
                {
                    this._xAxis = ((XYChartArea)this.ChartArea).GetAxis(this._xAxisName, AxisOrientation.X);
                    if (this._xAxis.Orientation != AxisOrientation.X)
                        throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, Properties.Resources.AxisUncompatibleOrientation, new object[2]
                        {
              (object) this._xAxis.Name,
              (object) this.Name
                        }));
                }
                return this._xAxis;
            }
        }

        public string YAxisName
        {
            get
            {
                return this._yAxisName;
            }
            set
            {
                if (!(this._yAxisName != value))
                    return;
                this._yAxisName = value;
                this._yAxis = (Axis)null;
                this.OnPropertyChanged("YAxisName");
            }
        }

        public Axis YAxis
        {
            get
            {
                if (this._yAxis == null)
                {
                    this._yAxis = ((XYChartArea)this.ChartArea).GetAxis(this._yAxisName, AxisOrientation.Y);
                    if (this._yAxis.Orientation != AxisOrientation.Y)
                        throw new InvalidOperationException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, Properties.Resources.AxisUncompatibleOrientation, new object[2]
                        {
              (object) this._yAxis.Name,
              (object) this.Name
                        }));
                }
                return this._yAxis;
            }
        }

        public Range<IComparable>? XDataRange
        {
            get
            {
                return (Range<IComparable>?)this.GetValue(XYSeries.XDataRangeProperty);
            }
            set
            {
                this.SetValue(XYSeries.XDataRangeProperty, (object)value);
            }
        }

        public Range<IComparable>? YDataRange
        {
            get
            {
                return (Range<IComparable>?)this.GetValue(XYSeries.YDataRangeProperty);
            }
            set
            {
                this.SetValue(XYSeries.YDataRangeProperty, (object)value);
            }
        }

        public Range<IComparable> ActualXDataRange
        {
            get
            {
                return this._actualXDataRange;
            }
            private set
            {
                if (!(this._actualXDataRange != value))
                    return;
                this._actualXDataRange = value;
                this.OnActualXDataRangeChanged(value);
            }
        }

        public Range<IComparable> ActualYDataRange
        {
            get
            {
                return this._actualYDataRange;
            }
            private set
            {
                if (!(this._actualYDataRange != value))
                    return;
                this._actualYDataRange = value;
                this.OnActualYDataRangeChanged(value);
            }
        }

        public IEnumerable<object> XValues
        {
            get
            {
                if (this._xValues == null)
                    this._xValues = this.GetXValues();
                return this._xValues;
            }
            private set
            {
                if (this._xValues == value)
                    return;
                this._xValues = value;
                this.OnPropertyChanged("XValues");
            }
        }

        public IEnumerable<object> YValues
        {
            get
            {
                if (this._yValues == null)
                    this._yValues = this.GetYValues();
                return this._yValues;
            }
            private set
            {
                if (this._yValues == value)
                    return;
                this._yValues = value;
                this.OnPropertyChanged("YValues");
            }
        }

        internal ValueAggregator XAggregator { get; private set; }

        internal ValueAggregator YAggregator { get; private set; }

        public List<DataPoint> ActualDataPoints { get; protected set; }

        internal virtual ScaleDefaults XScaleDefaults
        {
            get
            {
                return new ScaleDefaults(AutoBool.False, 0.2);
            }
        }

        internal virtual ScaleDefaults YScaleDefaults
        {
            get
            {
                return new ScaleDefaults(AutoBool.Auto, 0.9);
            }
        }

        protected XYSeries()
        {
            this.ActualDataPoints = new List<DataPoint>();
            this.XAggregator = (ValueAggregator)new DefaultValueAggregator();
            this.YAggregator = (ValueAggregator)new DefaultValueAggregator();
        }

        private void InvalidateDataPointsByXValue()
        {
            this._dataPointsByXValue = (OrderedMultipleDictionary<object, DataPoint>)null;
        }

        private void AddDataPointsByXValue(IEnumerable<DataPoint> dataPoints)
        {
            if (this._dataPointsByXValue == null)
                this._dataPointsByXValue = new OrderedMultipleDictionary<object, DataPoint>(this.GetXValueKeyComparison());
            DataValueType actualXvalueType = this.ActualXValueType;
            foreach (XYDataPoint xyDataPoint in dataPoints)
                this._dataPointsByXValue.Add(ValueHelper.ConvertValue(xyDataPoint.XValue, actualXvalueType), (DataPoint)xyDataPoint);
        }

        private static void OnXValueTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((XYSeries)o).UpdateActualXValueType();
        }

        private void OnActualXValueTypeChanged(DataValueType newValue)
        {
            this.XAggregator = this.CreateAggregator(newValue);
            this.InvalidateDataPointsByXValue();
            this.OnPropertyChanged("ActualXValueType");
        }

        private void UpdateActualXValueType()
        {
            DataValueType dataValueType = this.XValueType;
            if (dataValueType == DataValueType.Auto)
            {
                dataValueType = ValueHelper.GetDataValueType(Enumerable.Select<XYDataPoint, object>(Enumerable.OfType<XYDataPoint>((IEnumerable)this.DataPoints), (Func<XYDataPoint, object>)(p => p.XValue)));
                if (dataValueType == DataValueType.Auto && this.XDataRange.HasValue)
                    dataValueType = ValueHelper.GetDataValueType(this.XDataRange.Value);
            }
            this.ActualXValueType = dataValueType;
        }

        private void UpdateActualXValueType(IEnumerable<DataPoint> dataPoints)
        {
            if (this.ActualXValueType != DataValueType.Auto)
                return;
            this.ActualXValueType = ValueHelper.GetDataValueType(Enumerable.Select<XYDataPoint, object>(Enumerable.OfType<XYDataPoint>((IEnumerable)dataPoints), (Func<XYDataPoint, object>)(p => p.XValue)));
        }

        internal virtual ValueAggregator CreateAggregator(DataValueType valueType)
        {
            return ValueAggregator.GetAggregator(valueType);
        }

        private static void OnYValueTypeChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((XYSeries)o).UpdateActualYValueType();
        }

        protected void UpdateActualYValueType()
        {
            DataValueType dataValueType = this.YValueType;
            if (dataValueType == DataValueType.Auto)
            {
                dataValueType = this.GetYValueType((IEnumerable<DataPoint>)this.DataPoints);
                if (dataValueType == DataValueType.Auto && this.YDataRange.HasValue)
                    dataValueType = ValueHelper.GetDataValueType(this.YDataRange.Value);
            }
            this.ActualYValueType = dataValueType;
        }

        private void UpdateActualYValueType(IEnumerable<DataPoint> dataPoints)
        {
            if (this.ActualYValueType != DataValueType.Auto)
                return;
            this.ActualYValueType = this.GetYValueType(dataPoints);
        }

        protected virtual DataValueType GetYValueType(IEnumerable<DataPoint> dataPoints)
        {
            foreach (XYDataPoint xyDataPoint in dataPoints)
            {
                if (xyDataPoint.YValue != null)
                {
                    DataValueType dataValueType = ValueHelper.GetDataValueType(xyDataPoint.YValue);
                    if (dataValueType != DataValueType.Auto)
                        return dataValueType;
                }
            }
            return DataValueType.Auto;
        }

        public void UpdateActualValueTypes()
        {
            this.UpdateActualXValueType();
            this.UpdateActualYValueType();
        }

        private static void OnXDataRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as XYSeries).OnXDataRangeChanged(e.OldValue, e.NewValue);
        }

        private void OnXDataRangeChanged(object oldValue, object newValue)
        {
            this.UpdateActualXDataRange();
            this.UpdateActualXValueType();
        }

        private static void OnYDataRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as XYSeries).OnYDataRangeChanged(e.OldValue, e.NewValue);
        }

        private void OnYDataRangeChanged(object oldValue, object newValue)
        {
            this.UpdateActualYDataRange();
            this.UpdateActualYValueType();
        }

        protected virtual void OnActualXDataRangeChanged(Range<IComparable> newValue)
        {
            this.OnPropertyChanged("ActualXDataRange");
        }

        protected virtual void OnActualYDataRangeChanged(Range<IComparable> newValue)
        {
            this.OnPropertyChanged("ActualYDataRange");
        }

        private IEnumerable<object> GetXValues()
        {
            return Enumerable.Select<XYDataPoint, object>(Enumerable.OfType<XYDataPoint>((IEnumerable)this.DataPoints), (Func<XYDataPoint, object>)(item => ValueHelper.ConvertValue(item.XValue, this.XValueType)));
        }

        private IEnumerable<object> GetYValues()
        {
            return Enumerable.Distinct<object>(Enumerable.Select<XYDataPoint, object>(Enumerable.OfType<XYDataPoint>((IEnumerable)this.DataPoints), (Func<XYDataPoint, object>)(item => ValueHelper.ConvertValue(item.YValue, this.YValueType))));
        }

        internal override void OnDataPointsCollectionChanged(IEnumerable<DataPoint> removedDataPoints, IEnumerable<DataPoint> addedDataPoints, bool isResetting)
        {
            if (removedDataPoints != null)
            {
                if (this.DataPoints.Count == 0)
                {
                    this.DataPointsByXValue.Clear();
                }
                else
                {
                    foreach (DataPoint dataPoint in removedDataPoints)
                        this.DataPointsByXValue.Remove(dataPoint);
                }
            }
            if (isResetting)
            {
                removedDataPoints = (IEnumerable<DataPoint>)this._dataPointsCollectionShadow;
                this._dataPointsByXValue = (OrderedMultipleDictionary<object, DataPoint>)null;
                this.ResetActualValueType();
                this.InvalidateDataPointsByXValue();
                addedDataPoints = (IEnumerable<DataPoint>)this.DataPoints;
            }
            if (addedDataPoints != null && Enumerable.Any<DataPoint>(addedDataPoints))
            {
                foreach (DataPoint dataPoint in addedDataPoints)
                    this.InitializeDataPoint(dataPoint);
                DataValueType actualXvalueType = this.ActualXValueType;
                this.UpdateActualXValueType(addedDataPoints);
                if (this.ActualXValueType == actualXvalueType)
                    this.AddDataPointsByXValue(addedDataPoints);
                this.UpdateActualYValueType(addedDataPoints);
            }
            this.UpdateActualDataPoints();
            this.UpdateActualDataRange();
            if (this.ChartArea != null && this.ChartArea.IsTemplateApplied)
            {
                if (!this.SeriesPresenter.SeriesCollectionChanging)
                {
                    this.ChartArea.UpdateSession.BeginUpdates();
                    bool useAnimation = this.ChartArea.IsShowingAnimationEnabled && (this.XAxis.Scale == null || !this.XAxis.Scale.IsScrolling) && (this.YAxis.Scale == null || !this.YAxis.Scale.IsScrolling);
                    if (removedDataPoints != null)
                        EnumerableFunctions.ForEach<DataPoint>(removedDataPoints, (Action<DataPoint>)(item => this.SeriesPresenter.OnDataPointRemoved(item, useAnimation)));
                    if (addedDataPoints != null)
                        EnumerableFunctions.ForEach<DataPoint>(addedDataPoints, (Action<DataPoint>)(item => this.SeriesPresenter.OnDataPointAdded(item, useAnimation)));
                    this.ChartArea.UpdateSession.EndUpdates();
                }
            }
            else if (removedDataPoints != null)
                EnumerableFunctions.ForEach<DataPoint>(removedDataPoints, (Action<DataPoint>)(item => this.UninitializeDataPoint(item)));
            this._dataPointsCollectionShadow = Enumerable.ToArray<DataPoint>((IEnumerable<DataPoint>)this.DataPoints);
        }

        public void UpdateActualDataPoints()
        {
            this.ActualDataPoints = Enumerable.ToList<DataPoint>(Enumerable.Where<DataPoint>((IEnumerable<DataPoint>)this.DataPoints, (Func<DataPoint, bool>)(p => this.CanPlot(p))));
        }

        public virtual bool CanPlot(DataPoint dataPoint)
        {
            XYDataPoint xyDataPoint = dataPoint as XYDataPoint;
            if (this.ActualXValueType == DataValueType.Category)
            {
                if (xyDataPoint != null)
                    return this.XAggregator.CanPlot(xyDataPoint.XValue);
                return false;
            }
            if (this.ActualYValueType == DataValueType.Category)
            {
                if (xyDataPoint != null)
                    return this.YAggregator.CanPlot(xyDataPoint.XValue);
                return false;
            }
            if (xyDataPoint != null && this.XAggregator.CanPlot(xyDataPoint.XValue))
                return this.YAggregator.CanPlot(xyDataPoint.YValue);
            return false;
        }

        internal override void UpdateActualDataRange()
        {
            this.UpdateActualXDataRange();
            this.UpdateActualYDataRange();
        }

        internal void UpdateActualXDataRange()
        {
            this.ActualXDataRange = !this.XDataRange.HasValue || !this.XDataRange.Value.HasData ? this.GetActualXDataRange() : this.XDataRange.Value;
        }

        internal void UpdateActualYDataRange()
        {
            this.ActualYDataRange = !this.YDataRange.HasValue || !this.YDataRange.Value.HasData ? this.GetActualYDataRange() : this.YDataRange.Value;
        }

        protected virtual Range<IComparable> GetActualXDataRange()
        {
            IEnumerable<object> xvalues = XYSeries.GetXValues((IEnumerable<DataPoint>)this.ActualDataPoints);
            if (this.ActualXValueType == DataValueType.Category)
                this.XValues = Enumerable.Distinct<object>(xvalues);
            return this.XAggregator.GetRange(xvalues);
        }

        protected virtual Range<IComparable> GetActualYDataRange()
        {
            IEnumerable<object> yvalues = XYSeries.GetYValues((IEnumerable<DataPoint>)this.ActualDataPoints);
            if (this.ActualYValueType == DataValueType.Category)
                this.YValues = Enumerable.Distinct<object>(yvalues);
            return this.YAggregator.GetRange(yvalues);
        }

        private static IEnumerable<object> GetXValues(IEnumerable<DataPoint> dataPoints)
        {
            foreach (XYDataPoint xyDataPoint in dataPoints)
                yield return xyDataPoint.XValue;
        }

        private static IEnumerable<object> GetYValues(IEnumerable<DataPoint> dataPoints)
        {
            foreach (XYDataPoint xyDataPoint in dataPoints)
                yield return xyDataPoint.YValue;
        }

        internal override void RemoveAllDataPoints()
        {
            IEnumerable<DataPoint> that = (IEnumerable<DataPoint>)this._dataPointsCollectionShadow;
            this._dataPointsByXValue = (OrderedMultipleDictionary<object, DataPoint>)null;
            if (that == null)
                return;
            if (this.ChartArea != null && this.ChartArea.IsTemplateApplied)
                EnumerableFunctions.ForEach<DataPoint>(that, (Action<DataPoint>)(item => item.ViewState = DataPointViewState.Hiding));
            else
                EnumerableFunctions.ForEach<DataPoint>(that, (Action<DataPoint>)(item => this.UninitializeDataPoint(item)));
        }

        internal override void OnDataPointValueChanged(DataPoint dataPoint, string propertyName, object oldValue, object newValue)
        {
            base.OnDataPointValueChanged(dataPoint, propertyName, oldValue, newValue);
            if (!(dataPoint is XYDataPoint))
                return;
            switch (propertyName)
            {
                case "XValue":
                    this.DataPointsByXValue.Remove(dataPoint);
                    this.DataPointsByXValue.Add(ValueHelper.ConvertValue(newValue, this.ActualXValueType), dataPoint);
                    this.UpdateActualXDataRange();
                    break;
                case "YValue":
                    this.UpdateActualYDataRange();
                    break;
            }
        }

        internal virtual void OnDataPointProjectedYValueChanged(DataPoint dataPoint, object oldValue, object newValue)
        {
            if (this.ChartArea == null)
                return;
            this.ChartArea.UpdateSession.Update((IUpdatable)dataPoint);
        }

        protected override bool IsDataPointCompatible(DataPoint dataPoint)
        {
            return dataPoint is XYDataPoint;
        }

        internal void ResetActualValueType()
        {
            this.ActualXValueType = DataValueType.Auto;
            this.ActualYValueType = DataValueType.Auto;
        }

        internal Comparison<object> GetXValueKeyComparison()
        {
            return (Comparison<object>)((left, right) =>
           {
               if (left == null && right == null)
                   return 0;
               if (right == null)
                   return 1;
               if (left == null)
                   return -1;
               if (left.Equals(right))
                   return 0;
               IComparable left1 = left as IComparable;
               IComparable right1 = right as IComparable;
               if (left1 != null || right1 != null)
                   return ValueHelper.Compare(left1, right1);
               return RuntimeHelpers.GetHashCode(right).CompareTo(RuntimeHelpers.GetHashCode(left));
           });
        }

        internal void LoadDataFromVirtualizer(IEnumerable data)
        {
            object firstNewDataObject = (object)null;
            int num = -1;
            if (!EnumerableFunctions.IsEmpty(data))
            {
                firstNewDataObject = EnumerableFunctions.FastElementAt<object>(data, 0);
                if (firstNewDataObject != null)
                    num = EnumerableFunctions.FindIndexOf<DataPoint>((IEnumerable<DataPoint>)this.DataPoints, (Func<DataPoint, bool>)(item =>
                  {
                      if (item != firstNewDataObject)
                          return item.DataContext == firstNewDataObject;
                      return true;
                  }));
            }
            if (num < 0)
            {
                this.ItemsSource = data;
            }
            else
            {
                for (; num > 0; --num)
                    this.DataPoints.RemoveAt(0);
                int index = 0;
                foreach (object obj in data)
                {
                    if (index >= 0 && index < this.DataPoints.Count && (this.DataPoints[index] == obj || this.DataPoints[index].DataContext == obj))
                    {
                        ++index;
                    }
                    else
                    {
                        if (index >= 0)
                        {
                            while (index < this.DataPoints.Count)
                                this.DataPoints.RemoveAt(index);
                            index = -1;
                        }
                        DataPoint dataPoint = obj as DataPoint;
                        if (dataPoint == null || !this.IsDataPointCompatible(dataPoint))
                        {
                            dataPoint = this.CreateDataPoint();
                            dataPoint.DataContext = obj;
                            if (this.ItemsBinder != null)
                                this.ItemsBinder.Bind(dataPoint, dataPoint.DataContext);
                        }
                        this.DataPoints.Add(dataPoint);
                    }
                }
            }
        }
    }
}
