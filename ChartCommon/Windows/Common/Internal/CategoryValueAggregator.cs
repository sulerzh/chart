using System;
using System.Collections;
using System.Collections.Generic;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class CategoryValueAggregator : ValueAggregator
    {
        public override bool CanPlot(object value)
        {
            return value != null;
        }

        public override IComparable GetValue(object value)
        {
            return (IComparable)value;
        }

        public override Range<IComparable> GetRange(IEnumerable<object> values)
        {
            int num = EnumerableFunctions.FastCount((IEnumerable)values);
            if (num > 0)
                return new Range<IComparable>((IComparable)0, (IComparable)(num - 1));
            return new Range<IComparable>();
        }

        public override Range<IComparable> GetSumRange(IEnumerable<object> values)
        {
            return new Range<IComparable>((IComparable)0, (IComparable)EnumerableFunctions.FastCount((IEnumerable)values));
        }
    }
}
