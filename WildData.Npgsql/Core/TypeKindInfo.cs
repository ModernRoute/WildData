using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Npgsql.Core
{
    class TypeKindInfo : ITypeKindInfo
    {
        public bool IsSupported(TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Binary:
                case TypeKind.Boolean:
                case TypeKind.BooleanNullable:
                case TypeKind.DateTime:
                case TypeKind.DateTimeNullable:
                case TypeKind.DateTimeOffset:
                case TypeKind.DateTimeOffsetNullable:
                case TypeKind.Decimal:
                case TypeKind.DecimalNullable:
                case TypeKind.Double:
                case TypeKind.DoubleNullable:
                case TypeKind.Float:
                case TypeKind.FloatNullable:
                case TypeKind.Guid:
                case TypeKind.GuidNullable:
                case TypeKind.Int16:
                case TypeKind.Int16Nullable:
                case TypeKind.Int32:
                case TypeKind.Int32Nullable:
                case TypeKind.Int64:
                case TypeKind.Int64Nullable:
                case TypeKind.AnyNullable:
                case TypeKind.String:
                    return true;
                case TypeKind.Byte:
                case TypeKind.ByteNullable:
                    return false;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
