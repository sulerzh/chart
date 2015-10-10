using System;
using System.Collections.Generic;
using System.Linq;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public static class RangeEnumerableExtensions
    {
        public static Range<T> GetRange<T>(this IEnumerable<T> that) where T : IComparable
        {
            if (!Enumerable.Any<T>(that))
                return new Range<T>();
            T minimum = Enumerable.First<T>(that);
            T maximum = minimum;
            foreach (T obj in that)
            {
                if (ValueHelper.Compare((IComparable)minimum, (IComparable)obj) == 1)
                    minimum = obj;
                if (ValueHelper.Compare((IComparable)maximum, (IComparable)obj) == -1)
                    maximum = obj;
            }
            if ((object)minimum == null || (object)maximum == null)
                return Range<T>.Empty;
            return new Range<T>(minimum, maximum);
        }

        public static Range<T> Sum<T>(this IEnumerable<Range<T>> that) where T : IComparable
        {
            if (!Enumerable.Any<Range<T>>(that))
                return new Range<T>();
            return Enumerable.Aggregate<Range<T>>(that, (Func<Range<T>, Range<T>, Range<T>>)((x, y) => x.Add(y)));
        }
    }
}
