using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class SeriesMarkerPresenter : SeriesAttachedPresenter
    {
        private AnchorPanel _markersPanel;
        private PanelElementPool<FrameworkElement, DataPoint> _pointMarkerElementPool;

        protected virtual Panel MarkersPanel
        {
            get
            {
                if (this._markersPanel == null)
                {
                    this._markersPanel = new AnchorPanel();
                    this._markersPanel.CollisionDetectionEnabled = true;
                    Panel.SetZIndex((UIElement)this._markersPanel, 1100);
                    this.SeriesPresenter.RootPanel.Children.Add((UIElement)this._markersPanel);
                    this.SeriesPresenter.RootPanel.SizeChanged += (SizeChangedEventHandler)((s, e) =>
                   {
                       this._markersPanel.Width = e.NewSize.Width;
                       this._markersPanel.Height = e.NewSize.Height;
                   });
                }
                return (Panel)this._markersPanel;
            }
        }

        internal PanelElementPool<FrameworkElement, DataPoint> PointMarkerElementPool
        {
            get
            {
                if (this._pointMarkerElementPool == null)
                {
                    this._pointMarkerElementPool = new PanelElementPool<FrameworkElement, DataPoint>(this.MarkersPanel, (Func<FrameworkElement>)(() => this.CreateViewElement()), (Action<FrameworkElement, DataPoint>)((element, dataPoint) =>
                  {
                      element.DataContext = (object)dataPoint;
                      this.BindViewToDataPoint(dataPoint, element, (string)null);
                  }), (Action<FrameworkElement>)(element => element.DataContext = (object)null));
                    this._pointMarkerElementPool.MaxElementCount = 100;
                }
                return this._pointMarkerElementPool;
            }
        }

        public SeriesMarkerPresenter(SeriesPresenter seriesPresenter)
          : base(seriesPresenter)
        {
        }

        internal override void OnCreateView(DataPoint dataPoint)
        {
            if (!this.IsMarkerVisible(dataPoint))
                return;
            FrameworkElement viewElement = this.CreateViewElement(dataPoint);
            dataPoint.View.MarkerView = viewElement;
            this.BindViewToDataPoint(dataPoint, viewElement, (string)null);
        }

        internal override void OnRemoveView(DataPoint dataPoint)
        {
            if (dataPoint.View == null || dataPoint.View.MarkerView == null)
                return;
            this.PointMarkerElementPool.Release(dataPoint.View.MarkerView);
            SeriesTooltipPresenter.ClearToolTip((DependencyObject)dataPoint.View.MarkerView);
            dataPoint.View.MarkerView = (FrameworkElement)null;
            if (this.SeriesPresenter.ChartArea == null)
                return;
            this.SeriesPresenter.ChartArea.UpdateSession.ExecuteOnceAfterUpdating((Action)(() => this.PointMarkerElementPool.AdjustPoolSize()), (object)new Tuple<Series, string>(this.SeriesPresenter.Series, "__AdjustDataPointElementPoolSize__"), (string)null);
        }

        internal override void OnUpdateView(DataPoint dataPoint)
        {
            DataPointView view = dataPoint.View;
            if (view == null)
                return;
            FrameworkElement frameworkElement = view.MarkerView;
            bool flag = this.IsMarkerVisible(dataPoint) && ValueHelper.CanGraph(view.AnchorPoint.X) && ValueHelper.CanGraph(view.AnchorPoint.Y);
            if (flag && frameworkElement == null)
            {
                this.OnCreateView(dataPoint);
                frameworkElement = view.MarkerView;
            }
            else if (!flag && frameworkElement != null)
            {
                this.OnRemoveView(dataPoint);
                frameworkElement = (FrameworkElement)null;
            }
            if (frameworkElement == null)
                return;
            AnchorPanel.SetHideOverlapped((UIElement)frameworkElement, this.CanHideMarker(dataPoint));
            AnchorPanel.SetMaximumMovingDistance((UIElement)frameworkElement, 0.0);
            AnchorPanel.SetAnchorPoint((UIElement)frameworkElement, view.AnchorPoint);
            AnchorPanel.SetContentPosition((UIElement)frameworkElement, ContentPositions.MiddleCenter);
        }

        internal override void OnSeriesRemoved()
        {
            if (this._pointMarkerElementPool == null)
                return;
            this._pointMarkerElementPool.Clear();
        }

        protected virtual FrameworkElement CreateViewElement(DataPoint dataPoint)
        {
            if (this.SeriesPresenter.IsSimplifiedRenderingModeEnabled && !dataPoint.IsEmpty && dataPoint.ReadLocalValue(DataPoint.MarkerTypeProperty) == DependencyProperty.UnsetValue)
                return (FrameworkElement)null;
            return this.PointMarkerElementPool.Get(dataPoint);
        }

        internal virtual FrameworkElement CreateViewElement()
        {
            return (FrameworkElement)new MarkerControl();
        }

        internal virtual void BindViewToDataPoint(DataPoint dataPoint, FrameworkElement view, string valueName)
        {
            MarkerControl markerControl = view as MarkerControl;
            if (markerControl == null || (IAppearanceProvider)dataPoint == null)
                return;
            if (valueName == "MarkerType" || valueName == null)
                markerControl.MarkerType = dataPoint.MarkerType;
            if (valueName == "MarkerStyle" || valueName == null)
                markerControl.Style = dataPoint.MarkerStyle;
            if (valueName == "MarkerSize" || valueName == null)
            {
                markerControl.Width = dataPoint.MarkerSize;
                markerControl.Height = dataPoint.MarkerSize;
            }
            if (valueName == "Opacity" || valueName == "ActualOpacity" || valueName == null)
                markerControl.Opacity = dataPoint.ActualOpacity;
            if (!(valueName == "Effect") && !(valueName == "ActualEffect") && valueName != null)
                return;
            markerControl.Effect = dataPoint.ActualEffect;
        }

        internal virtual bool IsMarkerVisible(DataPoint dataPoint)
        {
            return this.SeriesPresenter.IsDataPointVisible(dataPoint) && dataPoint.MarkerType != MarkerType.None && (!this.SeriesPresenter.IsSimplifiedRenderingModeEnabled || !this.CanHideMarker(dataPoint));
        }

        internal virtual bool CanHideMarker(DataPoint dataPoint)
        {
            if (!dataPoint.IsEmpty)
                return dataPoint.ReadLocalValue(DataPoint.MarkerTypeProperty) == DependencyProperty.UnsetValue;
            return false;
        }

        internal virtual double GetMarkerSize(DataPoint dataPoint)
        {
            return dataPoint.MarkerSize;
        }
    }
}
