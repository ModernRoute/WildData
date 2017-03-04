using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Globalization;
using System.Linq;

namespace ModernRoute.WildData.Linq
{
    public class QueryConstant : QueryExpression
    {
        private const string _LongFormatForDateTime = "X16";
        private const string _GuidFormat = "N";
        private const string _Separator = ", ";

        public string InvariantRepresentation
        {
            get;
            private set;
        }

        private static void ThrowCannotInterpretateAs(TypeKind type, Exception innerException = null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.UnableToInterpretateString, type), innerException);
        }

        private byte GetByte(TypeKind interpretationType)
        {
            byte value;

            if (!byte.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return value;
        }

        public byte GetByte()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Byte, TypeKind.ByteNullable);

            return GetByte(TypeKind.Byte);
        }

        public byte? GetByteNullable()
        {
            CheckType(TypeKind.Byte, TypeKind.ByteNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetByte(TypeKind.ByteNullable);
        }

        private DateTime GetDateTime(TypeKind interpretationType)
        {
            long value;

            if (!long.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return new DateTime(value);
        }

        public DateTime GetDateTime()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.DateTime, TypeKind.DateTimeNullable, TypeKind.DateTimeOffset, TypeKind.DateTimeOffsetNullable);

            if (Type == TypeKind.DateTimeOffset || Type == TypeKind.DateTimeOffsetNullable)
            {
                return GetDateTimeOffset(TypeKind.DateTime).UtcDateTime;
            }

            return GetDateTime(TypeKind.DateTime);
        }

        public DateTime? GetDateTimeNullable()
        {
            CheckType(TypeKind.DateTime, TypeKind.DateTimeNullable, TypeKind.DateTimeOffset, TypeKind.DateTimeOffsetNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            if (Type == TypeKind.DateTimeOffset || Type == TypeKind.DateTimeOffsetNullable)
            {
                return GetDateTimeOffset(TypeKind.DateTimeNullable).UtcDateTime;
            }

            return GetDateTime(TypeKind.DateTimeNullable);
        }

        private DateTimeOffset GetDateTimeOffset(TypeKind interpretationType)
        {
            int halfLength = InvariantRepresentation.Length / 2;

            string ticks = InvariantRepresentation.Substring(0, halfLength);
            string offset = InvariantRepresentation.Substring(halfLength, halfLength);

            long ticksValue;
            long offsetValue;

            bool ticksConverted = long.TryParse(ticks, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ticksValue);
            bool offsetConverted = long.TryParse(offset, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out offsetValue);

            if (!ticksConverted || !offsetConverted)
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return new DateTimeOffset(ticksValue, new TimeSpan(offsetValue));
        }

        public DateTimeOffset GetDateTimeOffset()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.DateTime, TypeKind.DateTimeNullable, TypeKind.DateTimeOffset, TypeKind.DateTimeOffsetNullable);

            if (Type == TypeKind.DateTime || Type == TypeKind.DateTimeNullable)
            {
                return GetDateTime(TypeKind.DateTimeOffset);
            }

            return GetDateTimeOffset(TypeKind.DateTimeOffset);
        }

        public DateTimeOffset? GetDateTimeOffsetNullable()
        {
            CheckType(TypeKind.DateTime, TypeKind.DateTimeNullable, TypeKind.DateTimeOffset, TypeKind.DateTimeOffsetNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            if (Type == TypeKind.DateTime || Type == TypeKind.DateTimeNullable)
            {
                return GetDateTime(TypeKind.DateTimeOffsetNullable);
            }

            return GetDateTimeOffset(TypeKind.DateTimeOffsetNullable);
        }

        private float GetFloat(TypeKind interpretationType)
        {
            int value;

            if (!int.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
        }

        public float GetFloat()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Float, TypeKind.FloatNullable);

            return GetFloat(TypeKind.Float);
        }

        public float? GetFloatNullable()
        {
            CheckType(TypeKind.Float, TypeKind.FloatNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetFloat(TypeKind.FloatNullable);
        }

        private double GetDouble(TypeKind interpretationType)
        {
            long value;

            if (!long.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return BitConverter.Int64BitsToDouble(value);
        }

        public double GetDouble()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Double, TypeKind.DoubleNullable, TypeKind.Float, TypeKind.FloatNullable);

            if (Type == TypeKind.Float || Type == TypeKind.FloatNullable)
            {
                return GetFloat(TypeKind.Double);
            }

            return GetDouble(TypeKind.Double);
        }

        public double? GetDoubleNullable()
        {
            CheckType(TypeKind.Double, TypeKind.DoubleNullable, TypeKind.Float, TypeKind.FloatNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            if (Type == TypeKind.Float || Type == TypeKind.FloatNullable)
            {
                return GetFloat(TypeKind.DoubleNullable);
            }

            return GetDouble(TypeKind.DoubleNullable);
        }

        private short GetShort(TypeKind interpretationType)
        {
            short value;

            if (!short.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return value;
        }

        public short GetShort()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Int16, TypeKind.Int16Nullable, TypeKind.Byte, TypeKind.ByteNullable);

            return GetShort(TypeKind.Int16);
        }

        public short? GetShortNullable()
        {
            CheckType(TypeKind.Int16, TypeKind.Int16Nullable, TypeKind.Byte, TypeKind.ByteNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetShort(TypeKind.Int16Nullable);
        }

        public int GetInt(TypeKind interpretationType)
        {
            int value;

            if (!int.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return value;
        }

        public int GetInt()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Int16, TypeKind.Int16Nullable, TypeKind.Byte, TypeKind.ByteNullable, TypeKind.Int32, TypeKind.Int32Nullable);

            return GetInt(TypeKind.Int32);
        }

        public int? GetIntNullable()
        {
            CheckType(TypeKind.Int16, TypeKind.Int16Nullable, TypeKind.Byte, 
                TypeKind.ByteNullable, TypeKind.Int32, TypeKind.Int32Nullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetInt(TypeKind.Int32Nullable);
        }

        private long GetLong(TypeKind interpretationType)
        {
            long value;

            if (!long.TryParse(InvariantRepresentation, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return value;
        }

        public long GetLong()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Int16, TypeKind.Int16Nullable, TypeKind.Byte, TypeKind.ByteNullable, 
                TypeKind.Int32, TypeKind.Int32Nullable, TypeKind.Int64, TypeKind.Int64Nullable);

            return GetLong(TypeKind.Int64);
        }

        public long? GetLongNullable()
        {
            CheckType(TypeKind.Int16, TypeKind.Int16Nullable, TypeKind.Byte, TypeKind.ByteNullable,
                TypeKind.Int32, TypeKind.Int32Nullable, TypeKind.Int64, TypeKind.Int64Nullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetLong(TypeKind.Int64);
        }

        public string GetString()
        {
            CheckType(TypeKind.String, TypeKind.Null);

            return InvariantRepresentation;
        }

        private Guid GetGuid(TypeKind interpretationType)
        {
            Guid value;

            if (!Guid.TryParse(InvariantRepresentation, out value))
            {
                ThrowCannotInterpretateAs(TypeKind.Guid);
            }

            return value;
        }

        public Guid GetGuid()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Guid, TypeKind.GuidNullable);

            return GetGuid(TypeKind.Guid);
        }

        public Guid? GetGuidNullable()
        {
            CheckType(TypeKind.Guid, TypeKind.GuidNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetGuid(TypeKind.GuidNullable);
        }

        public byte[] GetBytes()
        {
            CheckType(TypeKind.Binary, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            byte[] value = null;

            try
            {
                value = Convert.FromBase64String(InvariantRepresentation);
            }
            catch (FormatException ex)
            {
                ThrowCannotInterpretateAs(TypeKind.Binary, ex);
            }

            return value;
        }

        private bool GetBoolean(TypeKind interpretationType)
        {
            bool value;

            if (!bool.TryParse(InvariantRepresentation, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return value;
        }

        public bool GetBoolean()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Boolean, TypeKind.BooleanNullable);

            return GetBoolean(TypeKind.Boolean);
        }

        public bool? GetBooleanNullable()
        {
            CheckType(TypeKind.Boolean, TypeKind.BooleanNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetBoolean(TypeKind.BooleanNullable);
        }

        private decimal GetDecimal(TypeKind interpretationType)
        {
            decimal value;

            if (!decimal.TryParse(InvariantRepresentation, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                ThrowCannotInterpretateAs(interpretationType);
            }

            return value;
        }

        public decimal GetDecimal()
        {
            CheckRepresentationNull();

            CheckType(TypeKind.Decimal, TypeKind.DecimalNullable);

            return GetDecimal(TypeKind.Decimal);
        }

        public decimal? GetDecimalNullable()
        {
            CheckType(TypeKind.Decimal, TypeKind.DecimalNullable, TypeKind.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetDecimal(TypeKind.DecimalNullable);
        }

        private void CheckRepresentationNull()
        {
            if (InvariantRepresentation == null)
            {
                throw new InvalidOperationException(Resources.Strings.ConstantValueIsNullButNullIsUnexpected);
            }
        }

        private void CheckType(params TypeKind[] typeKinds)
        {
            if (!typeKinds.Any(t => t == Type))
            {
                throw new InvalidOperationException(
                    string.Format(
                    Resources.Strings.ConstantTypeIsNotExpected, 
                    Type, string.Join(_Separator, typeKinds.Select(rt => rt.ToString()).ToArray())));
            }
        }

        public static QueryConstant Create(object value)
        {
            if (value == null)
            {
                return CreateNull();
            }

            TypeKind typeKind = value.GetType().GetTypeKind();

            switch (typeKind)
            {
                case TypeKind.Binary:
                    return CreateBinary((byte[])value);
                case TypeKind.Boolean:
                    return CreateBoolean((bool)value);
                case TypeKind.BooleanNullable:
                    return CreateBooleanNullable((bool?)value);
                case TypeKind.Float:
                    return CreateFloat((float)value);
                case TypeKind.FloatNullable:
                    return CreateFloatNullable((float?)value);
                case TypeKind.Double:
                    return CreateDouble((double)value);
                case TypeKind.DoubleNullable:
                    return CreateDoubleNullable((double?)value);
                case TypeKind.Decimal:
                    return CreateDecimal((decimal)value);
                case TypeKind.DecimalNullable:
                    return CreateDecimalNullable((decimal?)value);
                case TypeKind.Int16:
                    return CreateShort((short)value);
                case TypeKind.Int16Nullable:
                    return CreateShortNullable((short?)value);
                case TypeKind.Int32:
                    return CreateInt((int)value);
                case TypeKind.Int32Nullable:
                    return CreateIntNullable((int?)value);
                case TypeKind.Int64:
                    return CreateLong((long)value);
                case TypeKind.Int64Nullable:
                    return CreateLongNullable((long?)value);
                case TypeKind.Byte:
                    return CreateByte((byte)value);
                case TypeKind.ByteNullable:
                    return CreateByteNullable((byte?)value);
                case TypeKind.String:
                    return CreateString((string)value);
                case TypeKind.DateTime:
                    return CreateDateTime((DateTime)value);
                case TypeKind.DateTimeNullable:
                    return CreateDateTimeNullable((DateTime?)value);
                case TypeKind.DateTimeOffset:
                    return CreateDateTimeOffset((DateTimeOffset)value);
                case TypeKind.DateTimeOffsetNullable:
                    return CreateDateTimeOffsetNullable((DateTimeOffset?)value);
                default:
                    throw new NotSupportedException(string.Format(Resources.Strings.TypeIsNotSupported, typeKind));
            }
        }

        public static QueryConstant CreateNull()
        {
            return new QueryConstant(null,TypeKind.Null);
        }

        private static QueryConstant CreateFloat(float value, TypeKind interpretationType)
        {
            string stringValue = BitConverter.ToInt32(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture);
            return new QueryConstant(stringValue, interpretationType);
        }

        public static QueryConstant CreateFloat(float value)
        {
            return CreateFloat(value, TypeKind.Float);
        }

        public static QueryConstant CreateFloatNullable(float? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.FloatNullable);
            }

            return CreateFloat(value.Value, TypeKind.FloatNullable);
        }

        private static QueryConstant CreateDouble(double value, TypeKind interpretationType)
        {
            string stringValue = BitConverter.DoubleToInt64Bits(value).ToString(CultureInfo.InvariantCulture);
            return new QueryConstant(stringValue, interpretationType);
        }

        public static QueryConstant CreateDouble(double value)
        {
            return CreateDouble(value, TypeKind.Double);
        }

        public static QueryConstant CreateDoubleNullable(double? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.DoubleNullable);
            }

            return CreateDouble(value.Value, TypeKind.DoubleNullable);
        }

        private static QueryConstant CreateInt(int value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateInt(int value)
        {
            return CreateInt(value, TypeKind.Int32);
        }

        public static QueryConstant CreateIntNullable(int? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.Int32Nullable);
            }

            return CreateInt(value.Value, TypeKind.Int32Nullable);
        }

        private static QueryConstant CreateLong(long value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateLong(long value)
        {
            return CreateLong(value, TypeKind.Int64);
        }

        public static QueryConstant CreateLongNullable(long? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.Int64Nullable);
            }

            return CreateLong(value.Value, TypeKind.Int64Nullable);
        }

        private static QueryConstant CreateShort(short value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateShort(short value)
        {
            return CreateShort(value, TypeKind.Int16);
        }

        public static QueryConstant CreateShortNullable(short? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.Int16Nullable);
            }

            return CreateShort(value.Value, TypeKind.Int16Nullable);
        }

        private static QueryConstant CreateByte(byte value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateByte(byte value)
        {
            return CreateByte(value, TypeKind.Byte);
        }

        public static QueryConstant CreateByteNullable(byte? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.ByteNullable);
            }

            return CreateByte(value.Value, TypeKind.ByteNullable);
        }

        private static QueryConstant CreateBoolean(bool value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateBoolean(bool value)
        {
            return CreateBoolean(value, TypeKind.Boolean);
        }

        public static QueryConstant CreateBooleanNullable(bool? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.BooleanNullable);
            }

            return CreateBoolean(value.Value, TypeKind.BooleanNullable);
        }

        private static QueryConstant CreateDateTime(DateTime value, TypeKind interpretationType)
        {
            return new QueryConstant(value.Ticks.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateDateTime(DateTime value)
        {
            return CreateDateTime(value, TypeKind.DateTime);
        }

        public static QueryConstant CreateDateTimeNullable(DateTime? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.DateTimeNullable);
            }

            return CreateDateTime(value.Value, TypeKind.DateTimeNullable);
        }

        private static QueryConstant CreateDateTimeOffset(DateTimeOffset value, TypeKind interpretationType)
        {
            string ticks = value.Ticks.ToString(_LongFormatForDateTime, CultureInfo.InvariantCulture);
            string span = value.Offset.Ticks.ToString(_LongFormatForDateTime, CultureInfo.InvariantCulture);

            return new QueryConstant(string.Concat(ticks, span), interpretationType);
        }

        public static QueryConstant CreateDateTimeOffset(DateTimeOffset value)
        {
            return CreateDateTimeOffset(value, TypeKind.DateTimeOffset);
        }

        public static QueryConstant CreateDateTimeOffsetNullable(DateTimeOffset? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.DateTimeOffsetNullable);
            }

            return CreateDateTimeOffset(value.Value, TypeKind.DateTimeOffsetNullable);
        }

        private static QueryConstant CreateGuid(Guid value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(_GuidFormat, CultureInfo.InvariantCulture), interpretationType);  
        }

        public static QueryConstant CreateGuid(Guid value)
        {
            return CreateGuid(value, TypeKind.Guid);
        }

        public static QueryConstant CreateGuidNullable(Guid? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.GuidNullable);
            }

            return CreateGuid(value.Value, TypeKind.GuidNullable);
        }

        private static QueryConstant CreateDecimal(decimal value, TypeKind interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateDecimal(decimal value)
        {
            return CreateDecimal(value, TypeKind.Decimal);
        }

        public static QueryConstant CreateDecimalNullable(decimal? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.DecimalNullable);
            }

            return CreateDecimal(value.Value, TypeKind.DecimalNullable);
        }

        public static QueryConstant CreateString(string value)
        {
            return new QueryConstant(value, TypeKind.String);
        }

        public static QueryConstant CreateBinary(byte[] value)
        {
            if (value == null)
            {
                return new QueryConstant(null, TypeKind.Binary);
            }

            return new QueryConstant(Convert.ToBase64String(value), TypeKind.Binary);
        }

        private QueryConstant(string invariantRepresentation, TypeKind typeKind) 
            : base(typeKind)
        {
            InvariantRepresentation = invariantRepresentation;
        }

        public override QueryExpressionType ExpressionType
        {
            get
            {
                return QueryExpressionType.Constant;
            }
        }
    }
}
