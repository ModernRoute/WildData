using ModernRoute.WildData.Core;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernRoute.WildData.Npgsql.Extensions
{
    public static class ReturnTypeExtensions
    {
        public static NpgsqlDbType GetNpgsqlType(this ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Binary:
                    return NpgsqlDbType.Bytea;
                case ReturnType.Byte:
                case ReturnType.ByteNullable:
                    return NpgsqlDbType.Smallint;
                case ReturnType.DateTimeOffset:
                case ReturnType.DateTimeOffsetNullable:
                    return NpgsqlDbType.TimestampTZ;
                case ReturnType.DateTime:
                case ReturnType.DateTimeNullable:
                    return NpgsqlDbType.Timestamp;
                case ReturnType.Float:
                case ReturnType.FloatNullable:
                    return NpgsqlDbType.Real;
                case ReturnType.Double:
                case ReturnType.DoubleNullable:
                    return NpgsqlDbType.Double;
                case ReturnType.Decimal:
                case ReturnType.DecimalNullable:
                    return NpgsqlDbType.Numeric;
                case ReturnType.Int32:
                case ReturnType.Int32Nullable:
                    return NpgsqlDbType.Integer;
                case ReturnType.Int16:
                case ReturnType.Int16Nullable:
                    return NpgsqlDbType.Smallint;
                case ReturnType.Int64:
                case ReturnType.Int64Nullable:
                    return NpgsqlDbType.Bigint;
                case ReturnType.Guid:
                case ReturnType.GuidNullable:
                    return NpgsqlDbType.Uuid;
                case ReturnType.Boolean:
                case ReturnType.BooleanNullable:
                    return NpgsqlDbType.Boolean;
                case ReturnType.String:
                    return NpgsqlDbType.Varchar;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReturnTypeIsNotSupported, returnType));
            }
        }
    }
}
