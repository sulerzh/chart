using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Semantic.Reporting.Windows.Common.Internal
{
    public static class EnumerableFunctions
    {
        public static int FastCount(this IEnumerable that)
        {
            IList list = that as IList;
            if (list != null)
                return list.Count;
            return Enumerable.Count<object>(Enumerable.Cast<object>(that));
        }

        public static bool IsEmpty(this IEnumerable that)
        {
            return !that.GetEnumerator().MoveNext();
        }

        public static T MinOrNull<T>(this IEnumerable<T> that, Func<T, IComparable> projectionFunction) where T : class
        {
            T obj1 = default(T);
            if (!Enumerable.Any<T>(that))
                return obj1;
            T obj2 = Enumerable.First<T>(that);
            IComparable comparable1 = projectionFunction(obj2);
            foreach (T obj3 in Enumerable.Skip<T>(that, 1))
            {
                IComparable comparable2 = projectionFunction(obj3);
                if (comparable1.CompareTo((object)comparable2) > 0)
                {
                    comparable1 = comparable2;
                    obj2 = obj3;
                }
            }
            return obj2;
        }

        public static double SumOrDefault(this IEnumerable<double> that)
        {
            if (!Enumerable.Any<double>(that))
                return 0.0;
            return Enumerable.Sum(that);
        }

        public static T MaxOrNull<T>(this IEnumerable<T> that, Func<T, IComparable> projectionFunction) where T : class
        {
            T obj1 = default(T);
            if (!Enumerable.Any<T>(that))
                return obj1;
            T obj2 = Enumerable.First<T>(that);
            IComparable comparable1 = projectionFunction(obj2);
            foreach (T obj3 in Enumerable.Skip<T>(that, 1))
            {
                IComparable comparable2 = projectionFunction(obj3);
                if (comparable1.CompareTo((object)comparable2) < 0)
                {
                    comparable1 = comparable2;
                    obj2 = obj3;
                }
            }
            return obj2;
        }

        public static IEnumerable<T> Iterate<T>(T value, Func<T, T> nextFunction)
        {
            yield return value;
            while (true)
            {
                value = nextFunction(value);
                yield return value;
            }
        }

        public static int IndexOf(this IEnumerable that, object value)
        {
            int num = 0;
            foreach (object objB in that)
            {
                if (object.ReferenceEquals(value, objB) || value.Equals(objB))
                    return num;
                ++num;
            }
            return -1;
        }

        public static int FindIndexOf<T>(this IEnumerable<T> that, Func<T, bool> action)
        {
            int num = 0;
            foreach (T obj in that)
            {
                if (action(obj))
                    return num;
                ++num;
            }
            return -1;
        }

        public static void ForEachWithIndex<T>(this IEnumerable<T> that, Action<T, int> action)
        {
            int num = 0;
            foreach (T obj in that)
            {
                action(obj, num);
                ++num;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> that, Action<T> action)
        {
            foreach (T obj in that)
                action(obj);
        }

        public static T? MaxOrNullable<T>(this IEnumerable<T> that) where T : struct, IComparable
        {
            if (!Enumerable.Any<T>(that))
                return new T?();
            return new T?(Enumerable.Max<T>(that));
        }

        public static T? MinOrNullable<T>(this IEnumerable<T> that) where T : struct, IComparable
        {
            if (!Enumerable.Any<T>(that))
                return new T?();
            return new T?(Enumerable.Min<T>(that));
        }

        public static IEnumerable<TSource> DistinctOfSorted<TSource>(this IEnumerable<TSource> source)
        {
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    TSource last = enumerator.Current;
                    yield return last;
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator.Current.Equals((object)last))
                        {
                            last = enumerator.Current;
                            yield return last;
                        }
                    }
                }
            }
        }

        public static T FastElementAt<T>(this IEnumerable that, int index)
        {
            IList<T> list1 = that as IList<T>;
            if (list1 != null)
                return list1[index];
            IList list2 = that as IList;
            if (list2 != null)
                return (T)list2[index];
            return Enumerable.ElementAt<T>(Enumerable.Cast<T>(that), index);
        }

        public static bool IsSameAs<T>(this IList<T> one, IList<T> another)
        {
            if (one == another)
                return true;
            if (one == null || another == null || one.Count != another.Count)
                return false;
            for (int index = 0; index < one.Count; ++index)
            {
                if (!object.Equals((object)one[index], (object)another[index]))
                    return false;
            }
            return true;
        }

        public static bool IsSameAs<T>(this ISet<T> one, ISet<T> another)
        {
            if (one == another)
                return true;
            if (one == null || another == null || one.Count != another.Count)
                return false;
            foreach (T obj in (IEnumerable<T>)one)
            {
                if (!another.Contains(obj))
                    return false;
            }
            return true;
        }
    }
}
