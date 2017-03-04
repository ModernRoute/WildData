using ModernRoute.WildData.Resources;
using System;
using System.Globalization;

namespace ModernRoute.WildData.Core
{
    public class TypeKindNotSupported : Exception
    {
        public TypeKind TypeKind { get; private set; }

        public TypeKindNotSupported(TypeKind typeKind) : base(string.Format(CultureInfo.CurrentCulture, Strings.TypeIsNotSupported, typeKind))
        {
            TypeKind = typeKind;
        }
    }
}
