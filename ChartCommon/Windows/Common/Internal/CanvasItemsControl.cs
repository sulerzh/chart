using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class CanvasItemsControl : ItemsControl
    {
        public static readonly DependencyProperty XBindingPathProperty = DependencyProperty.Register("XBindingPath", typeof(string), typeof(CanvasItemsControl), new PropertyMetadata((object)"X"));
        public static readonly DependencyProperty YBindingPathProperty = DependencyProperty.Register("YBindingPath", typeof(string), typeof(CanvasItemsControl), new PropertyMetadata((object)"Y"));

        public string XBindingPath
        {
            get
            {
                return (string)this.GetValue(CanvasItemsControl.XBindingPathProperty);
            }
            set
            {
                this.SetValue(CanvasItemsControl.XBindingPathProperty, (object)value);
            }
        }

        public string YBindingPath
        {
            get
            {
                return (string)this.GetValue(CanvasItemsControl.YBindingPathProperty);
            }
            set
            {
                this.SetValue(CanvasItemsControl.YBindingPathProperty, (object)value);
            }
        }

        public CanvasItemsControl()
        {
            this.DefaultStyleKey = (object)typeof(CanvasItemsControl);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            FrameworkElement frameworkElement = element as FrameworkElement;
            Binding binding1 = new Binding(this.XBindingPath);
            Binding binding2 = new Binding(this.YBindingPath);
            frameworkElement.SetBinding(Canvas.LeftProperty, (BindingBase)binding1);
            frameworkElement.SetBinding(Canvas.TopProperty, (BindingBase)binding2);
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
