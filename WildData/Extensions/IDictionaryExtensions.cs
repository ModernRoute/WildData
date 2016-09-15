using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModernRoute.WildData.Extensions
{
    static class IDictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }
    }
}
