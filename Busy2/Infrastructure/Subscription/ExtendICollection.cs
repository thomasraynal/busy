using System;
using System.Collections.Generic;
using System.Text;

namespace Busy.Infrastructure
{
    internal static class ExtendICollection
    {

        public static ICollection<T> AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (items == null) throw new ArgumentNullException(nameof(items));

            var list = collection as List<T>;
            if (list != null)
            {
                list.AddRange(items);
                return list;
            }

            foreach (var item in items)
            {
                collection.Add(item);
            }
            return collection;
        }

        public static ICollection<T> AddRange<T>(this ICollection<T> collection, params T[] items)
        {
            return AddRange(collection, (IEnumerable<T>)items);
        }

        public static ICollection<T> RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                collection.Remove(item);
            }

            return collection;
        }
    }
}


