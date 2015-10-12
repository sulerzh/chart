using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal abstract class XYSeriesPresenter : SeriesPresenter
    {
        private LabelVisibilityManager _labelVisibilityManager;

        internal bool IsSimplifiedRenderingModeCheckRequired { get; set; }

        internal XYSeries Series
        {
            get
            {
                return (XYSeries)base.Series;
            }
        }

        internal XYChartArea XYChartArea
        {
            get
            {
                return (XYChartArea)this.Series.ChartArea;
            }
        }

        public LabelVisibilityManager LabelVisibilityManager
        {
            get
            {
                if (this._labelVisibilityManager == null && this.ChartArea != null)
                    this._labelVisibilityManager = (LabelVisibilityManager)this.ChartArea.SingletonRegistry.GetSingleton((object)"LabelVisibilityManager", (Func<object>)(() => (object)new LabelVisibilityManager(this.XYChartArea)), (Action<object>)null);
                return this._labelVisibilityManager;
            }
        }

        protected IEnumerable<XYSeries> ClusterSeries
        {
            get
            {
                return Enumerable.Where<XYSeries>((IEnumerable<XYSeries>)this.XYChartArea.Series, (Func<XYSeries, bool>)(item => item.ClusterKey == this.Series.ClusterKey));
            }
        }

        protected XYSeriesPresenter(XYSeries series)
          : base((Semantic.Reporting.Windows.Chart.Internal.Series)series)
        {
            this.IsSimplifiedRenderingModeCheckRequired = true;
        }

        internal virtual void OnXScaleChanged()
        {
            DateTime now = DateTime.Now;
            if (this.ChartArea == null)
                return;
            this.ChartArea.UpdateSession.BeginUpdates();
            bool flag = true;
            foreach (XYDataPoint xyDataPoint in (Collection<DataPoint>)this.Series.DataPoints)
            {
                xyDataPoint.XValueInScaleUnitsWithoutAnimation = this.Series.XAxis.Scale.ProjectDataValue(xyDataPoint.XValue);
                string storyboardKey = DependencyPropertyAnimationHelper.GetStoryboardKey("XValueInScaleUnits");
                StoryboardInfo si = (StoryboardInfo)null;
                if (xyDataPoint.Storyboards.TryGetValue(storyboardKey, out si) && si.Storyboard.Children.Count > 0)
                {
                    DoubleAnimation doubleAnimation = si.Storyboard.Children[0] as DoubleAnimation;
                    if (doubleAnimation != null && xyDataPoint.XValue != null)
                    {
                        double? to = doubleAnimation.To;
                        double xvalueInScaleUnits = xyDataPoint.XValueInScaleUnits;
                        if ((to.GetValueOrDefault() != xvalueInScaleUnits ? 1 : (!to.HasValue ? 1 : 0)) != 0)
                        {
                            if (!this.IsAnimationDirectionChanged(si, doubleAnimation.To.Value, xyDataPoint.XValueInScaleUnitsWithoutAnimation))
                                flag = false;
                            doubleAnimation.To = new double?(xyDataPoint.XValueInScaleUnitsWithoutAnimation);
                            continue;
                        }
                    }
                }
                xyDataPoint.XValueInScaleUnits = xyDataPoint.XValueInScaleUnitsWithoutAnimation;
            }
            if (!flag)
            {
                foreach (XYDataPoint dataPoint in (Collection<DataPoint>)this.Series.DataPoints)
                    this.SkipToFillValueAnimation(dataPoint, "XValueInScaleUnits");
            }
            if (this.Series.LabelVisibility == Visibility.Visible)
            {
                LabelVisibilityManager manager = this.LabelVisibilityManager;
                manager.InvalidateXIntervals();
                this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => manager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
            }
            this.ChartArea.UpdateSession.EndUpdates();
        }

        internal virtual void OnXScaleViewChanged(Range<IComparable> oldRange, Range<IComparable> newRange)
        {
            if (!this.Series.XAxis.Scale.IsScrolling)
                this.IsSimplifiedRenderingModeCheckRequired = true;
            if (this.Series.LabelVisibility != Visibility.Visible)
                return;
            this.LabelVisibilityManager.InvalidateXIntervals();
            this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.LabelVisibilityManager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
        }

        internal virtual void OnYScaleChanged()
        {
            if (this.ChartArea == null)
                return;
            this.ChartArea.UpdateSession.BeginUpdates();
            bool flag = true;
            foreach (XYDataPoint xyDataPoint in (Collection<DataPoint>)this.Series.DataPoints)
            {
                xyDataPoint.YValueInScaleUnitsWithoutAnimation = this.Series.YAxis.Scale.ProjectDataValue(xyDataPoint.YValue);
                string storyboardKey = DependencyPropertyAnimationHelper.GetStoryboardKey("YValueInScaleUnits");
                StoryboardInfo si = (StoryboardInfo)null;
                if (xyDataPoint.Storyboards.TryGetValue(storyboardKey, out si) && si.Storyboard.Children.Count > 0)
                {
                    DoubleAnimation doubleAnimation = si.Storyboard.Children[0] as DoubleAnimation;
                    if (doubleAnimation != null && xyDataPoint.YValue != null)
                    {
                        double? to = doubleAnimation.To;
                        double yvalueInScaleUnits = xyDataPoint.YValueInScaleUnits;
                        if ((to.GetValueOrDefault() != yvalueInScaleUnits ? 1 : (!to.HasValue ? 1 : 0)) != 0)
                        {
                            if (!this.IsAnimationDirectionChanged(si, doubleAnimation.To.Value, xyDataPoint.YValueInScaleUnitsWithoutAnimation))
                                flag = false;
                            doubleAnimation.To = new double?(xyDataPoint.YValueInScaleUnitsWithoutAnimation);
                            continue;
                        }
                    }
                }
                xyDataPoint.YValueInScaleUnits = xyDataPoint.YValueInScaleUnitsWithoutAnimation;
            }
            if (!flag)
            {
                foreach (XYDataPoint dataPoint in (Collection<DataPoint>)this.Series.DataPoints)
                    this.SkipToFillValueAnimation(dataPoint, "YValueInScaleUnits");
            }
            if (this.Series.LabelVisibility == Visibility.Visible)
            {
                this.LabelVisibilityManager.InvalidateYIntervals();
                this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.LabelVisibilityManager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
            }
            this.ChartArea.UpdateSession.EndUpdates();
        }

        private bool IsAnimationDirectionChanged(StoryboardInfo si, double oldAnimateTo, double newAnimateTo)
        {
            if (si.AnimateFrom is double)
            {
                double num = (double)si.AnimateFrom;
                if (num > oldAnimateTo && num > newAnimateTo || num < oldAnimateTo && num < newAnimateTo)
                    return true;
            }
            return false;
        }

        internal virtual void OnYScaleViewChanged(Range<IComparable> oldRange, Range<IComparable> newRange)
        {
            if (!this.Series.XAxis.Scale.IsScrolling)
                this.IsSimplifiedRenderingModeCheckRequired = true;
            if (this.Series.LabelVisibility != Visibility.Visible)
                return;
            this.LabelVisibilityManager.InvalidateYIntervals();
            this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.LabelVisibilityManager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
        }

        protected override void OnSeriesDataPointValueChanged(DataPoint dataPoint, string valueName, object oldValue, object newValue)
        {
            switch (valueName)
            {
                case "XValue":
                    this.ChangeDataPointXValue(dataPoint as XYDataPoint, newValue);
                    break;
                case "YValue":
                    this.ChangeDataPointYValue(dataPoint as XYDataPoint, newValue);
                    break;
                case "XValueInScaleUnitsWithoutAnimation":
                case "YValueInScaleUnitsWithoutAnimation":
                    if (this.ChartArea == null || !this.ChartArea.IsTemplateApplied || this.Series.LabelVisibility != Visibility.Visible)
                        break;
                    this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.LabelVisibilityManager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
                    break;
                case "XValueInScaleUnits":
                case "YValueInScaleUnits":
                    if (this.ChartArea == null || !this.ChartArea.IsTemplateApplied)
                        break;
                    Tuple<Semantic.Reporting.Windows.Chart.Internal.Series, string> tuple = new Tuple<Semantic.Reporting.Windows.Chart.Internal.Series, string>((Semantic.Reporting.Windows.Chart.Internal.Series)this.Series, "__UpdateDataPointVisibility__");
                    if (this.ChartArea.UpdateSession.IsUpdating)
                    {
                        this.ChartArea.UpdateSession.ExecuteOnceBeforeUpdating((Action)(() => this.UpdateDataPointVisibility()), (object)tuple);
                        break;
                    }
                    this.ChartArea.UpdateSession.PostExecuteOnceOnUIThread((Action)(() => this.UpdateDataPointVisibility()), (object)tuple);
                    break;
                default:
                    base.OnSeriesDataPointValueChanged(dataPoint, valueName, oldValue, newValue);
                    break;
            }
        }

        private void ChangeDataPointXValue(XYDataPoint dataPoint, object newValue)
        {
            if (dataPoint == null)
                return;
            double xvalueInScaleUnits = dataPoint.XValueInScaleUnits;
            double d = this.Series.XAxis.Scale == null ? double.NaN : this.Series.XAxis.Scale.ProjectDataValue(newValue);
            dataPoint.XValueInScaleUnitsWithoutAnimation = d;
            if (!double.IsNaN(xvalueInScaleUnits) && !double.IsNaN(d) && (this.IsSeriesAnimationEnabled && this.ChartArea != null))
            {
                DependencyPropertyAnimationHelper.BeginAnimation(this.ChartArea, "XValueInScaleUnits", (object)xvalueInScaleUnits, (object)d, (Action<object, object>)((value1, value2) =>
             {
                 if (double.IsNaN(dataPoint.XValueInScaleUnitsWithoutAnimation))
                     return;
                 dataPoint.XValueInScaleUnits = (double)value2;
             }), dataPoint.Storyboards, this.Series.ActualTransitionDuration, this.Series.ActualTransitionEasingFunction);
            }
            else
            {
                this.SkipToFillValueAnimation(dataPoint, "XValueInScaleUnits");
                dataPoint.XValueInScaleUnits = d;
            }
        }

        private void ChangeDataPointYValue(XYDataPoint dataPoint, object newValue)
        {
            if (dataPoint == null)
                return;
            double yvalueInScaleUnits = dataPoint.YValueInScaleUnits;
            double d = this.Series.YAxis.Scale == null ? double.NaN : this.Series.YAxis.Scale.ProjectDataValue(newValue);
            dataPoint.YValueInScaleUnitsWithoutAnimation = d;
            if (!double.IsNaN(d) && !double.IsNaN(yvalueInScaleUnits) && (this.IsSeriesAnimationEnabled && this.ChartArea != null))
            {
                DependencyPropertyAnimationHelper.BeginAnimation(this.ChartArea, "YValueInScaleUnits", (object)yvalueInScaleUnits, (object)d, (Action<object, object>)((value1, value2) =>
             {
                 if (double.IsNaN(dataPoint.YValueInScaleUnitsWithoutAnimation))
                     return;
                 dataPoint.YValueInScaleUnits = (double)value2;
             }), dataPoint.Storyboards, this.Series.ActualTransitionDuration, this.Series.ActualTransitionEasingFunction);
            }
            else
            {
                this.SkipToFillValueAnimation(dataPoint, "YValueInScaleUnits");
                dataPoint.YValueInScaleUnits = d;
            }
        }

        internal void SkipToFillValueAnimation(XYDataPoint dataPoint, string propertyName)
        {
            string storyboardKey = DependencyPropertyAnimationHelper.GetStoryboardKey(propertyName);
            StoryboardInfo storyboardInfo = (StoryboardInfo)null;
            if (!dataPoint.Storyboards.TryGetValue(storyboardKey, out storyboardInfo) || storyboardInfo.Storyboard.Children.Count <= 0)
                return;
            storyboardInfo.Storyboard.SkipToFill();
        }

        protected override void OnSeriesModelPropertyChanged(string propertyName)
        {
            base.OnSeriesModelPropertyChanged(propertyName);
            switch (propertyName)
            {
                case "LabelVisibility":
                    if (this.LabelVisibilityManager == null)
                        break;
                    this.UpdateDataPointLabelVisibility();
                    break;
            }
        }

        public override void OnSeriesRemoved()
        {
            base.OnSeriesRemoved();
            if (this.ChartArea == null || !this.ChartArea.IsTemplateApplied || this.Series.LabelVisibility != Visibility.Visible)
                return;
            this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.LabelVisibilityManager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
        }

        internal override void OnDataPointRemoved(DataPoint dataPoint, bool useHidingAnimation)
        {
            base.OnDataPointRemoved(dataPoint, useHidingAnimation);
            if (this.ChartArea == null || !this.ChartArea.IsTemplateApplied || this.Series.LabelVisibility != Visibility.Visible)
                return;
            this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() =>
           {
               if (this.LabelVisibilityManager == null)
                   return;
               this.LabelVisibilityManager.UpdateDataPointLabelVisibility();
           }), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
        }

        internal override void OnDataPointAdded(DataPoint dataPoint, bool useShowingAnimation)
        {
            XYDataPoint xyDataPoint = dataPoint as XYDataPoint;
            if (xyDataPoint == null)
                return;
            xyDataPoint.XValueInScaleUnits = this.Series.XAxis.Scale.ProjectDataValue(xyDataPoint.XValue);
            xyDataPoint.YValueInScaleUnits = this.Series.YAxis.Scale.ProjectDataValue(xyDataPoint.YValue);
            xyDataPoint.XValueInScaleUnitsWithoutAnimation = xyDataPoint.XValueInScaleUnits;
            xyDataPoint.YValueInScaleUnitsWithoutAnimation = xyDataPoint.YValueInScaleUnits;
            xyDataPoint.IsNewlyAdded = useShowingAnimation;
        }

        protected override bool StartDataPointShowingAnimation(DataPoint dataPoint)
        {
            StoryboardGroup storyboardGroup = new StoryboardGroup();
            if (!this.IsSeriesAnimationEnabled || (IAppearanceProvider)dataPoint == null)
                return false;
            double actualOpacity = dataPoint.ActualOpacity;
            dataPoint.ActualOpacity = 0.0;
            storyboardGroup.Children.Add(DependencyPropertyAnimationHelper.CreateAnimation(this.ChartArea, "ActualOpacity", (object)0.0, (object)actualOpacity, (Action<object, object>)((value1, value2) => dataPoint.ActualOpacity = (double)value2), dataPoint.Storyboards, this.Series.ActualTransitionDuration, this.Series.ActualTransitionEasingFunction));
            storyboardGroup.Completed += (EventHandler)((source, args) =>
           {
               if (dataPoint.ViewState != DataPointViewState.Showing)
                   return;
               dataPoint.ViewState = DataPointViewState.Normal;
           });
            storyboardGroup.Begin();
            return true;
        }

        protected override bool StartDataPointHidingAnimation(DataPoint dataPoint)
        {
            return false;
        }

        public virtual AxisMargin GetSeriesMarginInfo(AutoBool isAxisMarginVisible)
        {
            if (isAxisMarginVisible != AutoBool.True)
                return AxisMargin.Empty;
            double scaleUnitsFromValues = this.Series.XAxis.AxisPresenter.GetClusterSizeInScaleUnitsFromValues(SeriesPresenter.GetXValuesFromSeries(this.ClusterSeries));
            return new AxisMargin(scaleUnitsFromValues, scaleUnitsFromValues);
        }

        internal override void UpdateDataPointVisibility()
        {
            int index1 = 0;
            DataPointViewState[] dataPointViewStateArray = new DataPointViewState[this.Series.DataPoints.Count];
            foreach (DataPoint dataPoint in (Collection<DataPoint>)this.Series.DataPoints)
            {
                DataPointViewState dataPointViewState = DataPointViewState.Hidden;
                dataPoint.IsVisible = false;
                XYDataPoint dataPointXY = dataPoint as XYDataPoint;
                if (dataPointXY != null && this.ChartArea != null && (this.ChartArea.IsTemplateApplied && this.CanGraph(dataPointXY)) && this.Series.Visibility == Visibility.Visible)
                {
                    dataPoint.IsVisible = true;
                    dataPointViewState = dataPoint.IsNewlyAdded ? DataPointViewState.Showing : DataPointViewState.Normal;
                    this.ChartArea.UpdateSession.Update((IUpdatable)dataPoint);
                }
                dataPointViewStateArray[index1] = dataPointViewState;
                ++index1;
            }
            this.CheckSimplifiedRenderingMode();
            int index2 = 0;
            foreach (DataPoint dataPoint in (Collection<DataPoint>)this.Series.DataPoints)
            {
                this.SetDataPointViewState(dataPoint, dataPointViewStateArray[index2]);
                dataPoint.IsNewlyAdded = false;
                ++index2;
            }
        }

        internal virtual bool CanGraph(XYDataPoint dataPointXY)
        {
            if (ValueHelper.CanGraph(dataPointXY.XValueInScaleUnits) && ValueHelper.CanGraph(dataPointXY.YValueInScaleUnits) && (DoubleHelper.GreaterOrEqualWithPrecision(dataPointXY.XValueInScaleUnits, 0.0) && DoubleHelper.LessOrEqualWithPrecision(dataPointXY.XValueInScaleUnits, 1.0)) && DoubleHelper.GreaterOrEqualWithPrecision(dataPointXY.YValueInScaleUnits, 0.0))
                return DoubleHelper.LessOrEqualWithPrecision(dataPointXY.YValueInScaleUnits, 1.0);
            return false;
        }

        internal void SetDataPointViewState(DataPoint dataPoint, DataPointViewState newViewState)
        {
            if (newViewState == DataPointViewState.Normal || newViewState == DataPointViewState.Showing)
            {
                if (dataPoint.ViewState != DataPointViewState.Hidden)
                    return;
                dataPoint.ViewState = newViewState;
            }
            else
            {
                if (newViewState != DataPointViewState.Hidden && newViewState != DataPointViewState.Hiding || (dataPoint.ViewState == DataPointViewState.Hidden || dataPoint.ViewState == DataPointViewState.Hiding))
                    return;
                dataPoint.ViewState = newViewState;
            }
        }

        internal virtual bool CheckSimplifiedRenderingMode()
        {
            if (this.ChartArea != null && !this.ChartArea.UpdateSession.IsExecutingAfterUpdatingStarted && this.IsSimplifiedRenderingModeCheckRequired)
            {
                this.IsSimplifiedRenderingModeCheckRequired = false;
                this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.IsSimplifiedRenderingModeCheckRequired = true), (object)new Tuple<Semantic.Reporting.Windows.Chart.Internal.Series, string>((Semantic.Reporting.Windows.Chart.Internal.Series)this.Series, "__IsSimplifiedRenderingModeCheckRequired__"), (string)null);
                bool flag = this.ShouldSimplifiedRenderingModeBeEnabled();
                if (flag != this.IsSimplifiedRenderingModeEnabled)
                {
                    this.IsSimplifiedRenderingModeEnabled = flag;
                    return true;
                }
            }
            return false;
        }

        protected override void UpdateView()
        {
            if (this.Series.ChartArea == null)
                return;
            this.Series.ChartArea.UpdateSession.BeginUpdates();
            base.UpdateView();
            this.UpdateDataPointLabelVisibility();
            this.Series.ChartArea.UpdateSession.EndUpdates();
        }

        private void UpdateDataPointLabelVisibility()
        {
            this.LabelVisibilityManager.InvalidateXIntervals();
            this.LabelVisibilityManager.InvalidateYIntervals();
            this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.LabelVisibilityManager.UpdateDataPointLabelVisibility()), (object)"LabelVisibilityManager_UpdateDataPointLabelVisibility", (string)null);
        }
    }
}
