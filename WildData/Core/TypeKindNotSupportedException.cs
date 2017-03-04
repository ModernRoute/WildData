using ModernRoute.WildData.Resources;
using System;
using System.Globalization;

namespace ModernRoute.WildData.Core
{
    public class TypeKindNotSupportedException : Exception
    {
        public TypeKind TypeKind { get; private set; }

        public TypeKindNotSupportedException(TypeKind typeKind) : base(string.Format(CultureInfo.CurrentCulture, Strings.TypeIsNotSupported, typeKind))
        {
            TypeKind = typeKind;
        }
    }
}
