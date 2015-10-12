using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class NumericScale : Scale<double, double, NumericScaleUnit>
    {
        public static readonly DependencyProperty IncludeZeroProperty = DependencyProperty.Register("IncludeZero", typeof(AutoBool), typeof(NumericScale), new PropertyMetadata((object)AutoBool.Auto, new PropertyChangedCallback(NumericScale.OnIncludeZeroChanged)));
        public static readonly DependencyProperty MaxMarginProperty = DependencyProperty.Register("MaxMargin", typeof(double?), typeof(NumericScale), new PropertyMetadata((object)null, new PropertyChangedCallback(NumericScale.OnMaxMarginChanged)));
        private bool _actualIncludeZero = true;
        private const int MajorMinorRatio = 5;
        private const int MaxMarginCutoffCount = 5;
        private const double MaxMarginAfterCutoffRatio = 0.75;
        internal const string ActualIncludeZeroPropertryName = "ActualIncludeZero";
        internal const string ActualMaxMarginPropertryName = "ActualMaxMargin";
        private NumericRangeInfo _rangeInfo;
        private NumericSequence _majorSequence;
        private NumericSequence _minorSequence;
        private double _actualMaxMargin;
        private DisplayUnitSystem _displayUnitSystem;

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Maximum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.MaximumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MaximumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? Minimum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.MinimumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MinimumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? ViewMaximum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.ViewMaximumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.ViewMaximumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? ViewMinimum
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.ViewMinimumProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.ViewMinimumProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? MajorInterval
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.MajorIntervalProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MajorIntervalProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? MajorIntervalOffset
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.MajorIntervalOffsetProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MajorIntervalOffsetProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<NumericScaleUnit>))]
        public NumericScaleUnit? MajorIntervalUnit
        {
            get
            {
                return (NumericScaleUnit?)this.GetValue(Scale<double, double, NumericScaleUnit>.MajorIntervalUnitProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MajorIntervalUnitProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? MinorInterval
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.MinorIntervalProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MinorIntervalProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? MinorIntervalOffset
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.MinorIntervalOffsetProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MinorIntervalOffsetProperty, (object)value);
            }
        }

        [TypeConverter(typeof(NullableConverter<NumericScaleUnit>))]
        public NumericScaleUnit? MinorIntervalUnit
        {
            get
            {
                return (NumericScaleUnit?)this.GetValue(Scale<double, double, NumericScaleUnit>.MinorIntervalUnitProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.MinorIntervalUnitProperty, (object)value);
            }
        }

        public AutoBool IncludeZero
        {
            get
            {
                return (AutoBool)this.GetValue(NumericScale.IncludeZeroProperty);
            }
            set
            {
                this.SetValue(NumericScale.IncludeZeroProperty, (object)value);
            }
        }

        public bool ActualIncludeZero
        {
            get
            {
                return this._actualIncludeZero;
            }
            internal set
            {
                if (this._actualIncludeZero == value)
                    return;
                this._actualIncludeZero = value;
                this.OnPropertyChanged("ActualIncludeZero");
            }
        }

        public double? MaxMargin
        {
            get
            {
                return (double?)this.GetValue(NumericScale.MaxMarginProperty);
            }
            set
            {
                this.SetValue(NumericScale.MaxMarginProperty, (object)value);
            }
        }

        public double ActualMaxMargin
        {
            get
            {
                return this._actualMaxMargin;
            }
            internal set
            {
                if (this._actualMaxMargin == value)
                    return;
                this._actualMaxMargin = value;
                this.OnPropertyChanged("ActualMaxMargin");
            }
        }

        [TypeConverter(typeof(NullableConverter<double>))]
        public double? CrossingPosition
        {
            get
            {
                return (double?)this.GetValue(Scale<double, double, NumericScaleUnit>.CrossingPositionProperty);
            }
            set
            {
                this.SetValue(Scale<double, double, NumericScaleUnit>.CrossingPositionProperty, (object)value);
            }
        }

        protected override Range<double> DefaultRange
        {
            get
            {
                return new Range<double>(0.0, 10.0);
            }
        }

        public DisplayUnitSystem DisplayUnitSystem
        {
            get
            {
                return this._displayUnitSystem;
            }
            set
            {
                if (this._displayUnitSystem == value)
                    return;
                if (this._displayUnitSystem != null)
                    this._displayUnitSystem.PropertyChanged -= new PropertyChangedEventHandler(this.DisplayUnitSystem_PropertyChanged);
                this._displayUnitSystem = value;
                if (this._displayUnitSystem != null)
                    this._displayUnitSystem.PropertyChanged += new PropertyChangedEventHandler(this.DisplayUnitSystem_PropertyChanged);
                this.OnElementChanged((ScaleElementDefinition)this.LabelDefinition);
            }
        }

        internal override int ActualMajorCount
        {
            get
            {
                if (this._majorSequence == null)
                    return 0;
                return this._majorSequence.Count;
            }
        }

        internal override object DefaultLabelContent
        {
            get
            {
                return (object)10000;
            }
        }

        public NumericScale()
        {
            this._displayUnitSystem = (DisplayUnitSystem)new DefaultDisplayUnitSystem();
            this._displayUnitSystem.PropertyChanged += new PropertyChangedEventHandler(this.DisplayUnitSystem_PropertyChanged);
            this.LabelDefinition.SampleContent = this.LabelDefinition.GetContent((object)1000);
        }

        private static void OnIncludeZeroChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((NumericScale)d).OnIncludeZeroChanged(args);
        }

        internal virtual void OnIncludeZeroChanged(DependencyPropertyChangedEventArgs args)
        {
            this.Recalculate();
        }

        private static void OnMaxMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((NumericScale)d).OnMaxMarginChanged(args);
        }

        internal virtual void OnMaxMarginChanged(DependencyPropertyChangedEventArgs args)
        {
            this.Recalculate();
        }

        private void DisplayUnitSystem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.Invalidate();
        }

        public override bool CanProject(DataValueType valueType)
        {
            if (valueType != DataValueType.Float && valueType != DataValueType.Integer)
                return valueType == DataValueType.Auto;
            return true;
        }

        protected override double ConvertToPositionType(object value)
        {
            return ValueHelper.ToDouble(value);
        }

        protected override void ResetView()
        {
            this.BeginInit();
            base.ResetView();
            this.ViewMinimum = new double?();
            this.ViewMaximum = new double?();
            this.EndInit();
        }

        public override void Recalculate()
        {
            this.CalculateRangeInfo();
            this.CalculateActualIntervalUnits();
            if (this.ActualMajorIntervalUnit != NumericScaleUnit.MinorInterval)
            {
                this.CalculateMajorSequence(this.MaxCount, this.ActualMaxMargin);
                this.CalculateMinorSequence(this.MaxCount * 5, this.ActualMaxMargin);
            }
            else
            {
                this.CalculateMinorSequence(this.MaxCount * 5, this.ActualMaxMargin);
                this.CalculateMajorSequence(this.MaxCount, this.ActualMaxMargin);
            }
            this.CalculateActual();
            this.CalculateSampleContent();
            if (this.DisplayUnitSystem != null)
                this.DisplayUnitSystem.CalculateActualDisplayUnit(Math.Min(this.ActualMajorInterval, this.ActualViewMaximum - this.ActualViewMinimum), this.LabelDefinition.Format);
            base.Recalculate();
        }

        internal override void RecalculateIfEmpty()
        {
            if (this._majorSequence != null && this._minorSequence != null)
                return;
            this.Recalculate();
        }

        private void CalculateActualIntervalUnits()
        {
            this.ActualMajorIntervalUnit = this.MajorIntervalUnit.HasValue ? this.MajorIntervalUnit.Value : NumericScaleUnit.Number;
            if (this.ActualMajorIntervalUnit == NumericScaleUnit.MinorInterval)
            {
                NumericScaleUnit? minorIntervalUnit = this.MinorIntervalUnit;
                if ((minorIntervalUnit.GetValueOrDefault() != NumericScaleUnit.MajorInterval ? 0 : (minorIntervalUnit.HasValue ? 1 : 0)) != 0)
                    this.ActualMajorIntervalUnit = NumericScaleUnit.Number;
            }
            this.ActualMinorIntervalUnit = this.MajorIntervalUnit.HasValue ? this.MajorIntervalUnit.Value : NumericScaleUnit.MajorInterval;
            if (this.MinorInterval.HasValue || this.MinorIntervalOffset.HasValue)
                return;
            this.ActualMinorIntervalUnit = NumericScaleUnit.MajorInterval;
        }

        private void CalculateRangeInfo()
        {
            this.ActualIncludeZero = ValueHelper.ToBoolean(this.IncludeZero, ValueHelper.ToBoolean(this.Defaults.IncludeZero, false));
            this.ActualMaxMargin = this.GetMaxAllowedMargin(this.MaxCount);
            Range<double>? dataRange = RangeHelper.Add<double>(this.ActualDataRange, this.CrossingPosition);
            double? viewMinimum = this.ViewMinimum;
            double? minimum = viewMinimum.HasValue ? new double?(viewMinimum.GetValueOrDefault()) : this.Minimum;
            double? viewMaximum = this.ViewMaximum;
            double? maximum = viewMaximum.HasValue ? new double?(viewMaximum.GetValueOrDefault()) : this.Maximum;
            int num = this.ActualIncludeZero ? 1 : 0;
            double? majorInterval = this.MajorInterval;
            double? interval = majorInterval.HasValue ? new double?(majorInterval.GetValueOrDefault()) : this.MinorInterval;
            this._rangeInfo = NumericRangeInfo.Calculate(dataRange, minimum, maximum, num != 0, interval);
        }

        private void CalculateMajorSequence(int maxCount, double maxAllowedMargin)
        {
            switch (this.ActualMajorIntervalUnit)
            {
                case NumericScaleUnit.Number:
                    this._majorSequence = NumericSequence.Calculate(this._rangeInfo, this.MajorInterval, this.MajorIntervalOffset, maxCount, this.GetMinPower(), (DoubleR10[])null, true, maxAllowedMargin);
                    break;
                case NumericScaleUnit.MajorInterval:
                    this._majorSequence = NumericSequence.Calculate(this._rangeInfo, new double?(), new double?(), maxCount, int.MinValue, (DoubleR10[])null, true, 1.0);
                    this._majorSequence = (NumericSequence)RelativeSequence.Calculate(this._majorSequence, this.MajorInterval, this.MajorIntervalOffset, 1.0);
                    break;
                case NumericScaleUnit.MinorInterval:
                    this._majorSequence = (NumericSequence)RelativeSequence.Calculate(this._minorSequence, this.MajorInterval, this.MajorIntervalOffset, 5.0);
                    break;
            }
        }

        private void CalculateMinorSequence(int maxCount, double maxAllowedMargin)
        {
            switch (this.ActualMinorIntervalUnit)
            {
                case NumericScaleUnit.Number:
                    this._minorSequence = NumericSequence.Calculate(this._rangeInfo, this.MinorInterval, this.MinorIntervalOffset, maxCount, this.GetMinPower(), (DoubleR10[])null, true, maxAllowedMargin);
                    break;
                case NumericScaleUnit.MajorInterval:
                    this._minorSequence = (NumericSequence)RelativeSequence.Calculate(this._majorSequence, this.MinorInterval, this.MinorIntervalOffset, 0.2);
                    break;
                case NumericScaleUnit.MinorInterval:
                    this._minorSequence = (NumericSequence)RelativeSequence.Calculate(this._majorSequence, this.MinorInterval, this.MinorIntervalOffset, 0.2);
                    this._minorSequence = (NumericSequence)RelativeSequence.Calculate(this._minorSequence, this.MinorInterval, this.MinorIntervalOffset, 1.0);
                    break;
            }
            this.ActualMinorInterval = (double)this._minorSequence.Interval;
            this.ActualMinorIntervalOffset = (double)this._minorSequence.IntervalOffset;
        }

        private int GetMinPower()
        {
            return this.ValueType != DataValueType.Integer ? int.MinValue : 0;
        }

        private double GetMaxAllowedMargin(int maxCount)
        {
            if (this.MaxMargin.HasValue)
                return this.MaxMargin.Value;
            if (maxCount > 5 || this.Defaults.MaxAllowedMargin == 1.0)
                return this.Defaults.MaxAllowedMargin;
            return this.Defaults.MaxAllowedMargin * 0.75;
        }

        private void CalculateActual()
        {
            double num1 = (double)this._majorSequence.Minimum;
            double num2 = (double)this._majorSequence.Maximum;
            this.ActualMajorInterval = (double)this._majorSequence.Interval;
            this.ActualMajorIntervalOffset = (double)this._majorSequence.IntervalOffset;
            this.ActualMinorInterval = (double)this._minorSequence.Interval;
            this.ActualMinorIntervalOffset = (double)this._minorSequence.IntervalOffset;
            if (!this.IsZooming)
            {
                if (this.Minimum.HasValue)
                    this.ActualMinimum = this.Minimum.Value;
                else
                    this.ActualMinimum = num1;
                if (this.Maximum.HasValue)
                    this.ActualMaximum = this.Maximum.Value;
                else
                    this.ActualMaximum = num2;
                if (this.ActualMinimum == this.ActualMaximum)
                {
                    this.ActualMinimum -= this.ActualMajorInterval;
                    this.ActualMaximum += this.ActualMajorInterval;
                }
            }
            this.ActualViewMinimum = this.ViewMinimum.HasValue ? this.ViewMinimum.Value : this.ActualMinimum;
            this.ActualViewMaximum = this.ViewMaximum.HasValue ? this.ViewMaximum.Value : this.ActualMaximum;
        }

        private void CalculateSampleContent()
        {
            string str1 = (string)null;
            for (int index = 0; index < this._majorSequence.Count && index < 20; ++index)
            {
                this.LabelDefinition.DisplayUnitSystem = this.DisplayUnitSystem;
                string str2 = this.LabelDefinition.GetContent((object)(double)this._majorSequence[index]) as string;
                if (!string.IsNullOrEmpty(str2) && (str1 == null || str2.Length > str1.Length))
                    str1 = str2;
            }
            if (string.IsNullOrEmpty(str1))
                this.LabelDefinition.SampleContent = this.LabelDefinition.GetContent((object)1000);
            else
                this.LabelDefinition.SampleContent = (object)str1;
        }

        private NumericSequence CalculateElementSequence(ScaleElementDefinition element, NumericScaleUnit defaultUnit)
        {
            double? interval = Scale<double, double, NumericScaleUnit>.GetInterval(element);
            double? intervalOffset = Scale<double, double, NumericScaleUnit>.GetIntervalOffset(element);
            NumericScaleUnit? intervalUnit = Scale<double, double, NumericScaleUnit>.GetIntervalUnit(element);
            NumericScaleUnit numericScaleUnit = intervalUnit.HasValue ? intervalUnit.Value : defaultUnit;
            int? maxCount1 = Scale<double, double, NumericScaleUnit>.GetMaxCount(element);
            int maxCount2 = maxCount1.HasValue ? maxCount1.Value : this.MaxCount;
            NumericSequence numericSequence = (NumericSequence)null;
            switch (numericScaleUnit)
            {
                case NumericScaleUnit.Number:
                    numericSequence = NumericSequence.Calculate(new NumericRangeInfo((DoubleR10)this.ActualViewMinimum, (DoubleR10)this.ActualViewMaximum), interval, intervalOffset, maxCount2, int.MinValue, (DoubleR10[])null, true, 1.0);
                    break;
                case NumericScaleUnit.MajorInterval:
                    numericSequence = interval.HasValue || intervalOffset.HasValue ? (NumericSequence)RelativeSequence.Calculate(this._majorSequence, interval, intervalOffset, 1.0) : this._majorSequence;
                    break;
                case NumericScaleUnit.MinorInterval:
                    numericSequence = interval.HasValue || intervalOffset.HasValue ? (NumericSequence)RelativeSequence.Calculate(this._minorSequence, interval, intervalOffset, 1.0) : this._minorSequence;
                    break;
            }
            return numericSequence;
        }

        protected override double GetMaxPossibleZoom()
        {
            double num = this.ConvertToPercent((object)(this.ActualMinimum + Math.Pow(10.0, (double)this.GetMinPower())));
            return Math.Max(1.0, Math.Min(Scale.MaxZoomRange.Maximum, 1.0 / (num * (double)this.ActualMajorCount)));
        }

        public override double Project(double value)
        {
            return RangeHelper.Project(new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum), value, Scale.PercentRange);
        }

        public override double ConvertToPercent(object value)
        {
            if (value == null)
                return double.NaN;
            double num = Convert.ToDouble(value, (IFormatProvider)CultureInfo.InvariantCulture);
            Range<double> fromRange = new Range<double>(this.ActualMinimum, this.ActualMaximum);
            double precision1 = DoubleHelper.GetPrecision(num, fromRange.Minimum, fromRange.Maximum);
            if (DoubleHelper.LessWithPrecision(num, fromRange.Minimum, precision1) || DoubleHelper.GreaterWithPrecision(num, fromRange.Maximum, precision1))
                return double.NaN;
            double precision2 = DoubleHelper.GetPrecision(1, new double[0]);
            return DoubleHelper.RoundWithPrecision(RangeHelper.Project(fromRange, num, Scale.PercentRange), precision2);
        }

        public override IEnumerable<double> ProjectValues(IEnumerable values)
        {
            if (values != null)
            {
                Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
                foreach (object obj in values)
                {
                    double x = this.ConvertToPositionType(obj);
                    yield return RangeHelper.Project(fromRange, x, Scale.PercentRange);
                }
            }
        }

        private IEnumerable<ScalePosition> Project(ScaleElementDefinition element, NumericScaleUnit defaultUnit)
        {
            this.RecalculateIfEmpty();
            NumericSequence sequence = this.CalculateElementSequence(element, defaultUnit);
            Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
            foreach (DoubleR10 doubleR10 in sequence)
            {
                double x = (double)doubleR10;
                yield return new ScalePosition((object)x, RangeHelper.Project(fromRange, x, Scale.PercentRange));
            }
        }

        private IEnumerable<ScalePosition> ProjectLabels(LabelDefinition element, NumericScaleUnit defaultUnit)
        {
            this.RecalculateIfEmpty();
            NumericSequence sequence = this.CalculateElementSequence((ScaleElementDefinition)element, defaultUnit);
            Range<double> fromRange = new Range<double>(this.ActualViewMinimum, this.ActualViewMaximum);
            foreach (DoubleR10 doubleR10 in sequence)
            {
                double x = (double)doubleR10;
                yield return new ScalePosition((object)x, RangeHelper.Project(fromRange, x, Scale.PercentRange));
            }
        }

        public override IEnumerable<ScaleElementDefinition> ProjectElements()
        {
            this.MajorTickmarkDefinition.Positions = this.Project((ScaleElementDefinition)this.MajorTickmarkDefinition, NumericScaleUnit.MajorInterval);
            this.MinorTickmarkDefinition.Positions = this.Project((ScaleElementDefinition)this.MinorTickmarkDefinition, NumericScaleUnit.MinorInterval);
            this.LabelDefinition.DisplayUnitSystem = this.DisplayUnitSystem;
            this.LabelDefinition.Positions = this.ProjectLabels(this.LabelDefinition, NumericScaleUnit.MajorInterval);
            yield return (ScaleElementDefinition)this.MajorTickmarkDefinition;
            yield return (ScaleElementDefinition)this.MinorTickmarkDefinition;
            yield return (ScaleElementDefinition)this.LabelDefinition;
        }

        public override IEnumerable<ScalePosition> ProjectMajorIntervals()
        {
            return this.Project((ScaleElementDefinition)this.MajorTickmarkDefinition, NumericScaleUnit.MajorInterval);
        }

        public override void ScrollToValue(double position)
        {
            double num = this.ActualViewMaximum - this.ActualViewMinimum;
            double min = position;
            double max = position + num;
            RangeHelper.BoxRangeInsideAnother(ref min, ref max, this.ActualMinimum, this.ActualMaximum);
            double precision = DoubleHelper.GetPrecision(new double[2]
            {
        min,
        this.ActualViewMinimum
            });
            if (DoubleHelper.EqualsWithPrecision(min, this.ActualViewMinimum, precision))
                return;
            this._majorSequence.MoveToCover((DoubleR10)min, (DoubleR10)max);
            this.CalculateMinorSequence(this.MaxCount * 5, this.GetMaxAllowedMargin(this.MaxCount));
            this.BeginInit();
            this.ViewMinimum = new double?(this.ActualViewMinimum = min);
            this.ViewMaximum = new double?(this.ActualViewMaximum = max);
            this.IsScrolling = true;
            this.EndInit();
        }

        public override void ScrollToPercent(double viewPosition)
        {
            Range<double> targetRange = new Range<double>(this.ActualMinimum, this.ActualMaximum);
            this.ScrollToValue(RangeHelper.Project(Scale.PercentRange, viewPosition, targetRange));
        }

        public override void ZoomToPercent(double viewMinimum, double viewMaximum)
        {
            double minSize = 1.0 / this.ActualZoomRange.Maximum;
            double maxSize = 1.0 / this.ActualZoomRange.Minimum;
            RangeHelper.BoxRangeInsideAnother(ref viewMinimum, ref viewMaximum, 0.0, 1.0, minSize, maxSize, 0.01);
            Range<double> targetRange = new Range<double>(this.ActualMinimum, this.ActualMaximum);
            this.ZoomToValue(RangeHelper.Project(Scale.PercentRange, viewMinimum, targetRange), RangeHelper.Project(Scale.PercentRange, viewMaximum, targetRange));
        }

        internal override void BoxViewRange(ref double viewMinimum, ref double viewMaximum)
        {
            RangeHelper.BoxRangeInsideAnother(ref viewMinimum, ref viewMaximum, this.ActualMinimum, this.ActualMaximum);
        }

        internal override object GetAutomaticCrossing()
        {
            if (DoubleHelper.GreaterOrEqualWithPrecision(this.ActualMinimum, 0.0))
                return (object)this.ActualMinimum;
            if (DoubleHelper.LessWithPrecision(this.ActualMaximum, 0.0))
                return (object)this.ActualMaximum;
            this.HasCustomCrossingPosition = true;
            return (object)0.0;
        }

        internal class NumericScaleFactory : Scale.ScaleFactory
        {
            public override Scale Create(DataValueType valueType)
            {
                NumericScale numericScale = new NumericScale();
                numericScale.ValueType = valueType;
                return (Scale)numericScale;
            }
        }
    }
}
