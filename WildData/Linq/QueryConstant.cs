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

        private static void ThrowCannotInterpretateAs(ReturnType type, Exception innerException = null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.UnableToInterpretateString, type), innerException);
        }

        private byte GetByte(ReturnType interpretationType)
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

            CheckType(ReturnType.Byte, ReturnType.ByteNullable);

            return GetByte(ReturnType.Byte);
        }

        public byte? GetByteNullable()
        {
            CheckType(ReturnType.Byte, ReturnType.ByteNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetByte(ReturnType.ByteNullable);
        }

        private DateTime GetDateTime(ReturnType interpretationType)
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

            CheckType(ReturnType.DateTime, ReturnType.DateTimeNullable, ReturnType.DateTimeOffset, ReturnType.DateTimeOffsetNullable);

            if (Type == ReturnType.DateTimeOffset || Type == ReturnType.DateTimeOffsetNullable)
            {
                return GetDateTimeOffset(ReturnType.DateTime).UtcDateTime;
            }

            return GetDateTime(ReturnType.DateTime);
        }

        public DateTime? GetDateTimeNullable()
        {
            CheckType(ReturnType.DateTime, ReturnType.DateTimeNullable, ReturnType.DateTimeOffset, ReturnType.DateTimeOffsetNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            if (Type == ReturnType.DateTimeOffset || Type == ReturnType.DateTimeOffsetNullable)
            {
                return GetDateTimeOffset(ReturnType.DateTimeNullable).UtcDateTime;
            }

            return GetDateTime(ReturnType.DateTimeNullable);
        }

        private DateTimeOffset GetDateTimeOffset(ReturnType interpretationType)
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

            CheckType(ReturnType.DateTime, ReturnType.DateTimeNullable, ReturnType.DateTimeOffset, ReturnType.DateTimeOffsetNullable);

            if (Type == ReturnType.DateTime || Type == ReturnType.DateTimeNullable)
            {
                return GetDateTime(ReturnType.DateTimeOffset);
            }

            return GetDateTimeOffset(ReturnType.DateTimeOffset);
        }

        public DateTimeOffset? GetDateTimeOffsetNullable()
        {
            CheckType(ReturnType.DateTime, ReturnType.DateTimeNullable, ReturnType.DateTimeOffset, ReturnType.DateTimeOffsetNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            if (Type == ReturnType.DateTime || Type == ReturnType.DateTimeNullable)
            {
                return GetDateTime(ReturnType.DateTimeOffsetNullable);
            }

            return GetDateTimeOffset(ReturnType.DateTimeOffsetNullable);
        }

        private float GetFloat(ReturnType interpretationType)
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

            CheckType(ReturnType.Float, ReturnType.FloatNullable);

            return GetFloat(ReturnType.Float);
        }

        public float? GetFloatNullable()
        {
            CheckType(ReturnType.Float, ReturnType.FloatNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetFloat(ReturnType.FloatNullable);
        }

        private double GetDouble(ReturnType interpretationType)
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

            CheckType(ReturnType.Double, ReturnType.DoubleNullable, ReturnType.Float, ReturnType.FloatNullable);

            if (Type == ReturnType.Float || Type == ReturnType.FloatNullable)
            {
                return GetFloat(ReturnType.Double);
            }

            return GetDouble(ReturnType.Double);
        }

        public double? GetDoubleNullable()
        {
            CheckType(ReturnType.Double, ReturnType.DoubleNullable, ReturnType.Float, ReturnType.FloatNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            if (Type == ReturnType.Float || Type == ReturnType.FloatNullable)
            {
                return GetFloat(ReturnType.DoubleNullable);
            }

            return GetDouble(ReturnType.DoubleNullable);
        }

        private short GetShort(ReturnType interpretationType)
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

            CheckType(ReturnType.Int16, ReturnType.Int16Nullable, ReturnType.Byte, ReturnType.ByteNullable);

            return GetShort(ReturnType.Int16);
        }

        public short? GetShortNullable()
        {
            CheckType(ReturnType.Int16, ReturnType.Int16Nullable, ReturnType.Byte, ReturnType.ByteNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetShort(ReturnType.Int16Nullable);
        }

        public int GetInt(ReturnType interpretationType)
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

            CheckType(ReturnType.Int16, ReturnType.Int16Nullable, ReturnType.Byte, ReturnType.ByteNullable, ReturnType.Int32, ReturnType.Int32Nullable);

            return GetInt(ReturnType.Int32);
        }

        public int? GetIntNullable()
        {
            CheckType(ReturnType.Int16, ReturnType.Int16Nullable, ReturnType.Byte, 
                ReturnType.ByteNullable, ReturnType.Int32, ReturnType.Int32Nullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetInt(ReturnType.Int32Nullable);
        }

        private long GetLong(ReturnType interpretationType)
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

            CheckType(ReturnType.Int16, ReturnType.Int16Nullable, ReturnType.Byte, ReturnType.ByteNullable, 
                ReturnType.Int32, ReturnType.Int32Nullable, ReturnType.Int64, ReturnType.Int64Nullable);

            return GetLong(ReturnType.Int64);
        }

        public long? GetLongNullable()
        {
            CheckType(ReturnType.Int16, ReturnType.Int16Nullable, ReturnType.Byte, ReturnType.ByteNullable,
                ReturnType.Int32, ReturnType.Int32Nullable, ReturnType.Int64, ReturnType.Int64Nullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetLong(ReturnType.Int64);
        }

        public string GetString()
        {
            CheckType(ReturnType.String, ReturnType.Null);

            return InvariantRepresentation;
        }

        private Guid GetGuid(ReturnType interpretationType)
        {
            Guid value;

            if (!Guid.TryParse(InvariantRepresentation, out value))
            {
                ThrowCannotInterpretateAs(ReturnType.Guid);
            }

            return value;
        }

        public Guid GetGuid()
        {
            CheckRepresentationNull();

            CheckType(ReturnType.Guid, ReturnType.GuidNullable);

            return GetGuid(ReturnType.Guid);
        }

        public Guid? GetGuidNullable()
        {
            CheckType(ReturnType.Guid, ReturnType.GuidNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetGuid(ReturnType.GuidNullable);
        }

        public byte[] GetBytes()
        {
            CheckType(ReturnType.Binary, ReturnType.Null);

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
                ThrowCannotInterpretateAs(ReturnType.Binary, ex);
            }

            return value;
        }

        private bool GetBoolean(ReturnType interpretationType)
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

            CheckType(ReturnType.Boolean, ReturnType.BooleanNullable);

            return GetBoolean(ReturnType.Boolean);
        }

        public bool? GetBooleanNullable()
        {
            CheckType(ReturnType.Boolean, ReturnType.BooleanNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetBoolean(ReturnType.BooleanNullable);
        }

        private decimal GetDecimal(ReturnType interpretationType)
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

            CheckType(ReturnType.Decimal, ReturnType.DecimalNullable);

            return GetDecimal(ReturnType.Decimal);
        }

        public decimal? GetDecimalNullable()
        {
            CheckType(ReturnType.Decimal, ReturnType.DecimalNullable, ReturnType.Null);

            if (InvariantRepresentation == null)
            {
                return null;
            }

            return GetDecimal(ReturnType.DecimalNullable);
        }

        private void CheckRepresentationNull()
        {
            if (InvariantRepresentation == null)
            {
                throw new InvalidOperationException(Resources.Strings.ConstantValueIsNullButNullIsUnexpected);
            }
        }

        private void CheckType(params ReturnType[] returnTypes)
        {
            if (!returnTypes.Any(t => t == Type))
            {
                throw new InvalidOperationException(
                    string.Format(
                    Resources.Strings.ConstantTypeIsNotExpected, 
                    Type, string.Join(_Separator, returnTypes.Select(rt => rt.ToString()).ToArray())));
            }
        }

        public static QueryConstant Create(object value)
        {
            if (value == null)
            {
                return CreateNull();
            }

            ReturnType returnType = value.GetType().GetReturnType();

            switch (returnType)
            {
                case ReturnType.Binary:
                    return CreateBinary((byte[])value);
                case ReturnType.Boolean:
                    return CreateBoolean((bool)value);
                case ReturnType.BooleanNullable:
                    return CreateBooleanNullable((bool?)value);
                case ReturnType.Float:
                    return CreateFloat((float)value);
                case ReturnType.FloatNullable:
                    return CreateFloatNullable((float?)value);
                case ReturnType.Double:
                    return CreateDouble((double)value);
                case ReturnType.DoubleNullable:
                    return CreateDoubleNullable((double?)value);
                case ReturnType.Decimal:
                    return CreateDecimal((decimal)value);
                case ReturnType.DecimalNullable:
                    return CreateDecimalNullable((decimal?)value);
                case ReturnType.Int16:
                    return CreateShort((short)value);
                case ReturnType.Int16Nullable:
                    return CreateShortNullable((short?)value);
                case ReturnType.Int32:
                    return CreateInt((int)value);
                case ReturnType.Int32Nullable:
                    return CreateIntNullable((int?)value);
                case ReturnType.Int64:
                    return CreateLong((long)value);
                case ReturnType.Int64Nullable:
                    return CreateLongNullable((long?)value);
                case ReturnType.Byte:
                    return CreateByte((byte)value);
                case ReturnType.ByteNullable:
                    return CreateByteNullable((byte?)value);
                case ReturnType.String:
                    return CreateString((string)value);
                case ReturnType.DateTime:
                    return CreateDateTime((DateTime)value);
                case ReturnType.DateTimeNullable:
                    return CreateDateTimeNullable((DateTime?)value);
                case ReturnType.DateTimeOffset:
                    return CreateDateTimeOffset((DateTimeOffset)value);
                case ReturnType.DateTimeOffsetNullable:
                    return CreateDateTimeOffsetNullable((DateTimeOffset?)value);
                default:
                    throw new NotSupportedException(string.Format(Resources.Strings.TypeIsNotSupported, returnType));
            }
        }

        public static QueryConstant CreateNull()
        {
            return new QueryConstant(null,ReturnType.Null);
        }

        private static QueryConstant CreateFloat(float value, ReturnType interpretationType)
        {
            string stringValue = BitConverter.ToInt32(BitConverter.GetBytes(value), 0).ToString(CultureInfo.InvariantCulture);
            return new QueryConstant(stringValue, interpretationType);
        }

        public static QueryConstant CreateFloat(float value)
        {
            return CreateFloat(value, ReturnType.Float);
        }

        public static QueryConstant CreateFloatNullable(float? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.FloatNullable);
            }

            return CreateFloat(value.Value, ReturnType.FloatNullable);
        }

        private static QueryConstant CreateDouble(double value, ReturnType interpretationType)
        {
            string stringValue = BitConverter.DoubleToInt64Bits(value).ToString(CultureInfo.InvariantCulture);
            return new QueryConstant(stringValue, interpretationType);
        }

        public static QueryConstant CreateDouble(double value)
        {
            return CreateDouble(value, ReturnType.Double);
        }

        public static QueryConstant CreateDoubleNullable(double? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.DoubleNullable);
            }

            return CreateDouble(value.Value, ReturnType.DoubleNullable);
        }

        private static QueryConstant CreateInt(int value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateInt(int value)
        {
            return CreateInt(value, ReturnType.Int32);
        }

        public static QueryConstant CreateIntNullable(int? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.Int32Nullable);
            }

            return CreateInt(value.Value, ReturnType.Int32Nullable);
        }

        private static QueryConstant CreateLong(long value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateLong(long value)
        {
            return CreateLong(value, ReturnType.Int64);
        }

        public static QueryConstant CreateLongNullable(long? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.Int64Nullable);
            }

            return CreateLong(value.Value, ReturnType.Int64Nullable);
        }

        private static QueryConstant CreateShort(short value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateShort(short value)
        {
            return CreateShort(value, ReturnType.Int16);
        }

        public static QueryConstant CreateShortNullable(short? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.Int16Nullable);
            }

            return CreateShort(value.Value, ReturnType.Int16Nullable);
        }

        private static QueryConstant CreateByte(byte value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateByte(byte value)
        {
            return CreateByte(value, ReturnType.Byte);
        }

        public static QueryConstant CreateByteNullable(byte? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.ByteNullable);
            }

            return CreateByte(value.Value, ReturnType.ByteNullable);
        }

        private static QueryConstant CreateBoolean(bool value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateBoolean(bool value)
        {
            return CreateBoolean(value, ReturnType.Boolean);
        }

        public static QueryConstant CreateBooleanNullable(bool? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.BooleanNullable);
            }

            return CreateBoolean(value.Value, ReturnType.BooleanNullable);
        }

        private static QueryConstant CreateDateTime(DateTime value, ReturnType interpretationType)
        {
            return new QueryConstant(value.Ticks.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateDateTime(DateTime value)
        {
            return CreateDateTime(value, ReturnType.DateTime);
        }

        public static QueryConstant CreateDateTimeNullable(DateTime? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.DateTimeNullable);
            }

            return CreateDateTime(value.Value, ReturnType.DateTimeNullable);
        }

        private static QueryConstant CreateDateTimeOffset(DateTimeOffset value, ReturnType interpretationType)
        {
            string ticks = value.Ticks.ToString(_LongFormatForDateTime, CultureInfo.InvariantCulture);
            string span = value.Offset.Ticks.ToString(_LongFormatForDateTime, CultureInfo.InvariantCulture);

            return new QueryConstant(string.Concat(ticks, span), interpretationType);
        }

        public static QueryConstant CreateDateTimeOffset(DateTimeOffset value)
        {
            return CreateDateTimeOffset(value, ReturnType.DateTimeOffset);
        }

        public static QueryConstant CreateDateTimeOffsetNullable(DateTimeOffset? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.DateTimeOffsetNullable);
            }

            return CreateDateTimeOffset(value.Value, ReturnType.DateTimeOffsetNullable);
        }

        private static QueryConstant CreateGuid(Guid value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(_GuidFormat, CultureInfo.InvariantCulture), interpretationType);  
        }

        public static QueryConstant CreateGuid(Guid value)
        {
            return CreateGuid(value, ReturnType.Guid);
        }

        public static QueryConstant CreateGuidNullable(Guid? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.GuidNullable);
            }

            return CreateGuid(value.Value, ReturnType.GuidNullable);
        }

        private static QueryConstant CreateDecimal(decimal value, ReturnType interpretationType)
        {
            return new QueryConstant(value.ToString(CultureInfo.InvariantCulture), interpretationType);
        }

        public static QueryConstant CreateDecimal(decimal value)
        {
            return CreateDecimal(value, ReturnType.Decimal);
        }

        public static QueryConstant CreateDecimalNullable(decimal? value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.DecimalNullable);
            }

            return CreateDecimal(value.Value, ReturnType.DecimalNullable);
        }

        public static QueryConstant CreateString(string value)
        {
            return new QueryConstant(value, ReturnType.String);
        }

        public static QueryConstant CreateBinary(byte[] value)
        {
            if (value == null)
            {
                return new QueryConstant(null, ReturnType.Binary);
            }

            return new QueryConstant(Convert.ToBase64String(value), ReturnType.Binary);
        }

        private QueryConstant(string invariantRepresentation, ReturnType returnType) 
            : base(returnType)
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
