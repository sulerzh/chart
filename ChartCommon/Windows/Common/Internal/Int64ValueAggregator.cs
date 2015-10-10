using System;
using System.Collections.Generic;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public class Int64ValueAggregator : ValueAggregator
    {
        public override bool CanPlot(object value)
        {
            long x;
            return this.TryConvert(value, out x);
        }

        protected virtual bool CanPlot(long x)
        {
            return true;
        }

        protected virtual bool TryConvert(object value, out long x)
        {
            if (ValueHelper.TryConvert(value, true, out x))
                return this.CanPlot(x);
            return false;
        }

        public override IComparable GetValue(object value)
        {
            long x;
            if (this.TryConvert(value, out x))
                return (IComparable)x;
            return (IComparable)null;
        }

        public override Range<IComparable> GetRange(IEnumerable<object> values)
        {
            long num1 = long.MaxValue;
            long num2 = long.MinValue;
            foreach (object obj in values)
            {
                long x;
                if (this.TryConvert(obj, out x))
                {
                    if (x < num1)
                        num1 = x;
                    if (x > num2)
                        num2 = x;
                }
            }
            if (num1 != long.MaxValue)
                return new Range<IComparable>((IComparable)num1, (IComparable)num2);
            return new Range<IComparable>();
        }

        public override Range<IComparable> GetSumRange(IEnumerable<object> values)
        {
            long num = 0L;
            foreach (object obj in values)
            {
                long x;
                if (this.TryConvert(obj, out x))
                    num += x;
            }
            return new Range<IComparable>((IComparable)0, (IComparable)num);
        }
    }
}
