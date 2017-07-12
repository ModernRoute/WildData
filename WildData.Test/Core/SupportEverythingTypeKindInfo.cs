using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Test.Core
{
    class SupportEverythingTypeKindInfo : ITypeKindInfo
    {
        public bool IsSupported(TypeKind typeKind)
        {
            return true;
        }
    }
}
