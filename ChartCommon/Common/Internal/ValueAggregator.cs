
using System;
using System.Collections.Generic;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public abstract class ValueAggregator
    {
        public abstract bool CanPlot(object value);

        public abstract IComparable GetValue(object value);

        public abstract Range<IComparable> GetRange(IEnumerable<object> values);

        public abstract Range<IComparable> GetSumRange(IEnumerable<object> values);

        public static ValueAggregator GetAggregator(DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Auto:
                    return (ValueAggregator)new DefaultValueAggregator();
                case DataValueType.Category:
                    return (ValueAggregator)new CategoryValueAggregator();
                case DataValueType.Integer:
                    return (ValueAggregator)new Int64ValueAggregator();
                case DataValueType.Float:
                    return (ValueAggregator)new DoubleValueAggregator();
                case DataValueType.DateTime:
                case DataValueType.Date:
                    return (ValueAggregator)new DateTimeValueAggregator();
                case DataValueType.Time:
                case DataValueType.TimeSpan:
                    return (ValueAggregator)new TimeValueAggregator();
                case DataValueType.DateTimeOffset:
                    return (ValueAggregator)new DateTimeOffsetValueAggregator();
                default:
                    throw new NotSupportedException();
            }
        }

        public static ValueAggregator GetPositiveAggregator(DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Integer:
                    return (ValueAggregator)new PositiveInt64ValueAggregator();
                case DataValueType.Float:
                    return (ValueAggregator)new PositiveDoubleValueAggregator();
                case DataValueType.Time:
                case DataValueType.TimeSpan:
                    return (ValueAggregator)new PositiveTimeValueAggregator();
                default:
                    return ValueAggregator.GetAggregator(valueType);
            }
        }

        public static ValueAggregator GetNegativeAggregator(DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Integer:
                    return (ValueAggregator)new NegativeInt64ValueAggregator();
                case DataValueType.Float:
                    return (ValueAggregator)new NegativeDoubleValueAggregator();
                case DataValueType.Time:
                case DataValueType.TimeSpan:
                    return (ValueAggregator)new NegativeTimeValueAggregator();
                default:
                    return ValueAggregator.GetAggregator(valueType);
            }
        }

        public static ValueAggregator GetAbsAggregator(DataValueType valueType)
        {
            switch (valueType)
            {
                case DataValueType.Integer:
                    return (ValueAggregator)new AbsInt64ValueAggregator();
                case DataValueType.Float:
                    return (ValueAggregator)new AbsDoubleValueAggregator();
                case DataValueType.Time:
                case DataValueType.TimeSpan:
                    return (ValueAggregator)new AbsTimeValueAggregator();
                default:
                    return ValueAggregator.GetAggregator(valueType);
            }
        }
    }
}
