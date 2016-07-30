using ModernRoute.WildData.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Extensions
{
    public static class IEnumerableExtensions
    {
        public static void ThrowIfAnyNull<T>(this IEnumerable<T> obj) where T : class
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
    }
}
