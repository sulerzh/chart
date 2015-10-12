using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class LineSeriesPresenter : XYSeriesPresenter
    {
        private HashSet<DataPoint> _dataPointsToForceVisibility = new HashSet<DataPoint>();
        internal const string UpdateLinePointsActionKey = "__UpdateLinePoints__";
        private const double MaximumScaleUnitValue = 100.0;
        private Point _mousePosition;
        private ToolTip _tooltip;
        private PolylineControl _polylineControl;

        internal PolylineControl PolylineControl
        {
            get
            {
                if (this._polylineControl == null && this.ChartArea != null)
                {
                    this._polylineControl = new PolylineControl();
                    this._tooltip = new ToolTip();
                    this._tooltip.Opened += new RoutedEventHandler(this.Tooltip_Opened);
                    ToolTipService.SetToolTip((DependencyObject)this._polylineControl, (object)this._tooltip);
                    this.UpdateToolTipStyle();
                    this.RootPanel.MouseMove += new MouseEventHandler(this.RootPanel_MouseMove);
                    this.RootPanel.Children.Add((UIElement)this._polylineControl);
                }
                return this._polylineControl;
            }
        }

        public LineSeriesPresenter(XYSeries series)
          : base(series)
        {
            this.IsRootPanelClipped = true;
        }

        internal override SeriesMarkerPresenter CreateMarkerPresenter()
        {
            return new SeriesMarkerPresenter((SeriesPresenter)this);
        }

        internal override SeriesLabelPresenter CreateLabelPresenter()
        {
            return (SeriesLabelPresenter)new LineSeriesLabelPresenter((SeriesPresenter)this);
        }

        protected override void OnRemoved()
        {
            if (this._tooltip != null)
            {
                this._tooltip.Opened -= new RoutedEventHandler(this.Tooltip_Opened);
                this.RootPanel.MouseMove -= new MouseEventHandler(this.RootPanel_MouseMove);
            }
            base.OnRemoved();
        }

        protected override FrameworkElement CreateViewElement(DataPoint dataPoint)
        {
            this.CheckSimplifiedRenderingMode();
            return (FrameworkElement)null;
        }

        protected override void RemoveView(DataPoint dataPoint)
        {
            if (this.ChartArea != null)
                this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.UpdateLinePoints()), (object)new Tuple<Series, string>((Series)this.Series, "__UpdateLinePoints__"), (string)null);
            base.RemoveView(dataPoint);
        }

        protected override void UpdateView(DataPoint dataPoint)
        {
            if (this.IsDataPointVisible(dataPoint))
            {
                XYDataPoint xyDataPoint = dataPoint as XYDataPoint;
                if (xyDataPoint.View != null)
                {
                    dataPoint.View.AnchorPoint = this.ChartArea.ConvertScaleToPlotCoordinate(this.Series.XAxis, this.Series.YAxis, xyDataPoint.XValueInScaleUnits, xyDataPoint.YValueInScaleUnits);
                    this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.UpdateLinePoints()), (object)new Tuple<Series, string>((Series)this.Series, "__UpdateLinePoints__"), (string)null);
                }
            }
            base.UpdateView(dataPoint);
        }

        protected override void BindViewToDataPoint(DataPoint dataPoint, FrameworkElement view, string valueName)
        {
            if (valueName == null || valueName == "Stroke" || (valueName == "StrokeThickness" || valueName == "StrokeDashType") || (valueName == "ActualEffect" || valueName == "Effect" || (valueName == "ActualOpacity" || valueName == "Opacity")))
                this.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.UpdateLinePoints()), (object)new Tuple<Series, string>((Series)this.Series, "__UpdateLinePoints__"), (string)null);
            DataPointView dataPointView = dataPoint != null ? dataPoint.View : (DataPointView)null;
            if (dataPointView == null)
                return;
            this.LabelPresenter.BindViewToDataPoint(dataPoint, (FrameworkElement)dataPointView.LabelView, valueName);
            this.MarkerPresenter.BindViewToDataPoint(dataPoint, dataPointView.MarkerView, valueName);
        }

        private void UpdateLinePoints()
        {
            if (this.ChartArea == null || !this.IsRootPanelVisible)
                return;
            DateTime now = DateTime.Now;
            PointCollection pointCollection = new PointCollection();
            Collection<IAppearanceProvider> collection = new Collection<IAppearanceProvider>();
            XYDataPoint xyDataPoint1 = (XYDataPoint)null;
            if (this.Series.Visibility == Visibility.Visible)
            {
                for (int index = 0; index < this.Series.DataPoints.Count; ++index)
                {
                    XYDataPoint xyDataPoint2 = this.Series.DataPoints[index] as XYDataPoint;
                    if (xyDataPoint2.View != null && this.IsDataPointVisible((DataPoint)xyDataPoint2) && (this.IsValidScaleUnitValue(xyDataPoint2.XValueInScaleUnits) && this.IsValidScaleUnitValue(xyDataPoint2.YValueInScaleUnits)))
                    {
                        if (xyDataPoint1 != null && xyDataPoint1.ActualIsEmpty)
                        {
                            collection.Add((IAppearanceProvider)xyDataPoint1);
                            pointCollection.Add(xyDataPoint2.View.AnchorPoint);
                            if (index < this.Series.DataPoints.Count - 1)
                            {
                                XYDataPoint xyDataPoint3 = this.Series.DataPoints[index + 1] as XYDataPoint;
                                if (xyDataPoint3.ActualIsEmpty)
                                {
                                    collection.Add((IAppearanceProvider)xyDataPoint3);
                                    pointCollection.Add(xyDataPoint2.View.AnchorPoint);
                                }
                            }
                        }
                        collection.Add((IAppearanceProvider)xyDataPoint2);
                        pointCollection.Add(xyDataPoint2.View.AnchorPoint);
                    }
                    xyDataPoint1 = xyDataPoint2;
                }
            }
            if (this.PolylineControl != null)
            {
                this.PolylineControl.Points = pointCollection;
                this.PolylineControl.Appearances = collection;
                this.PolylineControl.Update();
            }
            this.ChartArea.UpdateSession.AddCounter("LineSeriesPresenter.UpdateLinePoints", DateTime.Now - now);
        }

        private bool IsValidScaleUnitValue(double scaleUnitValue)
        {
            if (ValueHelper.CanGraph(scaleUnitValue) && scaleUnitValue < 100.0)
                return scaleUnitValue > -100.0;
            return false;
        }

        internal override void UpdateDataPointVisibility()
        {
            int index1 = 0;
            bool flag1 = true;
            bool flag2 = true;
            DataPointViewState[] dataPointViewStateArray = new DataPointViewState[this.Series.DataPoints.Count];
            foreach (DataPoint dataPoint1 in (Collection<DataPoint>)this.Series.DataPoints)
            {
                DataPointViewState dataPointViewState = DataPointViewState.Hidden;
                dataPoint1.IsVisible = false;
                XYDataPoint xyDataPoint = dataPoint1 as XYDataPoint;
                if (xyDataPoint != null && this.ChartArea != null && (this.ChartArea.IsTemplateApplied && ValueHelper.CanGraph(xyDataPoint.XValueInScaleUnits)) && ValueHelper.CanGraph(xyDataPoint.YValueInScaleUnits))
                {
                    if (DoubleHelper.GreaterOrEqualWithPrecision(xyDataPoint.XValueInScaleUnits, 0.0) && DoubleHelper.LessOrEqualWithPrecision(xyDataPoint.XValueInScaleUnits, 1.0) && (DoubleHelper.GreaterOrEqualWithPrecision(xyDataPoint.YValueInScaleUnits, 0.0) && DoubleHelper.LessOrEqualWithPrecision(xyDataPoint.YValueInScaleUnits, 1.0)))
                    {
                        if (this.Series.Visibility == Visibility.Visible)
                        {
                            flag1 = false;
                            dataPoint1.IsVisible = true;
                            if (!dataPoint1.IsNewlyAdded)
                                flag2 = false;
                            dataPointViewState = dataPoint1.IsNewlyAdded ? DataPointViewState.Showing : DataPointViewState.Normal;
                            if (index1 > 0)
                            {
                                DataPoint dataPoint2 = this.Series.DataPoints[index1 - 1];
                                if (dataPointViewStateArray[index1 - 1] == DataPointViewState.Hidden || dataPointViewStateArray[index1 - 1] == DataPointViewState.Hiding)
                                {
                                    dataPoint2.IsVisible = true;
                                    dataPointViewStateArray[index1 - 1] = dataPointViewState;
                                    this.ChartArea.UpdateSession.Update((IUpdatable)dataPoint2);
                                }
                            }
                            if (index1 < this.Series.DataPoints.Count - 1)
                            {
                                DataPoint dataPoint2 = this.Series.DataPoints[index1 + 1];
                                if (!this._dataPointsToForceVisibility.Contains(dataPoint2))
                                    this._dataPointsToForceVisibility.Add(dataPoint2);
                            }
                            this.ChartArea.UpdateSession.Update((IUpdatable)dataPoint1);
                        }
                    }
                    else if (this._dataPointsToForceVisibility.Contains(dataPoint1))
                    {
                        dataPoint1.IsVisible = true;
                        dataPointViewState = dataPoint1.IsNewlyAdded ? DataPointViewState.Showing : DataPointViewState.Normal;
                    }
                }
                if (this._dataPointsToForceVisibility.Contains(dataPoint1))
                    this._dataPointsToForceVisibility.Remove(dataPoint1);
                dataPointViewStateArray[index1] = dataPointViewState;
                ++index1;
            }
            this.IsSimplifiedRenderingModeCheckRequired = true;
            this.CheckSimplifiedRenderingMode();
            if (!flag2)
            {
                for (int index2 = 0; index2 < EnumerableFunctions.FastCount((IEnumerable)dataPointViewStateArray); ++index2)
                {
                    if (dataPointViewStateArray[index2] == DataPointViewState.Showing)
                        dataPointViewStateArray[index2] = DataPointViewState.Normal;
                }
            }
            if (flag1 && this.Series.DataPoints.Count > 0 && (this.ChartArea != null && this.ChartArea.IsTemplateApplied) && (DoubleHelper.LessWithPrecision(((XYDataPoint)this.Series.DataPoints[0]).XValueInScaleUnits, 0.0) && DoubleHelper.GreaterWithPrecision(((XYDataPoint)this.Series.DataPoints[this.Series.DataPoints.Count - 1]).XValueInScaleUnits, 1.0)))
            {
                int index2;
                for (index2 = 1; index2 < this.Series.DataPoints.Count - 2; ++index2)
                {
                    if (DoubleHelper.GreaterOrEqualWithPrecision((this.Series.DataPoints[index2] as XYDataPoint).XValueInScaleUnits, 0.0))
                    {
                        --index2;
                        break;
                    }
                }
                XYDataPoint xyDataPoint1 = this.Series.DataPoints[index2] as XYDataPoint;
                xyDataPoint1.IsVisible = true;
                dataPointViewStateArray[index2] = xyDataPoint1.IsNewlyAdded ? DataPointViewState.Showing : DataPointViewState.Normal;
                this.ChartArea.UpdateSession.Update((IUpdatable)xyDataPoint1);
                XYDataPoint xyDataPoint2 = this.Series.DataPoints[index2 + 1] as XYDataPoint;
                xyDataPoint2.IsVisible = true;
                dataPointViewStateArray[index2 + 1] = xyDataPoint2.IsNewlyAdded ? DataPointViewState.Showing : DataPointViewState.Normal;
                this.ChartArea.UpdateSession.Update((IUpdatable)xyDataPoint2);
            }
            int index3 = 0;
            foreach (DataPoint dataPoint in (Collection<DataPoint>)this.Series.DataPoints)
            {
                this.SetDataPointViewState(dataPoint, dataPointViewStateArray[index3]);
                dataPoint.IsNewlyAdded = false;
                ++index3;
            }
        }

        internal override bool CheckSimplifiedRenderingMode()
        {
            bool flag = base.CheckSimplifiedRenderingMode();
            if (flag)
                EnumerableFunctions.ForEach<DataPoint>(this.VisibleDataPoints, (Action<DataPoint>)(item => this.UpdateView(item)));
            return flag;
        }

        internal override FrameworkElement GetLegendSymbol()
        {
            Grid grid = new Grid();
            grid.Width = 20.0;
            grid.Height = 12.0;
            PointCollection pointCollection = new PointCollection();
            pointCollection.Add(new Point(0.0, 6.0));
            pointCollection.Add(new Point(20.0, 6.0));
            Collection<IAppearanceProvider> collection = new Collection<IAppearanceProvider>();
            LineDataPoint lineDataPoint1 = Enumerable.FirstOrDefault<LineDataPoint>(Enumerable.Where<LineDataPoint>(Enumerable.OfType<LineDataPoint>((IEnumerable)this.Series.DataPoints), (Func<LineDataPoint, bool>)(p =>
          {
              if (!p.ActualIsEmpty)
                  return p.IsVisible;
              return false;
          })));
            LineDataPoint lineDataPoint2 = this.Series.CreateDataPoint() as LineDataPoint;
            if (lineDataPoint1 != null)
            {
                lineDataPoint2.Stroke = lineDataPoint1.Stroke;
                lineDataPoint2.StrokeThickness = lineDataPoint1.StrokeThickness;
                lineDataPoint2.StrokeDashType = lineDataPoint1.StrokeDashType;
            }
            else if (this.Series.ItemsBinder != null)
                this.Series.ItemsBinder.Bind((DataPoint)lineDataPoint2, this.Series.DataContext);
            lineDataPoint2.Series = (Series)this.Series;
            collection.Add((IAppearanceProvider)lineDataPoint2);
            collection.Add((IAppearanceProvider)lineDataPoint2);
            lineDataPoint2.ActualEffect = lineDataPoint2.Effect;
            lineDataPoint2.ActualOpacity = lineDataPoint2.Opacity;
            PolylineControl polylineControl = new PolylineControl();
            polylineControl.Points = pointCollection;
            polylineControl.Appearances = collection;
            polylineControl.Update();
            polylineControl.Width = 20.0;
            polylineControl.Height = 12.0;
            grid.Children.Add((UIElement)polylineControl);
            DataPoint dataPoint = (DataPoint)(lineDataPoint1 ?? lineDataPoint2);
            if (!this.IsSimplifiedRenderingModeEnabled && dataPoint.MarkerType != MarkerType.None)
            {
                FrameworkElement view = (FrameworkElement)new MarkerControl();
                this.MarkerPresenter.BindViewToDataPoint(dataPoint, view, (string)null);
                view.Opacity = lineDataPoint2.Opacity;
                view.Effect = lineDataPoint2.Effect;
                grid.Children.Add((UIElement)view);
            }
            lineDataPoint2.Series = (Series)null;
            return (FrameworkElement)grid;
        }

        private void RootPanel_MouseMove(object sender, MouseEventArgs e)
        {
            this._mousePosition = e.GetPosition((IInputElement)this.RootPanel);
        }

        private void Tooltip_Opened(object sender, RoutedEventArgs e)
        {
            this._tooltip.Content = (object)LineSeriesPresenter.FindDataPoint(this.PolylineControl, this._mousePosition).ToolTipContent;
        }

        internal static XYDataPoint FindDataPoint(PolylineControl polyline, Point position)
        {
            XYDataPoint xyDataPoint1 = (XYDataPoint)null;
            foreach (IAppearanceProvider appearanceProvider in polyline.Appearances)
            {
                XYDataPoint xyDataPoint2 = appearanceProvider as XYDataPoint;
                if (xyDataPoint2 != null && xyDataPoint2.View != null && !xyDataPoint2.ActualIsEmpty)
                {
                    if (xyDataPoint1 == null)
                        xyDataPoint1 = xyDataPoint2;
                    else if (LineSeriesPresenter.GetDistance(position, xyDataPoint2.View.AnchorPoint) < LineSeriesPresenter.GetDistance(position, xyDataPoint1.View.AnchorPoint))
                        xyDataPoint1 = xyDataPoint2;
                }
            }
            return xyDataPoint1;
        }

        private void UpdateToolTipStyle()
        {
            DataPoint dataPoint = Enumerable.FirstOrDefault<DataPoint>(this.VisibleDataPoints);
            if (this._tooltip == null || dataPoint == null)
                return;
            this._tooltip.Style = dataPoint.ToolTipStyle;
        }

        private static double GetDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - p1.X, 2.0) + Math.Pow(p2.Y - p1.Y, 2.0));
        }

        internal override Geometry GetSelectionOutline(DataPoint dataPoint)
        {
            if (dataPoint.View == null)
                return (Geometry)null;
            Rect rect1 = new Rect(0.0, 0.0, 6.0, 6.0);
            Point anchorPoint = dataPoint.View.AnchorPoint;
            if (double.IsNaN(anchorPoint.X) || double.IsInfinity(anchorPoint.X) || (double.IsNaN(anchorPoint.Y) || double.IsInfinity(anchorPoint.Y)))
                return (Geometry)null;
            Rect rect2 = new Rect(anchorPoint.X - rect1.Width / 2.0, anchorPoint.Y - rect1.Height / 2.0, rect1.Width, rect1.Height);
            FrameworkElement child = Enumerable.FirstOrDefault<FrameworkElement>(Enumerable.OfType<FrameworkElement>((IEnumerable)this.RootPanel.Children));
            return (Geometry)new RectangleGeometry()
            {
                Rect = RectExtensions.Expand(RectExtensions.TranslateToParent(rect2, child, (FrameworkElement)this.ChartArea), 1.0, 1.0)
            };
        }
    }
}
