using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Busy
{
    internal static class ExtendIEnumerable
    {

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var random = new Random();
            var buffer = source.ToList();
            for (var i = 0; i < buffer.Count; i++)
            {
                var randomIndex = random.Next(i, buffer.Count);
                yield return buffer[randomIndex];

                buffer[randomIndex] = buffer[i];
            }
        }

        public static IList<T> AsList<T>(this IEnumerable<T> collection)
            => collection is IList<T> list ? list : collection.ToList();

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static void ForEach<T>( this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var t in enumerable)
            {
                action(t);
            }
        }

        public static IEnumerable<T> AsReadOnlyEnumerable<T>(this IEnumerable<T> items)
        {
            foreach (var item in items)
                yield return item;
        }
    }
}
