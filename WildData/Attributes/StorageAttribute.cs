using System;

namespace ModernRoute.WildData.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class StorageAttribute : Attribute
    {
        public StorageAttribute(string name, string schema = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
            Schema = schema;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Schema
        {
            get;
            private set;
        }
    }
}
