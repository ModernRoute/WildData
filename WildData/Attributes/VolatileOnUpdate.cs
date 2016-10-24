using System;

namespace ModernRoute.WildData.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class VolatileOnUpdate : BaseVolatileAttribute
    {
        public VolatileOnUpdate(bool forcePush = false) : base(forcePush)
        {

        }
    }
}
