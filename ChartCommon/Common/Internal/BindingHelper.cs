using System.Windows;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public static class BindingHelper
    {
        public static void ClearBinding(this FrameworkElement element, DependencyProperty dp)
        {
            if (element.GetBindingExpression(dp) == null)
                return;
            object obj = element.GetValue(dp);
            element.ClearValue(dp);
            element.SetValue(dp, obj);
        }
    }
}
