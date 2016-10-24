using ModernRoute.WildData.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Extensions
{
    static class IEnumerableExtensions
    {
        internal static void ThrowIfAnyNull<T>(this IEnumerable<T> obj) where T : class
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (obj.Any(item => item == null))
            {
                throw new ArgumentException(Strings.AtLeastOneItemInCollectionIsNull);
            }
        }        

        internal static SortedDictionary<TKey, TElement> ToSortedDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (elementSelector == null)
            {
                throw new ArgumentNullException(nameof(elementSelector));
            }

            SortedDictionary<TKey, TElement> dictionary = new SortedDictionary<TKey, TElement>(comparer);

            foreach (TSource item in source)
            {
                dictionary.Add(keySelector(item), elementSelector(item));
            }

            return dictionary;
        }
    }
}
