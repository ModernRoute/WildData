using System.Collections.Generic;

namespace ModernRoute.WildData.Helpers
{
    static class Enumerate
    {
        public static IEnumerable<T> Sequence<T>(params T[] items)
        {
            foreach (T item in items)
            {
                yield return item;
            }

            yield break;
        }
    } 
}
