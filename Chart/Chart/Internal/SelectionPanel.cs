using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class SelectionPanel : Canvas, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedElementProperty = DependencyProperty.Register("SelectedElement", typeof(FrameworkElement), typeof(SelectionPanel), new PropertyMetadata((object)null, new PropertyChangedCallback(SelectionPanel.OnSelectedElementPropertyChanged)));
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(SelectionPanel), new PropertyMetadata((object)false, new PropertyChangedCallback(SelectionPanel.OnIsActivePropertyChanged)));
        internal const string SelectedElementPropertyName = "SelectedElement";
        internal const string IsActivePropertyName = "IsActive";
        private PanelElementPool<SelectionAdorner, Geometry> _pathAdornersPool;

        public ChartArea ChartArea { get; private set; }

        internal bool KeepActive { get; set; }

        public FrameworkElement SelectedElement
        {
            get
            {
                return (FrameworkElement)this.GetValue(SelectionPanel.SelectedElementProperty);
            }
            set
            {
                this.SetValue(SelectionPanel.SelectedElementProperty, (object)value);
            }
        }

        public bool IsActive
        {
            get
            {
                return (bool)this.GetValue(SelectionPanel.IsActiveProperty);
            }
            set
            {
                this.SetValue(SelectionPanel.IsActiveProperty, value);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SelectionPanel(ChartArea chartArea)
        {
            this._pathAdornersPool = new PanelElementPool<SelectionAdorner, Geometry>((Panel)this, new Func<SelectionAdorner>(this.CreatePathAdorner), new Action<SelectionAdorner, Geometry>(this.InitializePathAdorner), new Action<SelectionAdorner>(this.ResetPathAdorner));
            this.ChartArea = chartArea;
            this.ChartArea.PropertyChanged += new PropertyChangedEventHandler(this.ChartArea_PropertyChanged);
            this.ChartArea.KeyDown += new KeyEventHandler(this.ChartArea_KeyDown);
        }

        private SelectionAdorner CreatePathAdorner()
        {
            return new SelectionAdorner();
        }

        private void InitializePathAdorner(SelectionAdorner adorner, Geometry geometry)
        {
            adorner.Outline = geometry;
            adorner.Visibility = Visibility.Visible;
        }

        private void ResetPathAdorner(SelectionAdorner adorner)
        {
            adorner.Outline = (Geometry)null;
            adorner.Visibility = Visibility.Collapsed;
        }

        private static void OnSelectedElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SelectionPanel)d).OnSelectedElementPropertyChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnSelectedElementPropertyChanged(object oldValue, object newValue)
        {
            this.OnPropertyChanged("SelectedElement");
            this.FocusElement();
            this.Invalidate();
        }

        private static void OnIsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SelectionPanel)d).OnIsActivePropertyChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnIsActivePropertyChanged(object oldValue, object newValue)
        {
            this.OnPropertyChanged("IsActive");
            this.SelectedElement = this.IsActive ? (FrameworkElement)this.ChartArea : (FrameworkElement)null;
        }

        private IEnumerable<FrameworkElement> GetSelectableChartElements()
        {
            yield return (FrameworkElement)this.ChartArea;
            foreach (Series series in this.ChartArea.GetSeries())
            {
                yield return (FrameworkElement)series;
                foreach (DataPoint dataPoint in Enumerable.Where<DataPoint>((IEnumerable<DataPoint>)series.DataPoints, (Func<DataPoint, bool>)(d => d.ViewState == DataPointViewState.Normal)))
                    yield return (FrameworkElement)dataPoint;
            }
        }

        private FrameworkElement GetParentElement(FrameworkElement element)
        {
            if (element is DataPoint)
                return (FrameworkElement)((DataPoint)element).Series;
            return (FrameworkElement)this.ChartArea;
        }

        private Series GetNextSeries(FrameworkElement element, bool isForward)
        {
            Series series = (Series)null;
            if (element is DataPoint)
            {
                series = ((DataPoint)element).Series;
            }
            else
            {
                List<Series> list = new List<Series>(this.ChartArea.GetSeries());
                if (element is Series && list.Contains((Series)element))
                {
                    int num = EnumerableFunctions.IndexOf((IEnumerable)list, (object)element);
                    int index = !isForward ? num - 1 : num + 1;
                    if (index < 0)
                        index = list.Count - 1;
                    else if (index > list.Count - 1)
                        index = 0;
                    series = list[index];
                }
                if (series == null && list.Count > 0)
                    series = list[0];
            }
            return series;
        }

        private void ChartArea_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = this.ProcessKeys(e.Key, Keyboard.Modifiers);
        }

        internal bool ProcessKeys(Key key, ModifierKeys modifiers)
        {
            bool flag1 = false;
            bool flag2 = this.ChartArea.Orientation == Orientation.Horizontal;
            bool flag3 = modifiers == ModifierKeys.Control;
            bool addToSelection = (modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            switch (key)
            {
                case Key.Return:
                case Key.Space:
                    this.ChartArea.FireDataPointSelectionChanging(this.SelectedElement as DataPoint, addToSelection);
                    flag1 = true;
                    break;
                case Key.Prior:
                    if (modifiers == ModifierKeys.None)
                    {
                        this.ChartArea.ScrollPlotArea(-1, flag2);
                        flag1 = true;
                        break;
                    }
                    break;
                case Key.Next:
                    if (modifiers == ModifierKeys.None)
                    {
                        this.ChartArea.ScrollPlotArea(1, flag2);
                        flag1 = true;
                        break;
                    }
                    break;
                case Key.End:
                    this.ChartArea.ScrollPlotArea(true, flag2);
                    flag1 = true;
                    break;
                case Key.Home:
                    this.ChartArea.ScrollPlotArea(false, flag2);
                    flag1 = true;
                    break;
                case Key.Left:
                    if (modifiers == ModifierKeys.None)
                    {
                        this.MoveToTheNextElement(false, !flag2);
                        flag1 = true;
                        break;
                    }
                    if (flag3)
                    {
                        this.ChartArea.ZoomPlotArea(false);
                        flag1 = true;
                        break;
                    }
                    break;
                case Key.Up:
                    if (modifiers == ModifierKeys.None)
                    {
                        this.MoveToTheNextElement(flag2, flag2);
                        flag1 = true;
                        break;
                    }
                    if (flag3)
                    {
                        this.ChartArea.ZoomPlotArea(true);
                        flag1 = true;
                        break;
                    }
                    break;
                case Key.Right:
                    if (modifiers == ModifierKeys.None)
                    {
                        this.MoveToTheNextElement(true, !flag2);
                        flag1 = true;
                        break;
                    }
                    if (flag3)
                    {
                        this.ChartArea.ZoomPlotArea(true);
                        flag1 = true;
                        break;
                    }
                    break;
                case Key.Down:
                    if (modifiers == ModifierKeys.None)
                    {
                        this.MoveToTheNextElement(!flag2, flag2);
                        flag1 = true;
                        break;
                    }
                    if (flag3)
                    {
                        this.ChartArea.ZoomPlotArea(false);
                        flag1 = true;
                        break;
                    }
                    break;
            }
            return flag1;
        }

        private bool MoveToTheNextElement(bool isForward, bool isSeries)
        {
            List<FrameworkElement> list = Enumerable.ToList<FrameworkElement>(this.GetSelectableChartElements());
            FrameworkElement element = this.SelectedElement;
            int num;
            for (num = list.IndexOf(element); num == -1; num = list.IndexOf(element))
                element = this.GetParentElement(element);
            int index = num;
            if (isSeries)
            {
                Series nextSeries = this.GetNextSeries(element, isForward);
                if (nextSeries != null)
                    index = list.IndexOf((FrameworkElement)nextSeries);
            }
            else if (isForward)
                ++index;
            else
                --index;
            if (index < 0)
                index = list.Count - 1;
            if (index > list.Count - 1)
                index = 0;
            if (num == index)
                return false;
            this.SelectedElement = list[index];
            return true;
        }

        private void FocusElement()
        {
            if (this.SelectedElement == null)
                return;
            UIElementAutomationPeer.CreatePeerForElement((UIElement)this.ChartArea).RaiseAutomationEvent(AutomationEvents.AutomationFocusChanged);
        }

        private void ChartArea_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.KeepActive && this.IsActive)
                return;
            switch (e.PropertyName)
            {
                case "IsEnabled":
                case "IsFocused":
                case "IsKeyboardNavigationEnabled":
                    this.IsActive = this.ChartArea.IsFocused && this.ChartArea.IsEnabled && this.ChartArea.IsKeyboardNavigationEnabled;
                    break;
            }
        }

        private RectangleGeometry GetBoundingRectangle(FrameworkElement element)
        {
            Rect rect = element == this.ChartArea ? new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight) : LayoutInformation.GetLayoutSlot(element);
            return new RectangleGeometry()
            {
                Rect = RectExtensions.TranslateToParent(rect, element, (FrameworkElement)this.ChartArea)
            };
        }

        public void Invalidate()
        {
            this._pathAdornersPool.ReleaseAll();
            this.Clip = (Geometry)this.GetBoundingRectangle((FrameworkElement)this.ChartArea.PlotAreaPanel);
            if (this.SelectedElement == null || this.SelectedElement == this.ChartArea)
                return;
            Series series = this.SelectedElement as Series;
            if (series != null && series.SeriesPresenter != null)
            {
                foreach (Geometry selectedItemGeomerty in series.SeriesPresenter.GetSelectionOutlines())
                    this.ShowSelection(selectedItemGeomerty);
            }
            else
            {
                DataPoint dataPoint = this.SelectedElement as DataPoint;
                if (dataPoint != null && dataPoint.Series != null && dataPoint.Series.SeriesPresenter != null)
                    this.ShowSelection(dataPoint.Series.SeriesPresenter.GetSelectionOutline(dataPoint));
                else
                    this.ShowSelection((Geometry)this.GetBoundingRectangle(this.SelectedElement));
            }
        }

        private void ShowSelection(Geometry selectedItemGeomerty)
        {
            if (selectedItemGeomerty == null)
                return;
            this._pathAdornersPool.Get(selectedItemGeomerty);
        }

        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged == null)
                return;
            this.PropertyChanged((object)this, new PropertyChangedEventArgs(name));
        }
    }
}
