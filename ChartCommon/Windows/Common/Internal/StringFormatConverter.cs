
using System;
using System.Globalization;
using System.Windows.Data;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return (object)string.Empty;
            return (object)string.Format((IFormatProvider)CultureInfo.CurrentCulture, parameter as string ?? "{0}", new object[1]
            {
        value
            });
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
