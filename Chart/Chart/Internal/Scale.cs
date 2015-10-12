using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public abstract class Scale : FrameworkElement, INotifyPropertyChanged
    {
        internal static readonly Range<double> PercentRange = new Range<double>(0.0, 1.0);
        internal static readonly Range<double> DefaultZoomRange = new Range<double>(1.0, 100.0);
        internal static readonly Range<double> MaxZoomRange = new Range<double>(1.0, 1000000000.0);
        public static readonly DependencyProperty ZoomRangeProperty = DependencyProperty.Register("ZoomRange", typeof(Range<double>?), typeof(Scale), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale.OnZoomRangeChanged)));
        private Range<double> _actualZoomRange = Scale.DefaultZoomRange;
        internal const string DefaultFormatString = "{0}";
        internal const double PercentPrecision = 1E-11;
        internal const double ViewPrecision = 0.01;
        private const string ZoomRangePropertyName = "ZoomRange";
        private int _initCount;
        private int _invalidateCount;
        private int _invalidateViewCount;
        private DataValueType _valueType;
        private ScaleDefaults _defaults;
        private LabelDefinition _labelDefinition;
        private TickmarkDefinition _majorTickmarkDefinition;
        private TickmarkDefinition _minorTickmarkDefinition;
        private GridlineDefinition _majorGridLineDefinition;
        private GridlineDefinition _minorGridLineDefinition;
        private static Dictionary<DataValueType, Scale.ScaleFactory> _factoryRegistry;

        internal bool IsInitializing
        {
            get
            {
                return this._initCount > 0;
            }
        }

        internal bool IsInvalidated
        {
            get
            {
                return this._invalidateCount > 0;
            }
        }

        internal bool IsScrolling { get; set; }

        internal bool IsZooming { get; set; }

        internal DataValueType ValueType
        {
            get
            {
                return this._valueType;
            }
            set
            {
                if (this._valueType == value)
                    return;
                this._valueType = value;
            }
        }

        internal ScaleDefaults Defaults
        {
            get
            {
                return this._defaults;
            }
            set
            {
                this._defaults = value;
            }
        }

        public int MaxCount { get; set; }

        public LabelDefinition LabelDefinition
        {
            get
            {
                return this._labelDefinition;
            }
            set
            {
                this.SetElement<LabelDefinition>(ref this._labelDefinition, value);
            }
        }

        public Range<double>? ZoomRange
        {
            get
            {
                return (Range<double>?)this.GetValue(Scale.ZoomRangeProperty);
            }
            set
            {
                this.SetValue(Scale.ZoomRangeProperty, (object)value);
            }
        }

        public Range<double> ActualZoomRange
        {
            get
            {
                return this._actualZoomRange;
            }
            set
            {
                if (!(this._actualZoomRange != value))
                    return;
                this._actualZoomRange = value;
                this.InvalidateView();
            }
        }

        public abstract double ActualZoom { get; }

        public TickmarkDefinition MajorTickmarkDefinition
        {
            get
            {
                return this._majorTickmarkDefinition;
            }
            set
            {
                this.SetElement<TickmarkDefinition>(ref this._majorTickmarkDefinition, value);
            }
        }

        public TickmarkDefinition MinorTickmarkDefinition
        {
            get
            {
                return this._minorTickmarkDefinition;
            }
            set
            {
                this.SetElement<TickmarkDefinition>(ref this._minorTickmarkDefinition, value);
            }
        }

        public GridlineDefinition MajorGridLineDefinition
        {
            get
            {
                return this._majorGridLineDefinition;
            }
            set
            {
                this.SetElement<GridlineDefinition>(ref this._majorGridLineDefinition, value);
            }
        }

        public GridlineDefinition MinorGridLineDefinition
        {
            get
            {
                return this._minorGridLineDefinition;
            }
            set
            {
                this.SetElement<GridlineDefinition>(ref this._minorGridLineDefinition, value);
            }
        }

        public IList<ScaleElementDefinition> CustomElementDefinitions { get; private set; }

        public object ActualCrossingPosition { get; internal set; }

        public bool HasCustomCrossingPosition { get; internal set; }

        public abstract double ProjectedStartMargin { get; }

        public abstract double ProjectedEndMargin { get; }

        internal virtual object SampleLabelContent
        {
            get
            {
                return this.LabelDefinition.SampleContent;
            }
        }

        internal abstract object DefaultLabelContent { get; }

        public virtual int PreferredMaxCount
        {
            get
            {
                return 10;
            }
        }

        internal abstract int ActualMajorCount { get; }

        public abstract bool IsEmpty { get; }

        private static Dictionary<DataValueType, Scale.ScaleFactory> FactoryRegistry
        {
            get
            {
                if (Scale._factoryRegistry == null)
                {
                    Scale._factoryRegistry = new Dictionary<DataValueType, Scale.ScaleFactory>();
                    Scale.ScaleFactory scaleFactory1 = (Scale.ScaleFactory)new CategoryScale.CategoryScaleFactory();
                    Scale.ScaleFactory scaleFactory2 = (Scale.ScaleFactory)new NumericScale.NumericScaleFactory();
                    Scale.ScaleFactory scaleFactory3 = (Scale.ScaleFactory)new DateTimeScale.DateTimeScaleFactory();
                    Scale._factoryRegistry.Add(DataValueType.Category, scaleFactory1);
                    Scale._factoryRegistry.Add(DataValueType.Date, scaleFactory3);
                    Scale._factoryRegistry.Add(DataValueType.DateTime, scaleFactory3);
                    Scale._factoryRegistry.Add(DataValueType.DateTimeOffset, scaleFactory3);
                    Scale._factoryRegistry.Add(DataValueType.Float, scaleFactory2);
                    Scale._factoryRegistry.Add(DataValueType.Integer, scaleFactory2);
                    Scale._factoryRegistry.Add(DataValueType.Time, scaleFactory3);
                    Scale._factoryRegistry.Add(DataValueType.TimeSpan, scaleFactory3);
                }
                return Scale._factoryRegistry;
            }
        }

        public event EventHandler Updated;

        public event EventHandler ElementChanged;

        public event EventHandler<ScaleViewChangedArgs> ViewChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        protected Scale()
        {
            ++this._initCount;
            this.IsScrolling = false;
            this.IsZooming = false;
            this.MaxCount = 10;
            LabelDefinition labelDefinition = new LabelDefinition();
            labelDefinition.Level = 0;
            labelDefinition.Group = ScaleElementGroup.Major;
            labelDefinition.Format = "{0}";
            this.LabelDefinition = labelDefinition;
            TickmarkDefinition tickmarkDefinition1 = new TickmarkDefinition();
            tickmarkDefinition1.Level = 0;
            tickmarkDefinition1.Group = ScaleElementGroup.Major;
            this.MajorTickmarkDefinition = tickmarkDefinition1;
            TickmarkDefinition tickmarkDefinition2 = new TickmarkDefinition();
            tickmarkDefinition2.Level = 0;
            tickmarkDefinition2.Group = ScaleElementGroup.Minor;
            tickmarkDefinition2.Visibility = Visibility.Collapsed;
            this.MinorTickmarkDefinition = tickmarkDefinition2;
            --this._initCount;
        }

        private static void OnZoomRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale)d).OnZoomRangeChanged((Range<double>?)e.OldValue, (Range<double>?)e.NewValue);
        }

        protected virtual void OnZoomRangeChanged(Range<double>? oldValue, Range<double>? newValue)
        {
            this.CalculateActualZoomRange();
        }

        internal void SetElement<T>(ref T element, T newValue) where T : ScaleElementDefinition
        {
            if ((object)element == (object)newValue)
                return;
            this.ResetScale((ScaleElementDefinition)element);
            element = newValue;
            this.SetScale((ScaleElementDefinition)element);
            this.OnElementChanged((ScaleElementDefinition)element);
        }

        internal void ResetScale(ScaleElementDefinition element)
        {
            if (element == null)
                return;
            element.Scale = (Scale)null;
            element.PropertyChanged -= new PropertyChangedEventHandler(this.ScaleElement_PropertyChanged);
        }

        internal void SetScale(ScaleElementDefinition element)
        {
            if (element == null)
                return;
            element.Scale = this;
            element.PropertyChanged += new PropertyChangedEventHandler(this.ScaleElement_PropertyChanged);
        }

        public abstract bool CanProject(DataValueType valueType);

        public abstract double ProjectDataValue(object value);

        public abstract IEnumerable<double> ProjectValues(IEnumerable values);

        public abstract IEnumerable<ScaleElementDefinition> ProjectElements();

        public virtual double ProjectClusterSize(IEnumerable values)
        {
            double val2 = 0.0;
            double num = this.ProjectMajorIntervalSize();
            List<double> list = Enumerable.ToList<double>(this.ProjectValues(values));
            if (list.Count > 1)
            {
                list.Sort();
                val2 = MathHelper.GetMinimumDelta((IList<double>)list);
            }
            if (val2 == 0.0 || val2 > num)
                val2 = num;
            return Math.Min(0.5, val2);
        }

        public virtual double ProjectMajorIntervalSize()
        {
            double num = double.MaxValue;
            foreach (ScalePosition scalePosition in this.ProjectMajorIntervals())
            {
                double position = scalePosition.Position;
                if (num != double.MaxValue)
                    return position - num;
                num = position;
            }
            return 0.0;
        }

        public abstract IEnumerable<ScalePosition> ProjectMajorIntervals();

        public abstract void Recalculate();

        internal abstract void RecalculateIfEmpty();

        public void Invalidate()
        {
            ++this._invalidateCount;
            this.Update();
        }

        internal void InvalidateView()
        {
            ++this._invalidateViewCount;
        }

        protected virtual void ResetView()
        {
            this.IsScrolling = false;
            this.IsZooming = false;
        }

        protected virtual bool NeedsRecalculation()
        {
            return !this.IsScrolling;
        }

        public virtual void Update()
        {
            if (this.IsInitializing || !this.IsInvalidated)
                return;
            ++this._initCount;
            DateTime now = DateTime.Now;
            if (this.NeedsRecalculation())
                this.Recalculate();
            this.OnUpdated();
            if (this._invalidateViewCount > 0)
                this.OnViewChanged();
            string name = this.GetType().Name;
            this._invalidateCount = 0;
            this._invalidateViewCount = 0;
            this._initCount = 0;
        }

        protected virtual void OnUpdated()
        {
            EventHandler eventHandler = this.Updated;
            if (eventHandler == null)
                return;
            eventHandler((object)this, EventArgs.Empty);
        }

        protected virtual void OnElementChanged(ScaleElementDefinition element)
        {
            EventHandler eventHandler = this.ElementChanged;
            if (eventHandler == null)
                return;
            eventHandler((object)element, EventArgs.Empty);
        }

        protected virtual void OnViewChanged(Range<IComparable> oldRange, Range<IComparable> newRange)
        {
            EventHandler<ScaleViewChangedArgs> eventHandler = this.ViewChanged;
            if (eventHandler == null)
                return;
            eventHandler((object)this, new ScaleViewChangedArgs()
            {
                OldRange = oldRange,
                NewRange = newRange
            });
        }

        protected abstract void OnViewChanged();

        public abstract void UpdateRange(IEnumerable<Range<IComparable>> ranges);

        internal abstract void UpdateRangeIfUndefined(IEnumerable<Range<IComparable>> ranges);

        internal virtual void UpdateValuesIfUndefined(IEnumerable<object> values)
        {
        }

        public virtual void UpdateValueType(DataValueType valueType)
        {
            if (!this.CanProject(valueType))
                throw new ArgumentException(Properties.Resources.Scale_DataValueTypeOutOfRange);
            if (this.ValueType == valueType)
                return;
            this.ValueType = valueType;
            this.Invalidate();
        }

        public void UpdateDefaults(ScaleDefaults defaults)
        {
            if (this.Defaults.Equals((object)defaults))
                return;
            this.Defaults = defaults;
            this.Invalidate();
        }

        public virtual bool TryChangeInterval(double ratio)
        {
            int actualMajorCount1 = this.ActualMajorCount;
            int num = Math.Max((int)Math.Round((double)this.MaxCount / ratio), 1);
            if (num == this.MaxCount)
                return false;
            this.MaxCount = num;
            this.Recalculate();
            int actualMajorCount2 = this.ActualMajorCount;
            if (actualMajorCount1 != 0 && actualMajorCount2 != actualMajorCount1)
                this.Invalidate();
            return true;
        }

        public abstract double ConvertToPercent(object value);

        public abstract Range<double> ConvertActualViewToPercent();

        public abstract void ScrollToPercent(double position);

        public abstract void ScrollBy(double offset);

        public abstract void ZoomToPercent(double viewMinimum, double viewMaximum);

        public void ZoomToPercent(double viewSize)
        {
            Range<double> range = this.ConvertActualViewToPercent();
            double num = Math.Min(viewSize, 1.0);
            double viewMinimum = Math.Min(range.Minimum, 1.0 - viewSize);
            double viewMaximum = viewMinimum + num;
            this.ZoomToPercent(viewMinimum, viewMaximum);
        }

        public abstract void ZoomBy(double centerValue, double delta);

        internal virtual void CalculateActualZoomRange()
        {
            Range<double> range = this.ZoomRange.HasValue ? this.ZoomRange.Value : Scale.DefaultZoomRange;
            double minimum = range.Minimum;
            double maximum = range.Maximum;
            double minPossibleZoom = this.GetMinPossibleZoom();
            double maxPossibleZoom = this.GetMaxPossibleZoom();
            RangeHelper.BoxRangeInsideAnother(ref minimum, ref maximum, minPossibleZoom, maxPossibleZoom);
            this.ActualZoomRange = new Range<double>(minimum, maximum);
        }

        protected virtual double GetMinPossibleZoom()
        {
            return 1.0;
        }

        protected virtual double GetMaxPossibleZoom()
        {
            return 1000000000.0;
        }

        public virtual bool TryChangeMaxCount(double maxMajorCount)
        {
            return this.TryChangeInterval((double)this.MaxCount / maxMajorCount);
        }

        private void ScaleElement_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnElementChanged(sender as ScaleElementDefinition);
        }

        public static Scale CreateScaleByType(DataValueType valueType)
        {
            return Scale.FactoryRegistry[valueType].Create(valueType);
        }

        public new void BeginInit()
        {
            ++this._initCount;
        }

        public new void EndInit()
        {
            if (this._initCount <= 0)
                return;
            --this._initCount;
            if (this._initCount != 0)
                return;
            this.Update();
        }

        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(name));
        }

        internal abstract class ScaleFactory
        {
            public abstract Scale Create(DataValueType valueType);
        }
    }
}
