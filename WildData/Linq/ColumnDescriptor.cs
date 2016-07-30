using ModernRoute.WildData.Linq.Tree.Expression;
using ModernRoute.WildData.Resources;
using System;

namespace ModernRoute.WildData.Linq
{
    public class ColumnDescriptor
    {
        public int Index
        {
            get;
            private set;
        }

        public ColumnReference Reference
        {
            get;
            private set;
        }

        public ColumnDescriptor(int index, ColumnReference reference)
        {
            if (index < 0)
            {
                throw new ArgumentException(Strings.ParameterIsLessThanZero,nameof(index));
            }

            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            Index = index;
            Reference = reference;
        }
    }
}
