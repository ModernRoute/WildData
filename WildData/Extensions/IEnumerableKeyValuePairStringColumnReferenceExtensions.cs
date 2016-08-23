using ModernRoute.WildData.Linq;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Extensions
{
    public static class IEnumerableKeyValuePairStringColumnReferenceExtensions
    {
        public static IDictionary<string, ColumnDescriptor> ToColumnDescriptorDictionary(
            this IEnumerable<KeyValuePair<string, ColumnReference>> memberColumnMap)
        {
            return memberColumnMap.Select((item, index) =>
                    new KeyValuePair<string, ColumnDescriptor>(item.Key, new ColumnDescriptor(index, item.Value))).ToDictionary();
        }
    }
}
