using System;

namespace ModernRoute.WildData.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class IgnoreAttribute : Attribute
    {

    }
}
