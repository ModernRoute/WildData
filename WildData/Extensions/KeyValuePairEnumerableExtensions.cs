using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Extensions
{
    public static class KeyValuePairEnumerableExtensions
    {
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToDictionary(item => item.Key, item => item.Value);
        }
    }
}
