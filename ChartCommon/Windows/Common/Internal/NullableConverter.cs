using System;
using System.ComponentModel;
using System.Globalization;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class NullableConverter<T> : TypeConverter where T : struct
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(T) || sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(T);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string str = value as string;
            if (value is T)
                return (object)new T?((T)value);
            if (string.IsNullOrEmpty(str) || string.Equals(str, "Auto", StringComparison.OrdinalIgnoreCase))
                return (object)new T?();
            if (str != null)
            {
                if (typeof(T).IsEnum)
                {
                    try
                    {
                        return (object)new T?((T)Enum.Parse(typeof(T), str, false));
                    }
                    catch (ArgumentNullException ex)
                    {
                    }
                    catch (ArgumentException ex)
                    {
                    }
                }
            }
            if (typeof(T) == typeof(TimeSpan))
                return (object)new T?((T)(object)TimeSpan.Parse(str, (IFormatProvider)culture));
            return (object)new T?((T)Convert.ChangeType(value, typeof(T), (IFormatProvider)culture));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                return (object)string.Empty;
            if (destinationType == typeof(string))
                return (object)value.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
