using System;

namespace ModernRoute.WildData.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class StorageAttribute : Attribute
    {
        public StorageAttribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public string Name
        {
            get;
            private set;
        }
    }
}
