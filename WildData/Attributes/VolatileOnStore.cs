using System;

namespace ModernRoute.WildData.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class VolatileOnStore : BaseVolatileAttribute
    {
        public VolatileOnStore(bool forcePush = false) : base(forcePush)
        {

        }
    }
}
