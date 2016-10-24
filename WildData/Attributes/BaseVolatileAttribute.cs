using System;
namespace ModernRoute.WildData.Attributes
{
    public abstract class BaseVolatileAttribute : Attribute
    {
        public bool ForcePush
        {
            get;
            private set;
        }

        public BaseVolatileAttribute(bool forcePush)
        {
            ForcePush = forcePush;
        }
    }
}
