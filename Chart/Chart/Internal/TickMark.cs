using Semantic.Reporting.Windows.Common.Internal;
using System;
using System.Windows;

namespace Semantic.Reporting.Windows.Chart.Internal
{
    public class TickMark : MarkerControl
    {
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register("Position", typeof(AxisElementPosition), typeof(TickMark), new PropertyMetadata((object)AxisElementPosition.Outside, new PropertyChangedCallback(TickMark.OnPositionPropertyChanged)));
        internal const string PositionPropertyName = "Position";

        public AxisElementPosition Position
        {
            get
            {
                return (AxisElementPosition)this.GetValue(TickMark.PositionProperty);
            }
            set
            {
                this.SetValue(TickMark.PositionProperty, (object)value);
            }
        }

        public event EventHandler PositionChanged;

        public TickMark()
        {
            this.DefaultStyleKey = (object)typeof(TickMark);
        }

        private static void OnPositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TickMark)d).OnPositionPropertyChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnPositionPropertyChanged(object oldValue, object newValue)
        {
            if (this.PositionChanged == null)
                return;
            this.PositionChanged((object)this, EventArgs.Empty);
        }
    }
}
