using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class XYAxisGridlinesPanel : XYAxisElementsPanel
    {
        private PanelElementPool<Line, Axis> _majorGridLinePool;
        private PanelElementPool<Line, Axis> _minorGridLinePool;
        private Line _oppositeAxisLine;

        internal Line OppositeAxisLine
        {
            get
            {
                return this._oppositeAxisLine;
            }
        }

        internal XYAxisGridlinesPanel(XYAxisPresenter presenter)
          : base(presenter)
        {
            this._majorGridLinePool = new PanelElementPool<Line, Axis>((Panel)this, (Func<Line>)(() => this.CreateGridLine(true)), (Action<Line, Axis>)((line, axis) => this.PrepareGridLine(line)), (Action<Line>)null);
            this._minorGridLinePool = new PanelElementPool<Line, Axis>((Panel)this, (Func<Line>)(() => this.CreateGridLine(false)), (Action<Line, Axis>)((line, axis) => this.PrepareGridLine(line)), (Action<Line>)null);
            this._oppositeAxisLine = this.CreateAxisLine();
        }

        private Line CreateAxisLine()
        {
            Line line1 = (Line)null;
            if (this.Presenter != null && this.Presenter.OppositeAxis != null)
            {
                Line line2 = new Line();
                line2.Stretch = Stretch.Fill;
                line1 = line2;
                line1.UseLayoutRounding = true;
                line1.SetBinding(FrameworkElement.StyleProperty, (BindingBase)new Binding("LineStyle")
                {
                    Source = (object)this.Presenter.OppositeAxis
                });
                Panel.SetZIndex((UIElement)line1, 100);
                this.UpdateAxisLineVisibility();
                this.Children.Add((UIElement)line1);
            }
            return line1;
        }

        private void UpdateAxisLineVisibility()
        {
            if (this.OppositeAxisLine == null)
                return;
            if (this.Axis != null && this.Axis.Scale != null && !this.Axis.Scale.HasCustomCrossingPosition)
                this.OppositeAxisLine.Visibility = Visibility.Collapsed;
            else
                this.OppositeAxisLine.SetBinding(UIElement.VisibilityProperty, (BindingBase)new Binding("ShowAxisLine")
                {
                    Source = (object)this.Presenter.OppositeAxis,
                    Converter = (IValueConverter)new BooleanToVisibilityConverter()
                });
        }

        private Line CreateGridLine(bool major)
        {
            Line line = new Line();
            line.UseLayoutRounding = true;
            line.SetBinding(FrameworkElement.StyleProperty, (BindingBase)new Binding(major ? "MajorGridlineStyle" : "MinorGridlineStyle")
            {
                Source = (object)this.Axis
            });
            return line;
        }

        private void PrepareGridLine(Line line)
        {
            line.X1 = line.X2 = line.Y1 = line.Y2 = 0.0;
            if (this.Orientation == Orientation.Horizontal)
                line.Y2 = 1.0;
            else
                line.X2 = 1.0;
            line.Stretch = Stretch.Fill;
        }

        private void PrepareOppositeAxisLine()
        {
            if (this.OppositeAxisLine == null || this.Axis == null || this.Axis.Scale == null)
                return;
            this.OppositeAxisLine.X2 = this.OppositeAxisLine.Y2 = 0.0;
            if (this.Orientation == Orientation.Horizontal)
                this.OppositeAxisLine.Y2 = 1.0;
            else
                this.OppositeAxisLine.X2 = 1.0;
            double num = this.Axis.Scale.ProjectDataValue(this.Axis.Scale.ActualCrossingPosition);
            if (DoubleHelper.GreaterOrEqualWithPrecision(num, 0.0) && DoubleHelper.LessOrEqualWithPrecision(num, 1.0))
            {
                this.UpdateAxisLineVisibility();
                XYAxisElementsPanel.SetCoordinate((UIElement)this.OppositeAxisLine, num);
            }
            else
                this.OppositeAxisLine.Visibility = Visibility.Collapsed;
        }

        protected override void ArrangeChild(UIElement child, Rect rect, Size finalSize)
        {
            Shape shape = child as Shape;
            if (shape != null && shape.Stretch == Stretch.Fill)
            {
                Size desiredSize = XYAxisBasePanel.GetDesiredSize(child);
                double num = Math.Round((this.Orientation == Orientation.Horizontal ? rect.X : rect.Y) + this.ElementWidth(desiredSize) / 2.0);
                rect = this.Orientation != Orientation.Horizontal ? new Rect(0.0, num, finalSize.Width, rect.Height) : new Rect(num, 0.0, rect.Width, finalSize.Height);
            }
            base.ArrangeChild(child, rect, finalSize);
        }

        protected override void Populate(double availableLength)
        {
            this._majorGridLinePool.ReleaseAll();
            this._minorGridLinePool.ReleaseAll();
            if (this.Presenter.IsMinorGridlinesVisible)
                this._majorGridLinePool.AdjustPoolSize();
            try
            {
                this.PrepareOppositeAxisLine();
                foreach (ScaleElementDefinition elementDefinition in (IEnumerable<ScaleElementDefinition>)new List<ScaleElementDefinition>((IEnumerable<ScaleElementDefinition>)Enumerable.OrderBy<ScaleElementDefinition, int>(Enumerable.Where<ScaleElementDefinition>(this.Presenter.GetScaleElements(), (Func<ScaleElementDefinition, bool>)(p => p.Kind == ScaleElementKind.Tickmark)), (Func<ScaleElementDefinition, int>)(p => p.Group != ScaleElementGroup.Major ? 0 : 1))))
                {
                    if (elementDefinition.Group == ScaleElementGroup.Major && this.Axis.ShowMajorGridlines)
                        EnumerableFunctions.ForEachWithIndex<ScalePosition>(Enumerable.Where<ScalePosition>(elementDefinition.Positions, (Func<ScalePosition, bool>)(p =>
                       {
                           if (p.Position >= 0.0)
                               return p.Position <= 1.0;
                           return false;
                       })), (Action<ScalePosition, int>)((position, index) => XYAxisElementsPanel.SetCoordinate((UIElement)this._majorGridLinePool.Get(this.Axis), position.Position)));
                    if (elementDefinition.Group == ScaleElementGroup.Minor && this.Presenter.IsMinorGridlinesVisible)
                        EnumerableFunctions.ForEachWithIndex<ScalePosition>(Enumerable.Where<ScalePosition>(elementDefinition.Positions, (Func<ScalePosition, bool>)(p =>
                       {
                           if (p.Position >= 0.0)
                               return p.Position <= 1.0;
                           return false;
                       })), (Action<ScalePosition, int>)((position, index) => XYAxisElementsPanel.SetCoordinate((UIElement)this._minorGridLinePool.Get(this.Axis), position.Position)));
                }
            }
            finally
            {
                if (!this.Presenter.IsMinorGridlinesVisible)
                    this._majorGridLinePool.AdjustPoolSize();
                this._minorGridLinePool.AdjustPoolSize();
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
        }
    }
}
