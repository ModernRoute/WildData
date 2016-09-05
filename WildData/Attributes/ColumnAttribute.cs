using System;

namespace ModernRoute.WildData.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute(string name, int size = 0)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Name = name;
            Size = size;
        }

        public string Name
        {
            get;
            private set;
        }

        public int Size
        {
            get;
            private set;
        }
    }
}
