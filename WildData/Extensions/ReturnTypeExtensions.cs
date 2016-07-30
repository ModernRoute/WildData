using ModernRoute.WildData.Core;
using ModernRoute.WildData.Linq.Tree.Expression;

namespace ModernRoute.WildData.Extensions
{
    public static class ReturnTypeExtensions
    {
        public static bool IsNullable(this ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Binary:
                case ReturnType.BooleanNullable:
                case ReturnType.ByteNullable:
                case ReturnType.DateTimeNullable:
                case ReturnType.DateTimeOffsetNullable:
                case ReturnType.DecimalNullable:
                case ReturnType.DoubleNullable:
                case ReturnType.FloatNullable:
                case ReturnType.GuidNullable:
                case ReturnType.Int16Nullable:
                case ReturnType.Int32Nullable:
                case ReturnType.Int64Nullable:
                case ReturnType.Null:
                case ReturnType.String:
                    return true;
                default:
                    return false;
            }
        }
    }
}
