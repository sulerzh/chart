﻿using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class XYLabelsPanel : XYAxisBasePanel, IRequireMarginOnEdges
    {
        private static readonly Size TotalSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        public static readonly DependencyProperty CoordinateFromProperty = DependencyProperty.RegisterAttached("CoordinateFrom", typeof(double), typeof(XYLabelsPanel), new PropertyMetadata((object)0.0, new PropertyChangedCallback(XYLabelsPanel.OnCoordinateFromPropertyChanged)));
        public static readonly DependencyProperty CoordinateToProperty = DependencyProperty.RegisterAttached("CoordinateTo", typeof(double), typeof(XYLabelsPanel), new PropertyMetadata((object)0.0, new PropertyChangedCallback(XYLabelsPanel.OnCoordinateToPropertyChanged)));
        public static readonly DependencyProperty LevelProperty = DependencyProperty.RegisterAttached("Level", typeof(int), typeof(XYLabelsPanel), new PropertyMetadata(new PropertyChangedCallback(XYLabelsPanel.OnLevelPropertyChanged)));
        public static readonly DependencyProperty LevelOffsetProperty = DependencyProperty.RegisterAttached("LevelOffset", typeof(double), typeof(XYLabelsPanel), new PropertyMetadata((object)0.0));
        public static readonly DependencyProperty OffsetPaddingProperty = DependencyProperty.Register("OffsetPadding", typeof(double), typeof(XYLabelsPanel), new PropertyMetadata((object)2.0, new PropertyChangedCallback(XYLabelsPanel.OnOffsetPaddingPropertyChanged)));
        public static readonly DependencyProperty MinimumDistanceBetweenChildrenProperty = DependencyProperty.Register("MinimumDistanceBetweenChildren", typeof(double), typeof(XYLabelsPanel), new PropertyMetadata((object)2.0, new PropertyChangedCallback(XYLabelsPanel.OnMinimumDistanceBetweenChildrenPropertyChanged)));
        private double _maxLabelCount = 100.0;
        private Thickness _requiredMargin = new Thickness(0.0);
        private const int DefaultMaxLabelCount = 100;
        internal const string LevelOffsetPropertyName = "LevelOffset";
        private PanelElementPool<AxisLabelControl, Axis> _axisLabelsPool;
        private PanelElementPool<Line, Axis> _axisLabelLinesPool;
        private AxisLabelControl _measureLabelHelper;
        private bool _isScrollLabelsInfoAquired;

        public double OffsetPadding
        {
            get
            {
                return (double)this.GetValue(XYLabelsPanel.OffsetPaddingProperty);
            }
            set
            {
                this.SetValue(XYLabelsPanel.OffsetPaddingProperty, (object)value);
            }
        }

        public double MinimumDistanceBetweenChildren
        {
            get
            {
                return (double)this.GetValue(XYLabelsPanel.MinimumDistanceBetweenChildrenProperty);
            }
            set
            {
                this.SetValue(XYLabelsPanel.MinimumDistanceBetweenChildrenProperty, (object)value);
            }
        }

        private AxisLabelControl MeasureLabelHelper
        {
            get
            {
                if (this._measureLabelHelper == null)
                {
                    this._measureLabelHelper = new AxisLabelControl();
                    this._measureLabelHelper.SetBinding(FrameworkElement.StyleProperty, (BindingBase)new Binding("LabelStyle")
                    {
                        Source = (object)this.Axis
                    });
                }
                return this._measureLabelHelper;
            }
        }

        private Thickness MeasureLabelPading
        {
            get
            {
                return ValueHelper.Inflate(this.MeasureLabelHelper.Padding, this.MeasureLabelHelper.BorderThickness);
            }
        }

        internal bool IsGrouped { get; private set; }

        internal IDictionary<int, double> LevelOffsets { get; set; }

        protected IDictionary<int, XYLabelsPanel.LabelMeasureResult> LevelMeasureResults { get; set; }

        internal Size AvailableSizeInMeasureCycle { get; private set; }

        public Thickness RequiredMargin
        {
            get
            {
                return this._requiredMargin;
            }
        }

        internal XYLabelsPanel(XYAxisPresenter presenter)
          : base(presenter)
        {
            this._axisLabelsPool = new PanelElementPool<AxisLabelControl, Axis>((Panel)this, (Func<AxisLabelControl>)(() => this.CreateLabel()), (Action<AxisLabelControl, Axis>)((label, a) => this.ResetLabel(label)), (Action<AxisLabelControl>)null);
            this._axisLabelLinesPool = new PanelElementPool<Line, Axis>((Panel)this, (Func<Line>)(() => this.CreateLabelLine()), (Action<Line, Axis>)((line, axis) => this.PrepareLabelLine(line)), (Action<Line>)null);
        }

        private AxisLabelControl CreateLabel()
        {
            AxisLabelControl axisLabelControl = new AxisLabelControl();
            axisLabelControl.SetBinding(FrameworkElement.StyleProperty, (BindingBase)new Binding("LabelStyle")
            {
                Source = (object)this.Axis
            });
            axisLabelControl.DataContext = (object)axisLabelControl;
            return axisLabelControl;
        }

        private void ResetLabel(AxisLabelControl label)
        {
            label.Opacity = this.MeasureLabelHelper.Opacity;
            label.Content = (object)null;
            label.SetLabelContentBounds(double.PositiveInfinity, double.PositiveInfinity);
        }

        public static double GetCoordinateFrom(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (double)element.GetValue(XYLabelsPanel.CoordinateFromProperty);
        }

        public static void SetCoordinateFrom(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(XYLabelsPanel.CoordinateFromProperty, (object)value);
        }

        public static void OnCoordinateFromPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement uiElement = dependencyObject as UIElement;
            if (uiElement == null)
                throw new ArgumentNullException("dependencyObject");
            XYLabelsPanel xyLabelsPanel = VisualTreeHelper.GetParent((DependencyObject)uiElement) as XYLabelsPanel;
            if (xyLabelsPanel == null)
                return;
            xyLabelsPanel.InvalidateMeasure();
        }

        public static double GetCoordinateTo(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (double)element.GetValue(XYLabelsPanel.CoordinateToProperty);
        }

        public static void SetCoordinateTo(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(XYLabelsPanel.CoordinateToProperty, (object)value);
        }

        public static void OnCoordinateToPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement uiElement = dependencyObject as UIElement;
            if (uiElement == null)
                throw new ArgumentNullException("dependencyObject");
            XYLabelsPanel xyLabelsPanel = VisualTreeHelper.GetParent((DependencyObject)uiElement) as XYLabelsPanel;
            if (xyLabelsPanel == null)
                return;
            xyLabelsPanel.InvalidateMeasure();
        }

        public static int GetLevel(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (int)element.GetValue(XYLabelsPanel.LevelProperty);
        }

        public static void SetLevel(UIElement element, int value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(XYLabelsPanel.LevelProperty, (object)value);
        }

        public static void OnLevelPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            UIElement uiElement = dependencyObject as UIElement;
            if (uiElement == null)
                throw new ArgumentNullException("dependencyObject");
            XYLabelsPanel xyLabelsPanel = VisualTreeHelper.GetParent((DependencyObject)uiElement) as XYLabelsPanel;
            if (xyLabelsPanel == null)
                return;
            xyLabelsPanel.InvalidateMeasure();
        }

        public static double GetLevelOffset(UIElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (double)element.GetValue(XYLabelsPanel.LevelOffsetProperty);
        }

        public static void SetLevelOffset(UIElement element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(XYLabelsPanel.LevelOffsetProperty, (object)value);
        }

        private static void OnOffsetPaddingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XYLabelsPanel)d).OnOffsetPaddingPropertyChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnOffsetPaddingPropertyChanged(double oldValue, double newValue)
        {
            this.InvalidateMeasure();
        }

        private static void OnMinimumDistanceBetweenChildrenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XYLabelsPanel)d).OnMinimumDistanceBetweenChildrenPropertyChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual void OnMinimumDistanceBetweenChildrenPropertyChanged(double oldValue, double newValue)
        {
            this.InvalidateMeasure();
        }

        internal Size MeasureLabelString(string text)
        {
            TextBlock textBlock = new TextBlock()
            {
                Text = text,
                FontSize = this.GetFontSizeFromFirstChild(),
                FontFamily = this.MeasureLabelHelper.FontFamily,
                FontWeight = this.MeasureLabelHelper.FontWeight
            };
            return new Size(textBlock.ActualWidth + ValueHelper.Width(this.MeasureLabelPading), textBlock.ActualHeight + ValueHelper.Height(this.MeasureLabelPading));
        }

        private double GetFontSizeFromFirstChild()
        {
            if (this.Children.Count > 0)
            {
                Control control = this.Children[0] as Control;
                if (control != null)
                    return control.FontSize;
            }
            return this.MeasureLabelHelper.FontSize;
        }

        private Line CreateLabelLine()
        {
            return new Line();
        }

        private void PrepareLabelLine(Line line)
        {
            TickMark tickMarkForStyling = this.Presenter.TickMarkForStyling;
            line.X1 = line.X2 = line.Y1 = line.Y2 = 0.0;
            if (this.Orientation == Orientation.Horizontal)
                line.Y2 = 1.0;
            else
                line.X2 = 1.0;
            line.Stretch = Stretch.Fill;
            line.Stroke = tickMarkForStyling.Stroke ?? tickMarkForStyling.Background;
            line.StrokeThickness = tickMarkForStyling.StrokeThickness == 0.0 ? tickMarkForStyling.Width : tickMarkForStyling.StrokeThickness;
        }

        protected Rect GetChildRectangle(AxisLabelControl child, Size desiredSize, double availableLength)
        {
            Size size = new Size(this.ElementWidth(desiredSize), this.ElementHeight(desiredSize));
            return new Rect(this.Presenter.ConvertScaleToAxisUnits(this.GetCenterCoordinate((UIElement)child), availableLength) - size.Width / 2.0, 0.0, size.Width, size.Height);
        }

        protected Rect GetChildContentRectangle(AxisLabelControl child, double availableLength)
        {
            return this.GetChildRectangle(child, child.GetLabelContentDesiredSize(), availableLength);
        }

        protected Rect GetChildClientRectangle(AxisLabelControl child, double availableLength)
        {
            return this.GetChildRectangle(child, child.DesiredSize, availableLength);
        }

        internal void ResetAxisLabels()
        {
            if (!this.Axis.ActualShowScrollZoomBar && !this.Axis.IsAllowsAutoZoom || this.Axis.Scale != null && this.Axis.Scale.IsScrolling && !this.Axis.ScheduleOneScaleUpdate)
                return;
            this._isScrollLabelsInfoAquired = false;
        }

        internal double GetBaseLabelsOffset()
        {
            if (this.Axis.ChartArea != null)
            {
                Thickness margin = this.Axis.ChartArea.PlotAreaPanel.Margin;
                switch (this.Presenter.ActualLocation)
                {
                    case Edge.Left:
                        return margin.Left;
                    case Edge.Right:
                        return margin.Right;
                    case Edge.Bottom:
                        return margin.Bottom;
                    case Edge.Top:
                        return margin.Top;
                }
            }
            return 0.0;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Scale scale = this.Axis.Scale;
            if (scale == null)
                return new Size(0.0, 0.0);
            if (this.Axis.UseDefaultSize)
                this._requiredMargin = new Thickness();
            availableSize = new Size(availableSize.Width + ValueHelper.Width(this._requiredMargin), availableSize.Height + ValueHelper.Height(this._requiredMargin));
            double val2 = 0.0;
            double num1 = this.ElementWidth(availableSize);
            double num2 = this.ElementHeight(availableSize);
            double maxMajorCount = double.MaxValue;
            double num3 = this.Presenter.ActualOrientation == Orientation.Horizontal ? -90.0 : 0.0;
            bool flag1 = false;
            Size size1 = Size.Empty;
            if (this.Axis.UseDefaultSize && num1 > 0.0 && num2 > 0.0)
            {
                size1 = this.MeasureLabelString(this.GetLabelSample(scale.DefaultLabelContent));
                num2 = this.Presenter.ActualOrientation != Orientation.Horizontal ? Math.Min(num2, this.ElementHeight(size1)) : (!(scale is ICategoryScale) ? Math.Min(num2, this.ElementHeight(size1) * 1.6) : Math.Min(num2, this.ElementWidth(size1)));
            }
            if (this._isScrollLabelsInfoAquired && this.Presenter.IsScaleZoomed && EnumerableFunctions.FastCount((IEnumerable)this.LevelMeasureResults) > 0)
            {
                double previousHeight = this.GetBaseLabelsOffset();
                EnumerableFunctions.ForEach<XYLabelsPanel.LabelMeasureResult>((IEnumerable<XYLabelsPanel.LabelMeasureResult>)this.LevelMeasureResults.Values, (Action<XYLabelsPanel.LabelMeasureResult>)(m => previousHeight += m.Offset + this.OffsetPadding));
                num2 = previousHeight;
            }
            this.AvailableSizeInMeasureCycle = new Size(num1, num2);
            if (num1 > 0.0)
            {
                ICategoryScale categoryScale = scale as ICategoryScale;
                if (categoryScale != null)
                {
                    Size size2 = this.MeasureLabelString(this.GetLabelSample(scale.SampleLabelContent));
                    maxMajorCount = Math.Round(num1 / (size2.Height - ValueHelper.Height(this.MeasureLabelPading)));
                    if (maxMajorCount < double.MaxValue)
                    {
                        bool isScrolling = scale.IsScrolling;
                        if (!this._isScrollLabelsInfoAquired && this.Axis.IsAllowsAutoZoom && !scale.IsEmpty && (!scale.IsScrolling || this.Axis.ScheduleOneScaleUpdate))
                        {
                            double num4 = categoryScale.ActualMaximum - categoryScale.ActualMinimum;
                            double viewSize = maxMajorCount * 0.9 / num4;
                            scale.ZoomToPercent(viewSize);
                        }
                        if (this.Axis.ScheduleOneScaleUpdate)
                        {
                            this.Axis.ScheduleOneScaleUpdate = false;
                            scale.IsScrolling = isScrolling;
                        }
                        scale.TryChangeMaxCount(maxMajorCount);
                        flag1 = this.Axis.IsAllowsAutoZoom && this.Presenter.IsScaleZoomed && !this.Axis.LabelAngle.HasValue || (double)scale.MaxCount < categoryScale.ActualMaximum;
                    }
                }
                else
                {
                    if (!this.Presenter.IsScaleZoomed)
                    {
                        this._maxLabelCount = 100.0;
                        this.MeasureScalarScaleLabel(scale, num1, scale.DefaultLabelContent, ref this._maxLabelCount);
                    }
                    for (int index = 0; index < 2; ++index)
                        this.MeasureScalarScaleLabel(scale, num1, scale.SampleLabelContent, ref this._maxLabelCount);
                }
            }
            this.Populate(num1);
            if (this.LevelOffsets == null)
                this.LevelOffsets = (IDictionary<int, double>)new Dictionary<int, double>();
            this.LevelOffsets.Clear();
            if (this.LevelMeasureResults == null)
                this.LevelMeasureResults = (IDictionary<int, XYLabelsPanel.LabelMeasureResult>)new Dictionary<int, XYLabelsPanel.LabelMeasureResult>();
            if (!this._isScrollLabelsInfoAquired || !this.Presenter.IsScaleZoomed)
                this.LevelMeasureResults.Clear();
            Thickness thickness = new Thickness(0.0);
            this._requiredMargin = thickness;
            if (this.Children.Count > 0 && num1 > 0.0)
            {
                List<XYLabelsPanel.LabelMeasureInfo> list = Enumerable.ToList<XYLabelsPanel.LabelMeasureInfo>((IEnumerable<XYLabelsPanel.LabelMeasureInfo>)Enumerable.OrderByDescending<XYLabelsPanel.LabelMeasureInfo, int>(Enumerable.Select<IGrouping<int, AxisLabelControl>, XYLabelsPanel.LabelMeasureInfo>(Enumerable.GroupBy<AxisLabelControl, int>(Enumerable.OfType<AxisLabelControl>((IEnumerable)this.Children), (Func<AxisLabelControl, int>)(child => XYLabelsPanel.GetLevel((UIElement)child))), (Func<IGrouping<int, AxisLabelControl>, XYLabelsPanel.LabelMeasureInfo>)(levelGroup => new XYLabelsPanel.LabelMeasureInfo(this, levelGroup.Key, (IList<AxisLabelControl>)Enumerable.ToList<AxisLabelControl>((IEnumerable<AxisLabelControl>)levelGroup)))), (Func<XYLabelsPanel.LabelMeasureInfo, int>)(li => li.Level)));
                foreach (XYLabelsPanel.LabelMeasureInfo labelMeasureInfo in list)
                {
                    bool flag2 = false;
                    XYLabelsPanel.LabelMeasureResult labelMeasureResult;
                    if (this.LevelMeasureResults.TryGetValue(labelMeasureInfo.Level, out labelMeasureResult))
                    {
                        labelMeasureInfo.Measure(labelMeasureResult.Angle, labelMeasureResult.Staggered);
                    }
                    else
                    {
                        if (this.Axis.CanHideLabels && maxMajorCount < (double)labelMeasureInfo.Elements.Count && (!this.Axis.LabelAngle.HasValue || DoubleHelper.EqualsWithPrecision(this.Axis.LabelAngle.Value, num3)))
                        {
                            if (this.IsGrouped)
                            {
                                labelMeasureInfo.HideAllLabels();
                            }
                            else
                            {
                                int count = labelMeasureInfo.Elements.Count;
                                do
                                {
                                    labelMeasureInfo.HideEachSecondLabel();
                                    bool flag3 = count % 2 != 0;
                                    count /= 2;
                                    if (flag3 && count > 0)
                                        ++count;
                                }
                                while ((double)count > maxMajorCount);
                            }
                            labelMeasureResult = labelMeasureInfo.Measure(this.Axis.LabelAngle ?? num3, false, 0.0);
                            flag2 = true;
                        }
                        else
                            labelMeasureResult = !flag1 ? labelMeasureInfo.Measure(this.Axis.LabelAngle ?? 0.0, false) : labelMeasureInfo.Measure(this.Axis.LabelAngle ?? num3, false, 0.0);
                        if (!flag2 && !flag1 && (this.Presenter.ActualOrientation == Orientation.Horizontal && labelMeasureResult.HasCollisions))
                        {
                            if (this.IsGrouped && !this.Axis.LabelAngle.HasValue)
                            {
                                labelMeasureResult = labelMeasureInfo.Measure(-90.0, false, 0.0);
                            }
                            else
                            {
                                if (this.Axis.CanStaggerLabels && this.Axis.Scale is CategoryScale)
                                {
                                    if (this.Axis.LabelAngle.HasValue)
                                    {
                                        double? labelAngle = this.Axis.LabelAngle;
                                        if ((labelAngle.GetValueOrDefault() != 0.0 ? 0 : (labelAngle.HasValue ? 1 : 0)) == 0)
                                            goto label_48;
                                    }
                                    labelMeasureResult = labelMeasureInfo.Measure(0.0, true);
                                }
                                label_48:
                                if (labelMeasureResult.HasCollisions && this.Axis.CanRotateLabels && !this.Axis.LabelAngle.HasValue)
                                {
                                    labelMeasureResult = labelMeasureInfo.Measure(-30.0, false, 0.0);
                                    if (labelMeasureResult.HasCollisions)
                                    {
                                        labelMeasureResult = labelMeasureInfo.Measure(-60.0, false, 0.0);
                                        if (labelMeasureResult.HasCollisions)
                                            labelMeasureResult = labelMeasureInfo.Measure(-90.0, false, 0.0);
                                    }
                                }
                            }
                        }
                    }
                    bool hasCollisions = labelMeasureResult.HasCollisions || flag2;
                    double? nullable = !this._isScrollLabelsInfoAquired || !this.Presenter.IsScaleZoomed ? new double?() : new double?(labelMeasureResult.Offset);
                    if (labelMeasureResult.HasCollisions && this.Axis.CanHideLabels && !flag2)
                    {
                        int num4 = 32;
                        do
                        {
                            if (this.IsGrouped)
                                labelMeasureInfo.HideAllLabels();
                            else
                                labelMeasureInfo.HideEachSecondLabel();
                            labelMeasureResult = labelMeasureInfo.Measure(labelMeasureResult.Angle, labelMeasureResult.Staggered, 0.0);
                        }
                        while (labelMeasureResult.HasCollisions && --num4 > 0);
                    }
                    if (labelMeasureInfo.Level == 0)
                        val2 += this.GetBaseLabelsOffset();
                    this.LevelOffsets[labelMeasureInfo.Level] = val2;
                    if (!this._isScrollLabelsInfoAquired)
                        this.LevelMeasureResults[labelMeasureInfo.Level] = new XYLabelsPanel.LabelMeasureResult(labelMeasureResult.Offset, labelMeasureResult.MarginLeft, labelMeasureResult.MarginRight, hasCollisions, labelMeasureResult.Angle, labelMeasureResult.Staggered);
                    val2 += (nullable ?? labelMeasureResult.Offset) + this.OffsetPadding;
                    thickness.Left = Math.Max(thickness.Left, labelMeasureResult.MarginLeft);
                    thickness.Right = Math.Max(thickness.Right, labelMeasureResult.MarginRight);
                    if (list.Count == 1)
                        this.Axis.ActualLabelAngle = new double?(labelMeasureResult.Angle);
                }
            }
            else
                this.Axis.ActualLabelAngle = new double?();
            if (this.Axis.UseDefaultSize && num1 > 0.0 && num2 > 0.0)
            {
                val2 = num2;
                if (this.Presenter.ActualOrientation == Orientation.Horizontal)
                {
                    Point angleOffsetBySize = this.GetAngleOffsetBySize(size1, -30.0);
                    thickness.Left = Math.Max(angleOffsetBySize.X, size1.Width / 2.0);
                    thickness.Right = size1.Width / 2.0;
                }
                else
                {
                    thickness.Left = size1.Height / 2.0;
                    thickness.Right = size1.Height / 2.0;
                }
            }
            switch (this.Orientation)
            {
                case Orientation.Horizontal:
                    this._requiredMargin = new Thickness(double.IsInfinity(availableSize.Width) ? 0.0 : thickness.Left, 0.0, double.IsInfinity(availableSize.Width) ? 0.0 : thickness.Right, 0.0);
                    return new Size(0.0, Math.Max(0.0, val2));
                case Orientation.Vertical:
                    this._requiredMargin = new Thickness(0.0, double.IsInfinity(availableSize.Height) ? 0.0 : thickness.Left, 0.0, double.IsInfinity(availableSize.Height) ? 0.0 : thickness.Right);
                    return new Size(Math.Max(0.0, val2), 0.0);
                default:
                    throw new NotSupportedException();
            }
        }

        private void MeasureScalarScaleLabel(Scale scale, double availableWidth, object labelContent, ref double maxLabelCount)
        {
            Size labelSampleSize = this.MeasureLabelString(this.GetLabelSample(labelContent));
            double num = this.EstimateLabelCount(availableWidth, labelSampleSize);
            if (num >= maxLabelCount)
                return;
            maxLabelCount = num;
            maxLabelCount = Math.Min(maxLabelCount, (double)(scale.PreferredMaxCount + 1));
            maxLabelCount = Math.Max(maxLabelCount, 2.0);
            scale.TryChangeMaxCount(maxLabelCount - 1.0);
        }

        private string GetLabelSample(object labelContent)
        {
            string str = labelContent as string;
            if (string.IsNullOrEmpty(str))
                return "MMMMMM";
            if (this.Axis.UseDefaultSize && this.Axis.Scale is ICategoryScale && this.Orientation == Orientation.Vertical)
                return str + "MMMM";
            return str + "MM";
        }

        private double EstimateLabelCount(double availableWidth, Size labelSampleSize)
        {
            if (this.Presenter.ActualOrientation == Orientation.Vertical)
                return Math.Round(availableWidth / (labelSampleSize.Height * 1.7));
            return Math.Round(availableWidth / labelSampleSize.Width);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this._isScrollLabelsInfoAquired = !this.Axis.UseDefaultSize;
            if (this.Axis.Scale is CategoryScale && this.Presenter.IsScaleZoomed)
                this.Clip = (Geometry)new RectangleGeometry()
                {
                    Rect = new Rect(new Point(0.0, 0.0), finalSize)
                };
            else
                this.Clip = (Geometry)null;
            if (this.LevelOffsets != null && this.LevelOffsets.Count > 0)
                return base.ArrangeOverride(finalSize);
            return finalSize;
        }

        protected override double GetCenterCoordinate(UIElement element)
        {
            return XYLabelsPanel.GetCoordinateFrom(element) + (XYLabelsPanel.GetCoordinateTo(element) - XYLabelsPanel.GetCoordinateFrom(element)) / 2.0;
        }

        protected override double ElementOffset(UIElement element)
        {
            if (this.LevelOffsets.Count > 0)
                return this.LevelOffsets[XYLabelsPanel.GetLevel(element)] + XYLabelsPanel.GetLevelOffset(element);
            return 0.0;
        }

        protected override void ArrangeChild(UIElement child, Rect rect, Size finalSize)
        {
            AxisLabelControl control = child as AxisLabelControl;
            if (control != null)
            {
                Rect rect1 = RectExtensions.Translate(rect, this.MeasureLabelAngleOffset(control));
                base.ArrangeChild(child, rect1, finalSize);
            }
            else
            {
                if (!(child is Line))
                    return;
                Line line = child as Line;
                if (line != null && line.Stretch == Stretch.Fill)
                {
                    int key = XYLabelsPanel.GetLevel(child) - 1;
                    double num = !this.LevelOffsets.ContainsKey(key) ? (this.Orientation == Orientation.Horizontal ? finalSize.Height : finalSize.Width) : this.LevelOffsets[key];
                    rect = this.Orientation != Orientation.Horizontal ? (this.Presenter.ActualLocation != Edge.Left ? new Rect(0.0, rect.Y, num, rect.Height) : new Rect(finalSize.Width - num, rect.Y, num, rect.Height)) : (this.Presenter.ActualLocation != Edge.Top ? new Rect(rect.X, 0.0, rect.Width, num) : new Rect(rect.X, finalSize.Height - num, rect.Width, num));
                }
                base.ArrangeChild(child, rect, finalSize);
            }
        }

        internal Point MeasureLabelAngleOffset(AxisLabelControl control)
        {
            return this.GetAngleOffsetBySize(control.GetLabelContentDesiredSize(), control.RotationAngle);
        }

        internal Point GetAngleOffsetBySize(Size internalConrolSize, double labelAngle)
        {
            Point point = new Point(0.0, 0.0);
            double num1 = this.Orientation == Orientation.Horizontal ? 90.0 - labelAngle : labelAngle;
            double num2 = Math.Abs(num1);
            if (DoubleHelper.EqualsWithPrecision(num2, 0.0, 1.0) || DoubleHelper.EqualsWithPrecision(num2, 90.0, 1.0) || DoubleHelper.EqualsWithPrecision(num2, 180.0, 1.0))
                return point;
            double num3 = num1 / 180.0 * Math.PI;
            double num4 = internalConrolSize.Width / 2.0 * Math.Cos(num3);
            double num5 = Math.Tan(num3) * num4;
            point = new Point(0.0, num5);
            if (this.Orientation == Orientation.Horizontal)
                point = num1 < 90.0 ? new Point(num5, 0.0) : new Point(-num5, 0.0);
            if (this.IsInverted)
                point = new Point(-point.X, -point.Y);
            return point;
        }

        protected override void Populate(double availableLength)
        {
            this._axisLabelsPool.ReleaseAll();
            this._axisLabelLinesPool.ReleaseAll();
            this.IsGrouped = false;
            try
            {
                if (!this.Axis.ShowLabels)
                    return;
                bool flag = this.Axis.Scale is NumericScale;
                List<double> list1 = new List<double>();
                List<LabelDefinition> list2 = new List<LabelDefinition>((IEnumerable<LabelDefinition>)Enumerable.OrderBy<LabelDefinition, int>(Enumerable.OfType<LabelDefinition>((IEnumerable)Enumerable.Where<ScaleElementDefinition>(this.Presenter.GetScaleElements(), (Func<ScaleElementDefinition, bool>)(p => p.Kind == ScaleElementKind.Label))), (Func<LabelDefinition, int>)(l => l.Level)));
                list2.Sort((Comparison<LabelDefinition>)((x, y) => x.Level.CompareTo(y.Level)));
                foreach (LabelDefinition labelDefinition in list2)
                {
                    foreach (ScalePosition s in Enumerable.Where<ScalePosition>(labelDefinition.Positions, (Func<ScalePosition, bool>)(p =>
                   {
                       if (p.Position >= 0.0)
                           return p.Position <= 1.0;
                       return false;
                   })))
                    {
                        AxisLabelControl axisLabelControl = this._axisLabelsPool.Get(this.Axis);
                        axisLabelControl.SetContent(labelDefinition.GetContent(s));
                        XYLabelsPanel.SetLevel((UIElement)axisLabelControl, labelDefinition.Level);
                        XYLabelsPanel.SetCoordinateFrom((UIElement)axisLabelControl, s.BucketMin ?? s.Position - 0.5);
                        XYLabelsPanel.SetCoordinateTo((UIElement)axisLabelControl, s.BucketMax ?? s.Position + 0.5);
                        if (list2.Count > 1 && !flag && s.BucketMin.HasValue && s.BucketMax.HasValue)
                        {
                            double num1 = s.BucketMin ?? 0.0;
                            double num2 = s.BucketMax ?? 0.0;
                            if (!list1.Contains(num1))
                            {
                                double? bucketMin = s.BucketMin;
                                if ((bucketMin.GetValueOrDefault() < 0.0 ? 0 : (bucketMin.HasValue ? 1 : 0)) != 0)
                                {
                                    Line line = this._axisLabelLinesPool.Get(this.Axis);
                                    XYLabelsPanel.SetLevel((UIElement)line, labelDefinition.Level);
                                    XYLabelsPanel.SetCoordinateFrom((UIElement)line, num1);
                                    XYLabelsPanel.SetCoordinateTo((UIElement)line, num1);
                                    list1.Add(num1);
                                }
                            }
                            if (!list1.Contains(num2))
                            {
                                double? bucketMax = s.BucketMax;
                                if ((bucketMax.GetValueOrDefault() > 1.0 ? 0 : (bucketMax.HasValue ? 1 : 0)) != 0)
                                {
                                    Line line = this._axisLabelLinesPool.Get(this.Axis);
                                    XYLabelsPanel.SetLevel((UIElement)line, labelDefinition.Level);
                                    XYLabelsPanel.SetCoordinateFrom((UIElement)line, num2);
                                    XYLabelsPanel.SetCoordinateTo((UIElement)line, num2);
                                    list1.Add(num2);
                                }
                            }
                        }
                        XYLabelsPanel.RefreshBindings((FrameworkElement)axisLabelControl);
                    }
                }
                this.IsGrouped = list1.Count > 1;
            }
            finally
            {
                this._axisLabelsPool.AdjustPoolSize();
                this._axisLabelLinesPool.AdjustPoolSize();
            }
        }

        private static void RefreshBindings(FrameworkElement element)
        {
            if (element == null)
                return;
            BindingExpression bindingExpression = element.GetBindingExpression(Control.FontSizeProperty);
            if (bindingExpression == null)
                return;
            Binding parentBinding = bindingExpression.ParentBinding;
            element.SetBinding(Control.FontSizeProperty, (BindingBase)parentBinding);
        }

        public void ResetRequiredMargin(bool resetScrollingInfo)
        {
            if (resetScrollingInfo)
                this.ResetAxisLabels();
            this._requiredMargin = new Thickness(0.0);
            this._maxLabelCount = double.MaxValue;
            this.InvalidateMeasure();
        }

        public void ScheduleUpdate()
        {
            this.Axis.ScheduleOneScaleUpdate = true;
        }

        protected class LabelMeasureResult
        {
            public double Offset { get; private set; }

            public double MarginLeft { get; private set; }

            public double MarginRight { get; private set; }

            public bool HasCollisions { get; private set; }

            public double Angle { get; private set; }

            public bool Staggered { get; private set; }

            public bool StaggeredCollisions { get; set; }

            public LabelMeasureResult(double offset, double marginLeft, double marginRight, bool hasCollisions, double angle, bool staggered)
            {
                this.Offset = offset;
                this.MarginLeft = marginLeft;
                this.MarginRight = marginRight;
                this.HasCollisions = hasCollisions;
                this.Angle = angle;
                this.Staggered = staggered;
                this.StaggeredCollisions = hasCollisions;
            }
        }

        protected class LabelMeasureInfo
        {
            private double _maxWordLength = double.NaN;
            private char[] _delimiters = new char[9]
            {
        '.',
        '?',
        '!',
        ' ',
        ';',
        ':',
        ',',
        '\n',
        '\r'
            };

            public XYLabelsPanel Parent { get; private set; }

            public IList<AxisLabelControl> Elements { get; private set; }

            public IList<Rect> ContentRectangles { get; private set; }

            public IList<Rect> ClientRectangles { get; private set; }

            public IList<Point> AngleOffsets { get; private set; }

            public int Level { get; private set; }

            public TextWrapping TextWrapFlagResult { get; private set; }

            public LabelMeasureInfo(XYLabelsPanel parent, int level, IList<AxisLabelControl> elements)
            {
                this.Parent = parent;
                this.Level = level;
                this.Elements = elements;
                this.ContentRectangles = (IList<Rect>)new List<Rect>();
                this.ClientRectangles = (IList<Rect>)new List<Rect>();
                this.AngleOffsets = (IList<Point>)new List<Point>();
                this.TextWrapFlagResult = TextWrapping.NoWrap;
            }

            internal void HideEachSecondLabel()
            {
                bool flag = false;
                foreach (AxisLabelControl axisLabelControl in (IEnumerable<AxisLabelControl>)this.Elements)
                {
                    if (axisLabelControl.Opacity != 0.0)
                    {
                        if (flag)
                        {
                            axisLabelControl.Opacity = 0.0;
                            flag = false;
                        }
                        else
                            flag = true;
                    }
                }
            }

            internal void HideAllLabels()
            {
                foreach (UIElement uiElement in (IEnumerable<AxisLabelControl>)this.Elements)
                    uiElement.Opacity = 0.0;
            }

            private double GetMaxWordLength()
            {
                if (!double.IsNaN(this._maxWordLength))
                    return this._maxWordLength;
                this._maxWordLength = double.NaN;
                int val1 = 1;
                foreach (AxisLabelControl axisLabelControl in (IEnumerable<AxisLabelControl>)this.Elements)
                {
                    if (!string.IsNullOrEmpty(axisLabelControl.Text))
                    {
                        string[] strArray = axisLabelControl.Text.Split(this._delimiters);
                        val1 = Math.Max(val1, strArray.Length);
                        foreach (string text in strArray)
                        {
                            double num = this.Parent.ElementWidth(this.Parent.MeasureLabelString(text));
                            if (num > (double.IsNaN(this._maxWordLength) ? 0.0 : this._maxWordLength))
                                this._maxWordLength = num;
                        }
                    }
                }
                if (val1 <= 1)
                    return double.NaN;
                return this._maxWordLength;
            }

            private double GetElementsDistance()
            {
                if (this.Elements.Count > 0)
                    return Math.Abs(this.Parent.Presenter.ConvertScaleToAxisUnits(XYLabelsPanel.GetCoordinateTo((UIElement)this.Elements[0]), this.Parent.AvailableSizeInMeasureCycle.Width) - this.Parent.Presenter.ConvertScaleToAxisUnits(XYLabelsPanel.GetCoordinateFrom((UIElement)this.Elements[0]), this.Parent.AvailableSizeInMeasureCycle.Width));
                return this.Parent.AvailableSizeInMeasureCycle.Width / 2.0;
            }

            private void EvaluateWrapFlag()
            {
                if (this.Parent.Axis.CanWordWrapLabels && this.Elements.Count > 0 && (this.Parent.IsGrouped || this.Parent.Axis.Scale is CategoryScale))
                {
                    double maxWordLength = this.GetMaxWordLength();
                    if (!double.IsNaN(maxWordLength) && maxWordLength < this.GetElementsDistance())
                    {
                        this.TextWrapFlagResult = TextWrapping.Wrap;
                        return;
                    }
                }
                this.TextWrapFlagResult = TextWrapping.NoWrap;
            }

            private void PrepareLabels(double angle)
            {
                this.ContentRectangles.Clear();
                this.ClientRectangles.Clear();
                this.AngleOffsets.Clear();
                Size size = this.Parent.MeasureLabelString("M...");
                foreach (AxisLabelControl child in (IEnumerable<AxisLabelControl>)this.Elements)
                {
                    if (child.Opacity != 0.0)
                    {
                        child.RotationAngle = angle;
                        child.TextWrapping = this.TextWrapFlagResult;
                        child.MaxWidth = double.PositiveInfinity;
                        child.MaxHeight = double.PositiveInfinity;
                        child.SetLabelContentBounds(double.PositiveInfinity, double.PositiveInfinity);
                        if (this.TextWrapFlagResult == TextWrapping.Wrap)
                        {
                            if (this.Parent.Orientation == Orientation.Horizontal)
                                child.MaxWidth = this.GetElementsDistance();
                            else
                                child.MaxHeight = this.GetElementsDistance();
                            child.TextAlignment = this.Parent.Presenter.ActualLocation != Edge.Right ? (this.Parent.Presenter.ActualLocation != Edge.Left ? TextAlignment.Center : TextAlignment.Right) : TextAlignment.Left;
                        }
                        else
                            child.TextAlignment = this.Parent.Presenter.ActualLocation == Edge.Top || this.Parent.Presenter.ActualLocation == Edge.Right ? TextAlignment.Left : TextAlignment.Right;
                        XYLabelsPanel.SetLevelOffset((UIElement)child, 0.0);
                        child.Measure(XYLabelsPanel.TotalSize);
                        Rect contentRectangle = this.Parent.GetChildContentRectangle(child, 1.0);
                        Rect childClientRectangle = this.Parent.GetChildClientRectangle(child, 1.0);
                        double num1 = this.Parent.Presenter.ActualOrientation == Orientation.Vertical ? angle + 90.0 : angle;
                        double height = childClientRectangle.Height;
                        double d = Math.Max(0.0, this.Parent.AvailableSizeInMeasureCycle.Height - ValueHelper.Height(this.Parent.MeasureLabelPading) - (this.Parent.GetBaseLabelsOffset() - 1.0) * 2.0);
                        double num2 = num1 / 180.0 * Math.PI;
                        if (!DoubleHelper.EqualsWithPrecision(num1, 0.0) && !DoubleHelper.EqualsWithPrecision(Math.Abs(num1), 90.0))
                            height += Math.Cos(num2) * contentRectangle.Height;
                        if (height > d)
                        {
                            if (DoubleHelper.EqualsWithPrecision(num1, 0.0))
                                child.MaxHeight = Math.Floor(d);
                            else if (DoubleHelper.EqualsWithPrecision(Math.Abs(num1), 90.0))
                            {
                                child.MaxWidth = Math.Floor(d);
                            }
                            else
                            {
                                double num3 = (d - height) / Math.Sin(num2);
                                child.SetLabelContentBounds(Math.Max(size.Width, contentRectangle.Width - num3), double.PositiveInfinity);
                            }
                            child.Measure(XYLabelsPanel.TotalSize);
                            contentRectangle = this.Parent.GetChildContentRectangle(child, 1.0);
                            childClientRectangle = this.Parent.GetChildClientRectangle(child, 1.0);
                        }
                        Point point = this.Parent.Orientation != Orientation.Horizontal ? new Point(this.Parent.GetAngleOffsetBySize(new Size(contentRectangle.Height, contentRectangle.Width), angle).Y, 0.0) : new Point(this.Parent.GetAngleOffsetBySize(new Size(contentRectangle.Width, contentRectangle.Height), angle).X, 0.0);
                        this.ContentRectangles.Add(contentRectangle);
                        this.ClientRectangles.Add(childClientRectangle);
                        this.AngleOffsets.Add(point);
                    }
                }
            }

            private List<Rect> AdjustRectanglesToAvailableLength(IList<Rect> rects, IList<Point> angles, double availableLenght, double gap)
            {
                if (angles != null)
                    return Enumerable.ToList<Rect>(Enumerable.Zip<Rect, Point, Rect>((IEnumerable<Rect>)rects, (IEnumerable<Point>)angles, (Func<Rect, Point, Rect>)((r, a) => RectExtensions.Round(RectExtensions.Expand(RectExtensions.Translate(RectExtensions.Translate(r, (r.Left + r.Width / 2.0) * availableLenght, 0.0), a), gap, gap)))));
                return Enumerable.ToList<Rect>(Enumerable.Select<Rect, Rect>((IEnumerable<Rect>)rects, (Func<Rect, Rect>)(r => RectExtensions.Expand(RectExtensions.Translate(r, (r.Left + r.Width / 2.0) * availableLenght, 0.0), gap, gap))));
            }

            private bool HasCollisions(IList<Rect> adjustedRects, bool rotated)
            {
                if (rotated)
                    return Enumerable.Any<bool>(Enumerable.Zip<Rect, Rect, bool>((IEnumerable<Rect>)adjustedRects, Enumerable.Skip<Rect>((IEnumerable<Rect>)adjustedRects, 1), (Func<Rect, Rect, bool>)((prev, next) => prev.IntersectsWith(next))), (Func<bool, bool>)(intersect => intersect));
                for (int index1 = 0; index1 < adjustedRects.Count; ++index1)
                {
                    Rect rect1 = XYLabelsPanel.LabelMeasureInfo.Shrink(adjustedRects[index1], 1.0);
                    for (int index2 = index1 + 1; index2 < adjustedRects.Count; ++index2)
                    {
                        Rect rect2 = XYLabelsPanel.LabelMeasureInfo.Shrink(adjustedRects[index2], 1.0);
                        if (rect1.IntersectsWith(rect2))
                            return true;
                    }
                }
                return false;
            }

            private static Rect Shrink(Rect rect, double delta)
            {
                double num = delta + delta;
                return new Rect(rect.Width > num ? rect.X + delta : rect.X - delta, rect.Height > num ? rect.Y + delta : rect.Y - delta, rect.Width > num ? rect.Width - num : rect.Width + num, rect.Height > num ? rect.Height - num : rect.Height + num);
            }

            public XYLabelsPanel.LabelMeasureResult Measure(double angle, bool staggered, double gap)
            {
                return this.Measure(angle, staggered, true, gap);
            }

            public XYLabelsPanel.LabelMeasureResult Measure(double angle, bool staggered)
            {
                double gap = this.Parent.Presenter.ActualOrientation != Orientation.Horizontal ? (angle == 0.0 ? 0.0 : this.Parent.MinimumDistanceBetweenChildren) : (angle != 0.0 ? 0.0 : this.Parent.MinimumDistanceBetweenChildren);
                return this.Measure(angle, staggered, true, gap);
            }

            public XYLabelsPanel.LabelMeasureResult Measure(double angle, bool staggered, bool tryLabelWrap, double gap)
            {
                double offset = 0.0;
                double marginLeft = 0.0;
                double marginRight = 0.0;
                bool hasCollisions = false;
                bool flag = false;
                double availableLenght = this.Parent.AvailableSizeInMeasureCycle.Width;
                if (staggered)
                {
                    List<AxisLabelControl> firstLevel = new List<AxisLabelControl>();
                    List<AxisLabelControl> secondLevel = new List<AxisLabelControl>();
                    EnumerableFunctions.ForEachWithIndex<AxisLabelControl>((IEnumerable<AxisLabelControl>)this.Elements, (Action<AxisLabelControl, int>)((label, index) =>
                  {
                      if (index % 2 > 0)
                          secondLevel.Add(label);
                      else
                          firstLevel.Add(label);
                  }));
                    XYLabelsPanel.LabelMeasureInfo labelMeasureInfo1 = new XYLabelsPanel.LabelMeasureInfo(this.Parent, this.Level, (IList<AxisLabelControl>)firstLevel);
                    XYLabelsPanel.LabelMeasureInfo labelMeasureInfo2 = new XYLabelsPanel.LabelMeasureInfo(this.Parent, this.Level, (IList<AxisLabelControl>)secondLevel);
                    XYLabelsPanel.LabelMeasureResult firstInfo = labelMeasureInfo1.Measure(0.0, false, false, gap * 2.0);
                    XYLabelsPanel.LabelMeasureResult labelMeasureResult = labelMeasureInfo2.Measure(0.0, false, false, gap * 2.0);
                    secondLevel.ForEach((Action<AxisLabelControl>)(label => XYLabelsPanel.SetLevelOffset((UIElement)label, firstInfo.Offset)));
                    offset = firstInfo.Offset + labelMeasureResult.Offset;
                    marginLeft = Math.Max(firstInfo.MarginLeft, labelMeasureResult.MarginLeft);
                    marginRight = Math.Max(firstInfo.MarginRight, labelMeasureResult.MarginRight);
                    hasCollisions = firstInfo.HasCollisions || labelMeasureResult.HasCollisions || firstInfo.StaggeredCollisions || labelMeasureResult.StaggeredCollisions || marginRight + marginLeft > availableLenght * 0.35;
                    angle = 0.0;
                }
                else
                {
                    if (angle == 0.0 && tryLabelWrap)
                        this.EvaluateWrapFlag();
                    this.PrepareLabels(angle);
                    if (this.ClientRectangles.Count > 0)
                    {
                        offset = Enumerable.Max(Enumerable.Select<Rect, double>((IEnumerable<Rect>)this.ClientRectangles, (Func<Rect, double>)(r => r.Height)));
                        List<Rect> list1 = this.AdjustRectanglesToAvailableLength(this.ClientRectangles, this.AngleOffsets, availableLenght, gap);
                        if (!this.Parent.Presenter.IsScaleZoomed)
                        {
                            list1.ForEach((Action<Rect>)(r =>
                           {
                               marginLeft = Math.Max(Math.Max(0.0, marginLeft), -r.Left);
                               marginRight = Math.Max(Math.Max(0.0, marginRight), r.Right - availableLenght);
                           }));
                            marginLeft = Math.Ceiling(double.IsNaN(marginLeft) ? 0.0 : marginLeft);
                            marginRight = Math.Ceiling(double.IsNaN(marginRight) ? 0.0 : marginRight);
                        }
                        else if (!(this.Parent.Axis.Scale is CategoryScale))
                        {
                            list1.ForEach((Action<Rect>)(r => marginLeft = Math.Max(Math.Max(0.0, marginLeft), r.Width / 2.0)));
                            marginLeft = Math.Ceiling(double.IsNaN(marginLeft) ? 0.0 : marginLeft);
                            marginRight = marginLeft;
                        }
                        double availableLenght1 = Math.Max(0.0, availableLenght - (marginRight + marginLeft));
                        List<Rect> list2;
                        if (!DoubleHelper.EqualsWithPrecision(Math.Abs(angle % 90.0), 0.0))
                        {
                            List<Rect> list3 = this.AdjustRectanglesToAvailableLength(this.ContentRectangles, (IList<Point>)null, availableLenght1, gap);
                            RotateTransform transform = new RotateTransform()
                            {
                                Angle = angle
                            };
                            list2 = Enumerable.ToList<Rect>(Enumerable.Select<Rect, Rect>((IEnumerable<Rect>)list3, (Func<Rect, Rect>)(r =>
                          {
                              Point point = transform.Transform(new Point(r.X + r.Width / 2.0, r.Y + r.Height / 2.0));
                              return new Rect(point.X, point.Y, r.Width, r.Height);
                          })));
                        }
                        else
                            list2 = this.AdjustRectanglesToAvailableLength(this.ClientRectangles, this.AngleOffsets, availableLenght1, gap);
                        hasCollisions = this.TextWrapFlagResult != TextWrapping.Wrap && this.HasCollisions((IList<Rect>)list2, angle != 0.0);
                        if (!DoubleHelper.EqualsWithPrecision(Math.Abs(angle % 90.0), 0.0))
                            hasCollisions = hasCollisions || marginRight + marginLeft > availableLenght * 0.35;
                        flag = hasCollisions || angle != 0.0;
                        if (!flag && list2.Count > 1)
                        {
                            double distance = Enumerable.Min(Enumerable.Zip<Rect, Rect, double>((IEnumerable<Rect>)list2, Enumerable.Skip<Rect>((IEnumerable<Rect>)list2, 1), (Func<Rect, Rect, double>)((prev, next) => Math.Abs(next.X + next.Width / 2.0 - (prev.X + prev.Width / 2.0)))));
                            flag = Enumerable.Any<Rect>((IEnumerable<Rect>)list2, (Func<Rect, bool>)(r => r.Width > distance));
                        }
                    }
                }
                return new XYLabelsPanel.LabelMeasureResult(offset, marginLeft, marginRight, hasCollisions, angle, staggered)
                {
                    StaggeredCollisions = flag
                };
            }
        }
    }
}
