using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Reporting.Common.Toolkit.Internal
{
    public class HighContrastCanvasScrollBarOpacityConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (HighContrastHelper.CurrentTheme == HighContrastTheme.None)
                return (object)0.4;
            return (object)0.8;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
