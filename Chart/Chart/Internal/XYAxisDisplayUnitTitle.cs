﻿using Semantic.Reporting.Windows.Common.Internal;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    internal class XYAxisDisplayUnitTitle : Title
    {
        internal XYAxisPresenter Presenter { get; private set; }

        public Axis Axis
        {
            get
            {
                return this.Presenter.Axis;
            }
        }

        internal XYAxisDisplayUnitTitle(XYAxisPresenter presenter)
        {
            this.Presenter = presenter;
            this.Presenter.PropertyChanged += new PropertyChangedEventHandler(this.OnPresenterPropertyChanged);
            this.SetBinding(ContentControl.ContentProperty, (BindingBase)new Binding("ActualDisplayUnitTitle")
            {
                Source = (object)this.Axis
            });
            this.SetBinding(FrameworkElement.StyleProperty, (BindingBase)new Binding("DisplayUnitTitleStyle")
            {
                Source = (object)this.Axis
            });
            this.SetBinding(UIElement.VisibilityProperty, (BindingBase)new Binding("ShowTitles")
            {
                Source = (object)this.Axis,
                Converter = (IValueConverter)new BooleanToVisibilityConverter()
            });
        }

        private void OnPresenterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "ActualLocation"))
                return;
            this.UpdateActualTextOrientation();
        }

        protected override void UpdateActualTextOrientation()
        {
            if (this.TextOrientation == TextOrientation.Auto)
            {
                if (this.Presenter.ActualOrientation == Orientation.Vertical)
                {
                    if (this.Presenter.ActualLocation != Edge.Left && this.Presenter.ActualLocation != Edge.Right)
                        return;
                    this.ActualTextOrientation = this.Presenter.ActualLocation == Edge.Right ? TextOrientation.Rotated90 : TextOrientation.Rotated270;
                }
                else
                {
                    if (this.Presenter.ActualOrientation != Orientation.Horizontal || this.Presenter.ActualLocation != Edge.Top && this.Presenter.ActualLocation != Edge.Bottom)
                        return;
                    this.ActualTextOrientation = TextOrientation.Horizontal;
                }
            }
            else
                base.UpdateActualTextOrientation();
        }
    }
}
