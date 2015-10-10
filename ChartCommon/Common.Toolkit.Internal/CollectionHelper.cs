using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Semantic.Reporting.Common.Toolkit.Internal
{
    internal static class CollectionHelper
    {
        public static bool IsReadOnly(this IEnumerable collection)
        {
            if (!collection.GetType().IsArray)
                return Enumerable.Any<Type>(Enumerable.TakeWhile<Type>(EnumerableExtensions.Iterate<Type>(collection.GetType(), (Func<Type, Type>)(type => type.BaseType)), (Func<Type, bool>)(type => type != (Type)null)), (Func<Type, bool>)(type => type.FullName.StartsWith("System.Collections.ObjectModel.ReadOnlyCollection`1", StringComparison.Ordinal)));
            return true;
        }

        public static bool CanInsert(this IEnumerable collection, object item)
        {
            ICollectionView collectionView = collection as ICollectionView;
            if (collectionView != null)
                return CollectionHelper.CanInsert(collectionView.SourceCollection, item);
            if (CollectionHelper.IsReadOnly(collection))
                return false;
            Type type = Enumerable.FirstOrDefault<Type>(Enumerable.Where<Type>((IEnumerable<Type>)collection.GetType().GetInterfaces(), (Func<Type, bool>)(interfaceType => interfaceType.FullName.StartsWith("System.Collections.Generic.IList`1", StringComparison.Ordinal))));
            if (type != (Type)null)
                return type.GetGenericArguments()[0] == item.GetType();
            return collection is IList;
        }

        public static void Insert(this IEnumerable collection, int index, object item)
        {
            ICollectionView collectionView = collection as ICollectionView;
            if (collectionView != null)
            {
                CollectionHelper.Insert(collectionView.SourceCollection, index, item);
            }
            else
            {
                Type type = Enumerable.FirstOrDefault<Type>(Enumerable.Where<Type>((IEnumerable<Type>)collection.GetType().GetInterfaces(), (Func<Type, bool>)(interfaceType => interfaceType.FullName.StartsWith("System.Collections.Generic.IList`1", StringComparison.Ordinal))));
                if (type != (Type)null)
                    type.GetMethod("Insert").Invoke((object)collection, new object[2]
                    {
            (object) index,
            item
                    });
                else
                    (collection as IList).Insert(index, item);
            }
        }

        public static int Count(this IEnumerable collection)
        {
            ICollectionView collectionView = collection as ICollectionView;
            if (collectionView != null)
                return CollectionHelper.Count(collectionView.SourceCollection);
            Type type = Enumerable.FirstOrDefault<Type>(Enumerable.Where<Type>((IEnumerable<Type>)collection.GetType().GetInterfaces(), (Func<Type, bool>)(interfaceType => interfaceType.FullName.StartsWith("System.Collections.Generic.ICollection`1", StringComparison.Ordinal))));
            if (type != (Type)null)
                return (int)type.GetProperty("Count").GetValue((object)collection, new object[0]);
            IList list = collection as IList;
            if (list != null)
                return list.Count;
            return Enumerable.Count<object>(Enumerable.OfType<object>(collection));
        }

        public static void Add(this IEnumerable collection, object item)
        {
            ICollectionView collectionView = collection as ICollectionView;
            if (collectionView != null)
            {
                CollectionHelper.Add(collectionView.SourceCollection, item);
            }
            else
            {
                int index = (int)collection.GetType().GetProperty("Count").GetValue((object)collection, new object[0]);
                CollectionHelper.Insert(collection, index, item);
            }
        }

        public static void Remove(this IEnumerable collection, object item)
        {
            ICollectionView collectionView = collection as ICollectionView;
            if (collectionView != null)
            {
                CollectionHelper.Remove(collectionView.SourceCollection, item);
            }
            else
            {
                Type type = Enumerable.FirstOrDefault<Type>(Enumerable.Where<Type>((IEnumerable<Type>)collection.GetType().GetInterfaces(), (Func<Type, bool>)(interfaceType => interfaceType.FullName.StartsWith("System.Collections.Generic.IList`1", StringComparison.Ordinal))));
                if (type != (Type)null)
                {
                    int num = (int)type.GetMethod("IndexOf").Invoke((object)collection, new object[1]
                    {
            item
                    });
                    if (num == -1)
                        return;
                    type.GetMethod("RemoveAt").Invoke((object)collection, new object[1]
                    {
            (object) num
                    });
                }
                else
                    (collection as IList).Remove(item);
            }
        }

        public static void RemoveAt(this IEnumerable collection, int index)
        {
            ICollectionView collectionView = collection as ICollectionView;
            if (collectionView != null)
            {
                CollectionHelper.RemoveAt(collectionView.SourceCollection, index);
            }
            else
            {
                Type type = Enumerable.FirstOrDefault<Type>(Enumerable.Where<Type>((IEnumerable<Type>)collection.GetType().GetInterfaces(), (Func<Type, bool>)(interfaceType => interfaceType.FullName.StartsWith("System.Collections.Generic.IList`1", StringComparison.Ordinal))));
                if (type != (Type)null)
                    type.GetMethod("RemoveAt").Invoke((object)collection, new object[1]
                    {
            (object) index
                    });
                else
                    (collection as IList).RemoveAt(index);
            }
        }
    }
}
