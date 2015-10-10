using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class MarkerControl : Control
    {
        public static readonly DependencyProperty ActualGeometryProperty = DependencyProperty.Register("ActualGeometry", typeof(Geometry), typeof(MarkerControl), (PropertyMetadata)null);
        public static readonly DependencyProperty GeometryProperty = DependencyProperty.Register("Geometry", typeof(Geometry), typeof(MarkerControl), new PropertyMetadata(new PropertyChangedCallback(MarkerControl.OnGeometryPropertyChanged)));
        public static readonly DependencyProperty MarkerTypeProperty = DependencyProperty.Register("MarkerType", typeof(MarkerType), typeof(MarkerControl), new PropertyMetadata(new PropertyChangedCallback(MarkerControl.OnMarkerTypePropertyChanged)));
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(Brush), typeof(MarkerControl), new PropertyMetadata((PropertyChangedCallback)null));
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MarkerControl), new PropertyMetadata((object)1.0, (PropertyChangedCallback)null));
        internal const string StrokePropertyName = "Stroke";
        internal const string StrokeThicknessPropertyName = "StrokeThickness";

        public Geometry ActualGeometry
        {
            get
            {
                return this.GetValue(MarkerControl.ActualGeometryProperty) as Geometry;
            }
            internal set
            {
                this.SetValue(MarkerControl.ActualGeometryProperty, (object)value);
            }
        }

        public Geometry Geometry
        {
            get
            {
                return this.GetValue(MarkerControl.GeometryProperty) as Geometry;
            }
            set
            {
                this.SetValue(MarkerControl.GeometryProperty, (object)value);
            }
        }

        public MarkerType MarkerType
        {
            get
            {
                return (MarkerType)this.GetValue(MarkerControl.MarkerTypeProperty);
            }
            set
            {
                this.SetValue(MarkerControl.MarkerTypeProperty, (object)value);
            }
        }

        public Brush Stroke
        {
            get
            {
                return this.GetValue(MarkerControl.StrokeProperty) as Brush;
            }
            set
            {
                this.SetValue(MarkerControl.StrokeProperty, (object)value);
            }
        }

        public double StrokeThickness
        {
            get
            {
                return (double)this.GetValue(MarkerControl.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(MarkerControl.StrokeThicknessProperty, (object)value);
            }
        }

        public MarkerControl()
        {
            this.DefaultStyleKey = (object)typeof(MarkerControl);
        }

        private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MarkerControl)d).UpdateActualGeometry();
        }

        private static void OnMarkerTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MarkerControl)d).UpdateActualGeometry();
        }

        protected virtual void UpdateActualGeometry()
        {
            if (this.Geometry != null)
                this.ActualGeometry = this.Geometry;
            else
                this.ActualGeometry = VisualUtilities.GetMarkerGeometry(this.MarkerType, new Size(100.0, 100.0));
        }
    }
}
