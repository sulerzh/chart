using Semantic.Reporting.Windows.Chart.Internal.Properties;
using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public abstract class Scale<TPosition, TInterval, TScaleUnit> : Scale where TPosition : struct, IComparable, IComparable<TPosition> where TInterval : struct where TScaleUnit : struct
    {
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(TPosition?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMaximumPropertyChanged)));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(TPosition?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMinimumPropertyChanged)));
        public static readonly DependencyProperty ViewMaximumProperty = DependencyProperty.Register("ViewMaximum", typeof(TPosition?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnViewMaximumPropertyChanged)));
        public static readonly DependencyProperty ViewMinimumProperty = DependencyProperty.Register("ViewMinimum", typeof(TPosition?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnViewMinimumPropertyChanged)));
        public static readonly DependencyProperty MajorIntervalProperty = DependencyProperty.Register("MajorInterval", typeof(TInterval?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMajorIntervalPropertyChanged)));
        public static readonly DependencyProperty MajorIntervalUnitProperty = DependencyProperty.Register("MajorIntervalUnit", typeof(TScaleUnit?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMajorIntervalUnitPropertyChanged)));
        public static readonly DependencyProperty MajorIntervalOffsetProperty = DependencyProperty.Register("MajorIntervalOffset", typeof(TInterval?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMajorIntervalOffsetPropertyChanged)));
        public static readonly DependencyProperty MinorIntervalProperty = DependencyProperty.Register("MinorInterval", typeof(TInterval?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMinorIntervalPropertyChanged)));
        public static readonly DependencyProperty MinorIntervalUnitProperty = DependencyProperty.Register("MinorIntervalUnit", typeof(TScaleUnit?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMinorIntervalUnitPropertyChanged)));
        public static readonly DependencyProperty MinorIntervalOffsetProperty = DependencyProperty.Register("MinorIntervalOffset", typeof(TInterval?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMinorIntervalOffsetPropertyChanged)));
        public static readonly DependencyProperty CrossingPositionProperty = DependencyProperty.Register("CrossingPosition", typeof(TPosition?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnCrossingPositionPropertyChanged)));
        public static readonly DependencyProperty CrossingPositionModeProperty = DependencyProperty.Register("CrossingPositionMode", typeof(AxisCrossingPositionMode), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)AxisCrossingPositionMode.Auto, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnCrossingPositionModePropertyChanged)));
        public static readonly DependencyProperty DataRangeProperty = DependencyProperty.Register("DataRange", typeof(Range<TPosition>?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnDataRangeChanged)));
        public static readonly DependencyProperty IntervalAttachedProperty = DependencyProperty.RegisterAttached("Interval", typeof(TInterval?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnIntervalAttachedPropertyChanged)));
        public static readonly DependencyProperty IntervalOffsetAttachedProperty = DependencyProperty.RegisterAttached("IntervalOffset", typeof(TInterval?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnIntervalOffsetAttachedPropertyChanged)));
        public static readonly DependencyProperty IntervalUnitAttachedProperty = DependencyProperty.RegisterAttached("IntervalUnit", typeof(TScaleUnit?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnIntervalUnitAttachedPropertyChanged)));
        public static readonly DependencyProperty MaxCountAttachedProperty = DependencyProperty.RegisterAttached("MaxCount", typeof(int?), typeof(Scale<TPosition, int, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnMaxCountAttachedPropertyChanged)));
        public static readonly DependencyProperty ActualMinimumProperty = DependencyProperty.Register("ActualMinimum", typeof(TPosition), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata(new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnActualMinimumPropertyChanged)));
        public static readonly DependencyProperty ActualMaximumProperty = DependencyProperty.Register("ActualMaximum", typeof(TPosition), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata(new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnActualMaximumPropertyChanged)));
        public static readonly DependencyProperty ActualViewMinimumProperty = DependencyProperty.Register("ActualViewMinimum", typeof(TPosition), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata(new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnActualViewMinimumPropertyChanged)));
        public static readonly DependencyProperty ActualViewMaximumProperty = DependencyProperty.Register("ActualViewMaximum", typeof(TPosition), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata(new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnActualViewMaximumPropertyChanged)));
        public static readonly DependencyProperty ActualMajorIntervalProperty = DependencyProperty.Register("ActualMajorInterval", typeof(TInterval), typeof(Scale<TPosition, TInterval, TScaleUnit>), (PropertyMetadata)null);
        public static readonly DependencyProperty ActualMajorIntervalUnitProperty = DependencyProperty.Register("ActualMajorIntervalUnit", typeof(TScaleUnit), typeof(Scale<TPosition, TInterval, TScaleUnit>), (PropertyMetadata)null);
        public static readonly DependencyProperty ActualMajorIntervalOffsetProperty = DependencyProperty.Register("ActualMajorIntervalOffset", typeof(TInterval), typeof(Scale<TPosition, TInterval, TScaleUnit>), (PropertyMetadata)null);
        public static readonly DependencyProperty ActualMinorIntervalProperty = DependencyProperty.Register("ActualMinorInterval", typeof(TInterval), typeof(Scale<TPosition, TInterval, TScaleUnit>), (PropertyMetadata)null);
        public static readonly DependencyProperty ActualMinorIntervalUnitProperty = DependencyProperty.Register("ActualMinorIntervalUnit", typeof(TScaleUnit), typeof(Scale<TPosition, TInterval, TScaleUnit>), (PropertyMetadata)null);
        public static readonly DependencyProperty ActualMinorIntervalOffsetProperty = DependencyProperty.Register("ActualMinorIntervalOffset", typeof(TInterval), typeof(Scale<TPosition, TInterval, TScaleUnit>), (PropertyMetadata)null);
        public static readonly DependencyProperty ActualDataRangeProperty = DependencyProperty.Register("ActualDataRange", typeof(Range<TPosition>?), typeof(Scale<TPosition, TInterval, TScaleUnit>), new PropertyMetadata((object)null, new PropertyChangedCallback(Scale<TPosition, TInterval, TScaleUnit>.OnActualDataRangeChanged)));
        private TPosition? _previousViewMinimum;
        private TPosition? _previousViewMaximum;

        public AxisCrossingPositionMode CrossingPositionMode
        {
            get
            {
                return (AxisCrossingPositionMode)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.CrossingPositionModeProperty);
            }
            set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.CrossingPositionModeProperty, (object)value);
            }
        }

        public Range<TPosition>? DataRange
        {
            get
            {
                return (Range<TPosition>?)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.DataRangeProperty);
            }
            set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.DataRangeProperty, (object)value);
            }
        }

        public TPosition ActualMinimum
        {
            get
            {
                return (TPosition)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinimumProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinimumProperty, (object)value);
            }
        }

        public TPosition ActualMaximum
        {
            get
            {
                return (TPosition)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMaximumProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMaximumProperty, (object)value);
            }
        }

        public TPosition ActualViewMinimum
        {
            get
            {
                return (TPosition)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualViewMinimumProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualViewMinimumProperty, (object)value);
            }
        }

        public TPosition ActualViewMaximum
        {
            get
            {
                return (TPosition)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualViewMaximumProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualViewMaximumProperty, (object)value);
            }
        }

        public TInterval ActualMajorInterval
        {
            get
            {
                return (TInterval)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMajorIntervalProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMajorIntervalProperty, (object)value);
            }
        }

        public TScaleUnit ActualMajorIntervalUnit
        {
            get
            {
                return (TScaleUnit)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMajorIntervalUnitProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMajorIntervalUnitProperty, (object)value);
            }
        }

        public TInterval ActualMajorIntervalOffset
        {
            get
            {
                return (TInterval)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMajorIntervalOffsetProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMajorIntervalOffsetProperty, (object)value);
            }
        }

        public TInterval ActualMinorInterval
        {
            get
            {
                return (TInterval)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinorIntervalProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinorIntervalProperty, (object)value);
            }
        }

        public TScaleUnit ActualMinorIntervalUnit
        {
            get
            {
                return (TScaleUnit)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinorIntervalUnitProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinorIntervalUnitProperty, (object)value);
            }
        }

        public TInterval ActualMinorIntervalOffset
        {
            get
            {
                return (TInterval)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinorIntervalOffsetProperty);
            }
            protected set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualMinorIntervalOffsetProperty, (object)value);
            }
        }

        public Range<TPosition>? ActualDataRange
        {
            get
            {
                return (Range<TPosition>?)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualDataRangeProperty);
            }
            set
            {
                this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ActualDataRangeProperty, (object)value);
            }
        }

        protected abstract Range<TPosition> DefaultRange { get; }

        public override double ProjectedStartMargin
        {
            get
            {
                if (this.ActualDataRange.HasValue && this.ActualDataRange.Value.HasData && this.ActualDataRange.Value.Minimum.CompareTo(this.ActualViewMinimum) > 0)
                    return this.Project(this.ActualDataRange.Value.Minimum);
                return 0.0;
            }
        }

        public override double ProjectedEndMargin
        {
            get
            {
                if (this.ActualDataRange.HasValue && this.ActualDataRange.Value.HasData && this.ActualDataRange.Value.Maximum.CompareTo(this.ActualViewMaximum) < 0)
                    return 1.0 - this.Project(this.ActualDataRange.Value.Maximum);
                return 0.0;
            }
        }

        public override bool IsEmpty
        {
            get
            {
                return this.ActualMaximum.CompareTo(this.ActualMinimum) <= 0;
            }
        }

        public override double ActualZoom
        {
            get
            {
                return 1.0 / RangeHelper.Size(this.ConvertActualViewToPercent());
            }
        }

        private static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMaximumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnMaximumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.IsScrolling = false;
            this.IsZooming = false;
            this.Invalidate();
        }

        private static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMinimumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnMinimumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.IsScrolling = false;
            this.IsZooming = false;
            this.Invalidate();
        }

        private static void OnViewMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnViewMaximumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnViewMaximumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnViewMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnViewMinimumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnViewMinimumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnMajorIntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMajorIntervalPropertyChanged((TInterval?)e.NewValue, (TInterval?)e.OldValue);
        }

        protected virtual void OnMajorIntervalPropertyChanged(TInterval? newValue, TInterval? oldValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnMajorIntervalUnitPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMajorIntervalUnitPropertyChanged((TScaleUnit?)e.NewValue, (TScaleUnit?)e.OldValue);
        }

        protected virtual void OnMajorIntervalUnitPropertyChanged(TScaleUnit? newValue, TScaleUnit? oldValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnMajorIntervalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMajorIntervalOffsetPropertyChanged((TInterval?)e.NewValue, (TInterval?)e.OldValue);
        }

        protected virtual void OnMajorIntervalOffsetPropertyChanged(TInterval? newValue, TInterval? oldValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnMinorIntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMinorIntervalPropertyChanged((TInterval?)e.NewValue, (TInterval?)e.OldValue);
        }

        protected virtual void OnMinorIntervalPropertyChanged(TInterval? newValue, TInterval? oldValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnMinorIntervalUnitPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMinorIntervalUnitPropertyChanged((TScaleUnit?)e.NewValue, (TScaleUnit?)e.OldValue);
        }

        protected virtual void OnMinorIntervalUnitPropertyChanged(TScaleUnit? newValue, TScaleUnit? oldValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnMinorIntervalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnMinorIntervalOffsetPropertyChanged((TInterval?)e.NewValue, (TInterval?)e.OldValue);
        }

        protected virtual void OnMinorIntervalOffsetPropertyChanged(TInterval? newValue, TInterval? oldValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnCrossingPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnCrossingPositionPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnCrossingPositionPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnCrossingPositionModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnCrossingPositionModePropertyChanged((AxisCrossingPositionMode)e.OldValue, (AxisCrossingPositionMode)e.NewValue);
        }

        protected virtual void OnCrossingPositionModePropertyChanged(AxisCrossingPositionMode oldValue, AxisCrossingPositionMode newValue)
        {
            this.IsScrolling = false;
            this.Invalidate();
        }

        private static void OnDataRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Range<TPosition>? oldValue = (Range<TPosition>?)e.OldValue;
            Range<TPosition>? newValue = (Range<TPosition>?)e.NewValue;
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnDataRangeChanged(oldValue, newValue);
        }

        protected virtual void OnDataRangeChanged(Range<TPosition>? oldValue, Range<TPosition>? newValue)
        {
            this.ActualDataRange = newValue;
        }

        public static void SetInterval(ScaleElementDefinition element, TInterval? value)
        {
            element.SetValue(Scale<TPosition, TInterval, TScaleUnit>.IntervalAttachedProperty, (object)value);
        }

        public static TInterval? GetInterval(ScaleElementDefinition element)
        {
            return (TInterval?)element.GetValue(Scale<TPosition, TInterval, TScaleUnit>.IntervalAttachedProperty);
        }

        private static void OnIntervalAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScaleElementDefinition element = d as ScaleElementDefinition;
            Scale<TPosition, TInterval, TScaleUnit> scale = element.Scale as Scale<TPosition, TInterval, TScaleUnit>;
            TInterval? newValue = (TInterval?)e.NewValue;
            TInterval? oldValue = (TInterval?)e.OldValue;
            scale.OnIntervalAttachedPropertyChanged(element, newValue, oldValue);
        }

        protected virtual void OnIntervalAttachedPropertyChanged(ScaleElementDefinition element, TInterval? newValue, TInterval? oldValue)
        {
            this.OnElementChanged(element);
        }

        public static void SetIntervalOffset(ScaleElementDefinition element, TInterval? value)
        {
            element.SetValue(Scale<TPosition, TInterval, TScaleUnit>.IntervalOffsetAttachedProperty, (object)value);
        }

        public static TInterval? GetIntervalOffset(ScaleElementDefinition element)
        {
            return (TInterval?)element.GetValue(Scale<TPosition, TInterval, TScaleUnit>.IntervalOffsetAttachedProperty);
        }

        private static void OnIntervalOffsetAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScaleElementDefinition element = d as ScaleElementDefinition;
            Scale<TPosition, TInterval, TScaleUnit> scale = element.Scale as Scale<TPosition, TInterval, TScaleUnit>;
            TInterval? newValue = (TInterval?)e.NewValue;
            TInterval? oldValue = (TInterval?)e.OldValue;
            scale.OnIntervalOffsetAttachedPropertyChanged(element, newValue, oldValue);
        }

        protected virtual void OnIntervalOffsetAttachedPropertyChanged(ScaleElementDefinition element, TInterval? newValue, TInterval? oldValue)
        {
            this.OnElementChanged(element);
        }

        public static void SetIntervalUnit(ScaleElementDefinition element, TPosition? value)
        {
            element.SetValue(Scale<TPosition, TInterval, TScaleUnit>.IntervalUnitAttachedProperty, (object)value);
        }

        public static TScaleUnit? GetIntervalUnit(ScaleElementDefinition element)
        {
            return (TScaleUnit?)element.GetValue(Scale<TPosition, TInterval, TScaleUnit>.IntervalUnitAttachedProperty);
        }

        private static void OnIntervalUnitAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScaleElementDefinition element = d as ScaleElementDefinition;
            Scale<TPosition, TInterval, TScaleUnit> scale = element.Scale as Scale<TPosition, TInterval, TScaleUnit>;
            TScaleUnit? newValue = (TScaleUnit?)e.NewValue;
            TScaleUnit? oldValue = (TScaleUnit?)e.OldValue;
            scale.OnIntervalUnitAttachedPropertyChanged(element, newValue, oldValue);
        }

        protected virtual void OnIntervalUnitAttachedPropertyChanged(ScaleElementDefinition element, TScaleUnit? newValue, TScaleUnit? oldValue)
        {
            this.OnElementChanged(element);
        }

        public static void SetMaxCount(ScaleElementDefinition element, int? value)
        {
            element.SetValue(Scale<TPosition, TInterval, TScaleUnit>.MaxCountAttachedProperty, (object)value);
        }

        public static int? GetMaxCount(ScaleElementDefinition element)
        {
            return (int?)element.GetValue(Scale<TPosition, TInterval, TScaleUnit>.MaxCountAttachedProperty);
        }

        private static void OnMaxCountAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((d as ScaleElementDefinition).Scale as Scale<TPosition, int, TScaleUnit>).OnMaxCountAttachedPropertyChanged((int?)e.NewValue, (int?)e.OldValue);
        }

        protected virtual void OnMaxCountAttachedPropertyChanged(int? newValue, int? oldValue)
        {
            this.Invalidate();
        }

        private static void OnActualMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnActualMinimumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnActualMinimumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.Invalidate();
        }

        private static void OnActualMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnActualMaximumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnActualMaximumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.Invalidate();
        }

        private static void OnActualViewMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnActualViewMinimumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnActualViewMinimumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.InvalidateView();
        }

        private static void OnActualViewMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnActualViewMaximumPropertyChanged((TPosition?)e.OldValue, (TPosition?)e.NewValue);
        }

        protected virtual void OnActualViewMaximumPropertyChanged(TPosition? oldValue, TPosition? newValue)
        {
            this.InvalidateView();
        }

        private static void OnActualDataRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Range<TPosition>? oldValue = (Range<TPosition>?)e.OldValue;
            Range<TPosition>? newValue = (Range<TPosition>?)e.NewValue;
            ((Scale<TPosition, TInterval, TScaleUnit>)d).OnActualDataRangeChanged(oldValue, newValue);
        }

        protected virtual void OnActualDataRangeChanged(Range<TPosition>? oldValue, Range<TPosition>? newValue)
        {
            if (this.DataRange.HasValue)
            {
                Range<TPosition>? nullable = newValue;
                Range<TPosition>? dataRange = this.DataRange;
                if ((nullable.HasValue != dataRange.HasValue ? 1 : (!nullable.HasValue ? 0 : (nullable.GetValueOrDefault() != dataRange.GetValueOrDefault() ? 1 : 0))) != 0)
                {
                    this.ActualDataRange = this.DataRange;
                    return;
                }
            }
            this.ResetView();
            this.Invalidate();
        }

        private void CalculateActualCrossingPosition()
        {
            IComparable comparable = (IComparable)this.GetValue(Scale<TPosition, TInterval, TScaleUnit>.CrossingPositionProperty);
            this.HasCustomCrossingPosition = false;
            if (comparable != null)
            {
                if (comparable.CompareTo((object)this.ActualMinimum) < 0)
                    this.ActualCrossingPosition = (object)this.ActualMinimum;
                else if (comparable.CompareTo((object)this.ActualMaximum) > 0)
                {
                    this.ActualCrossingPosition = (object)this.ActualMaximum;
                }
                else
                {
                    this.ActualCrossingPosition = (object)comparable;
                    this.HasCustomCrossingPosition = true;
                }
            }
            else
            {
                switch (this.CrossingPositionMode)
                {
                    case AxisCrossingPositionMode.Minimum:
                        this.ActualCrossingPosition = (object)this.ActualMinimum;
                        break;
                    case AxisCrossingPositionMode.Maximum:
                        this.ActualCrossingPosition = (object)this.ActualMaximum;
                        break;
                    default:
                        this.ActualCrossingPosition = this.GetAutomaticCrossing();
                        break;
                }
            }
        }

        internal virtual object GetAutomaticCrossing()
        {
            return (object)this.ActualMinimum;
        }

        public override void Recalculate()
        {
            this.CalculateActualCrossingPosition();
            this.CalculateActualZoomRange();
        }

        protected abstract TPosition ConvertToPositionType(object value);

        public abstract double Project(TPosition value);

        public override double ProjectDataValue(object value)
        {
            if (value == null)
                return double.NaN;
            return this.Project(this.ConvertToPositionType(value));
        }

        public override void UpdateRange(IEnumerable<Range<IComparable>> ranges)
        {
            if (ranges != null)
            {
                Range<TPosition> range = this.GetRange(ranges);
                this.DataRange = new Range<TPosition>?(range.HasData ? range : this.DefaultRange);
            }
            else
                this.DataRange = new Range<TPosition>?();
        }

        internal override void UpdateRangeIfUndefined(IEnumerable<Range<IComparable>> ranges)
        {
            if (this.DataRange.HasValue)
                return;
            Range<TPosition> range = this.GetRange(ranges);
            this.ActualDataRange = range.HasData ? new Range<TPosition>?(range) : new Range<TPosition>?();
        }

        private Range<TPosition> GetRange(IEnumerable<Range<IComparable>> ranges)
        {
            Range<TPosition> range1 = new Range<TPosition>();
            foreach (Range<IComparable> range2 in ranges)
            {
                if (range2.HasData)
                {
                    Range<TPosition> range3 = new Range<TPosition>(this.ConvertToPositionType((object)range2.Minimum), this.ConvertToPositionType((object)range2.Maximum));
                    range1 = range1.Add(range3);
                }
            }
            return range1;
        }

        internal virtual double ConvertProjectedValueToPercent(double value)
        {
            return RangeHelper.Project(new Range<double>(this.Project(this.ActualMinimum), this.Project(this.ActualMaximum)), value, Scale.PercentRange);
        }

        public abstract void ScrollToValue(TPosition position);

        public override void ScrollBy(double offset)
        {
            this.ScrollToPercent(this.ConvertProjectedValueToPercent(offset));
        }

        public virtual void ZoomToValue(TPosition viewMinimum, TPosition viewMaximum)
        {
            if (viewMinimum.CompareTo(viewMaximum) == 0)
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, Properties.Resources.Scale_ViewRangeIsEmpty, new object[2]
                {
          (object) viewMinimum,
          (object) viewMaximum
                }));
            if (viewMinimum.CompareTo(viewMaximum) > 0)
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, Properties.Resources.Scale_ViewRangeIsReverse, new object[2]
                {
          (object) viewMinimum,
          (object) viewMaximum
                }));
            this.BoxViewRange(ref viewMinimum, ref viewMaximum);
            this.IsZooming = true;
            this.BeginInit();
            this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ViewMinimumProperty, (object)viewMinimum);
            this.SetValue(Scale<TPosition, TInterval, TScaleUnit>.ViewMaximumProperty, (object)viewMaximum);
            this.EndInit();
        }

        public override void ZoomBy(double centerValue, double ratio)
        {
            double num = this.ConvertProjectedValueToPercent(centerValue);
            Range<double> view = this.ConvertActualViewToPercent();
            this.BoxZoomRatio(ref ratio, view);
            if (DoubleHelper.EqualsWithPrecision(ratio, 1.0, 0.001))
                return;
            this.ZoomToPercent(num - (num - view.Minimum) / ratio, num + (view.Maximum - num) / ratio);
        }

        internal void BoxZoomRatio(ref double ratio, Range<double> view)
        {
            double num1 = 1.0 / RangeHelper.Size(view);
            double num2 = num1 * ratio;
            if (num2 < this.ActualZoomRange.Minimum)
            {
                double minimum = this.ActualZoomRange.Minimum;
                ratio = Math.Min(minimum / num1, 1.0);
            }
            else
            {
                if (num2 <= this.ActualZoomRange.Maximum)
                    return;
                double maximum = this.ActualZoomRange.Maximum;
                ratio = Math.Max(maximum / num1, 1.0);
            }
        }

        internal abstract void BoxViewRange(ref TPosition viewMinimum, ref TPosition viewMaximum);

        public override Range<double> ConvertActualViewToPercent()
        {
            return new Range<double>(this.ConvertToPercent((object)this.ActualViewMinimum), this.ConvertToPercent((object)this.ActualViewMaximum));
        }

        internal Range<double> ProjectActualRange()
        {
            return new Range<double>(this.Project(this.ActualMinimum), this.Project(this.ActualMaximum));
        }

        protected override void OnViewChanged()
        {
            Range<IComparable> newRange = new Range<IComparable>((IComparable)this.ActualViewMinimum, (IComparable)this.ActualViewMaximum);
            Range<IComparable> oldRange = !this._previousViewMaximum.HasValue ? new Range<IComparable>((IComparable)this.DefaultRange.Minimum, (IComparable)this.DefaultRange.Maximum) : new Range<IComparable>((IComparable)this._previousViewMinimum, (IComparable)this._previousViewMaximum);
            this._previousViewMinimum = new TPosition?(this.ActualViewMinimum);
            this._previousViewMaximum = new TPosition?(this.ActualViewMaximum);
            this.OnViewChanged(oldRange, newRange);
        }
    }
}
