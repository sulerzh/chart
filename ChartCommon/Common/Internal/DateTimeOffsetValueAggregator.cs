using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class DateTimeOffsetValueAggregator : ValueAggregator
    {
        public override bool CanPlot(object value)
        {
            return value is DateTimeOffset;
        }

        public override IComparable GetValue(object value)
        {
            if (value is DateTimeOffset)
                return (IComparable)value;
            return (IComparable)null;
        }

        public override Range<IComparable> GetRange(IEnumerable<object> values)
        {
            DateTimeOffset dateTimeOffset1 = DateTimeOffset.MaxValue;
            DateTimeOffset dateTimeOffset2 = DateTimeOffset.MinValue;
            foreach (DateTimeOffset dateTimeOffset3 in Enumerable.OfType<DateTimeOffset>((IEnumerable)values))
            {
                if (dateTimeOffset3 < dateTimeOffset1)
                    dateTimeOffset1 = dateTimeOffset3;
                if (dateTimeOffset3 > dateTimeOffset2)
                    dateTimeOffset2 = dateTimeOffset3;
            }
            if (dateTimeOffset1 != DateTimeOffset.MaxValue)
                return new Range<IComparable>((IComparable)dateTimeOffset1, (IComparable)dateTimeOffset2);
            return new Range<IComparable>();
        }

        public override Range<IComparable> GetSumRange(IEnumerable<object> values)
        {
            throw new NotSupportedException();
        }
    }
}
