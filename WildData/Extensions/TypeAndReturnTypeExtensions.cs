using ModernRoute.WildData.Core;
using ModernRoute.WildData.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ModernRoute.WildData.Extensions
{
    public static class TypeAndReturnTypeExtensions
    {
        private const string _ReaderWrapperGetBytesMethodName = "GetBytes";
        private const string _ReaderWrapperGetByteMethodName = "GetByte";
        private const string _ReaderWrapperGetByteNullableMethodName = "GetByteNullable";
        private const string _ReaderWrapperGetDateTimeOffsetMethodName = "GetDateTimeOffset";
        private const string _ReaderWrapperGetDateTimeOffsetNullableMethodName = "GetDateTimeOffsetNullable";
        private const string _ReaderWrapperGetDateTimeMethodName = "GetDateTime";
        private const string _ReaderWrapperGetDateTimeNullableMethodName = "GetDateTimeNullable";
        private const string _ReaderWrapperGetFloatMethodName = "GetFloat";
        private const string _ReaderWrapperGetFloatNullableMethodName = "GetFloatNullable";
        private const string _ReaderWrapperGetDoubleMethodName = "GetDouble";
        private const string _ReaderWrapperGetDoubleNullableMethodName = "GetDoubleNullable";
        private const string _ReaderWrapperGetDecimalMethodName = "GetDecimal";
        private const string _ReaderWrapperGetDecimalNullableMethodName = "GetDecimalNullable";
        private const string _ReaderWrapperGetIntMethodName = "GetInt";
        private const string _ReaderWrapperGetIntNullableMethodName = "GetIntNullable";
        private const string _ReaderWrapperGetShortMethodName = "GetShort";
        private const string _ReaderWrapperGetShortNullableMethodName = "GetShortNullable";
        private const string _ReaderWrapperGetLongMethodName = "GetLong";
        private const string _ReaderWrapperGetLongNullableMethodName = "GetLongNullable";
        private const string _ReaderWrapperGetGuidMethodName = "GetGuid";
        private const string _ReaderWrapperGetGuidNullableMethodName = "GetGuidNullable";
        private const string _ReaderWrapperGetBooleanMethodName = "GetBoolean";
        private const string _ReaderWrapperGetBooleanNullableMethodName = "GetBooleanNullable";
        private const string _ReaderWrapperGetStringMethodName = "GetString";

        private static IReadOnlyDictionary<Type, ReturnType> _Map;

        static TypeAndReturnTypeExtensions()
        {
            IDictionary<Type, ReturnType> map = new Dictionary<Type, ReturnType>();

            map.Add(typeof(byte), ReturnType.Byte);
            map.Add(typeof(byte?), ReturnType.ByteNullable);
            map.Add(typeof(int), ReturnType.Int32);
            map.Add(typeof(int?), ReturnType.Int32Nullable);
            map.Add(typeof(long), ReturnType.Int64);
            map.Add(typeof(long?), ReturnType.Int64Nullable);
            map.Add(typeof(short), ReturnType.Int16);
            map.Add(typeof(short?), ReturnType.Int16Nullable);
            map.Add(typeof(string), ReturnType.String);
            map.Add(typeof(Guid), ReturnType.Guid);
            map.Add(typeof(Guid?), ReturnType.GuidNullable);
            map.Add(typeof(float), ReturnType.Float);
            map.Add(typeof(float?), ReturnType.FloatNullable);
            map.Add(typeof(double), ReturnType.Double);
            map.Add(typeof(double?), ReturnType.DoubleNullable);
            map.Add(typeof(decimal), ReturnType.Decimal);
            map.Add(typeof(decimal?), ReturnType.DecimalNullable);
            map.Add(typeof(bool), ReturnType.Boolean);
            map.Add(typeof(bool?), ReturnType.BooleanNullable);
            map.Add(typeof(byte[]), ReturnType.Binary);
            map.Add(typeof(DateTime), ReturnType.DateTime);
            map.Add(typeof(DateTime?), ReturnType.DateTimeNullable);
            map.Add(typeof(DateTimeOffset), ReturnType.DateTimeOffset);
            map.Add(typeof(DateTimeOffset?), ReturnType.DateTimeOffsetNullable);

            _Map = map.AsReadOnly();
        }

        public static bool IsSizeableType(this ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.String:
                case ReturnType.Binary:
                    return true;
                case ReturnType.Byte:
                case ReturnType.DateTimeOffset:
                case ReturnType.DateTime:
                case ReturnType.Float:
                case ReturnType.Double:
                case ReturnType.Decimal:
                case ReturnType.Int32:
                case ReturnType.Int16:
                case ReturnType.Int64:
                case ReturnType.Guid:
                case ReturnType.Boolean:
                case ReturnType.ByteNullable:
                case ReturnType.DateTimeOffsetNullable:
                case ReturnType.DateTimeNullable:
                case ReturnType.FloatNullable:
                case ReturnType.DoubleNullable:
                case ReturnType.DecimalNullable:
                case ReturnType.Int32Nullable:
                case ReturnType.Int16Nullable:
                case ReturnType.Int64Nullable:
                case ReturnType.GuidNullable:
                case ReturnType.BooleanNullable:
                    return false;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReturnTypeIsNotSupported, returnType));
            }
        }

        public static bool IsNotNullType(this ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Byte:
                case ReturnType.DateTimeOffset:
                case ReturnType.DateTime:
                case ReturnType.Float:
                case ReturnType.Double:
                case ReturnType.Decimal:
                case ReturnType.Int32:
                case ReturnType.Int16:
                case ReturnType.Int64:
                case ReturnType.Guid:
                case ReturnType.Boolean:
                    return true;
                case ReturnType.String:
                case ReturnType.Binary:
                case ReturnType.ByteNullable:
                case ReturnType.DateTimeOffsetNullable:
                case ReturnType.DateTimeNullable:
                case ReturnType.FloatNullable:
                case ReturnType.DoubleNullable:
                case ReturnType.DecimalNullable:
                case ReturnType.Int32Nullable:
                case ReturnType.Int16Nullable:
                case ReturnType.Int64Nullable:
                case ReturnType.GuidNullable:
                case ReturnType.BooleanNullable:
                    return false;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReturnTypeIsNotSupported, returnType));
            }
        }

        public static MethodInfo GetMethodByReturnType(this ReturnType returnType)
        {
            Type readerWrapperType = typeof(IReaderWrapper);

            switch (returnType)
            {
                case ReturnType.Binary:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetBytesMethodName);
                case ReturnType.Byte:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetByteMethodName);
                case ReturnType.ByteNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetByteNullableMethodName);
                case ReturnType.DateTimeOffset:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDateTimeOffsetMethodName);
                case ReturnType.DateTimeOffsetNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDateTimeOffsetNullableMethodName);
                case ReturnType.DateTime:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDateTimeMethodName);
                case ReturnType.DateTimeNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDateTimeNullableMethodName);
                case ReturnType.Float:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetFloatMethodName);
                case ReturnType.FloatNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetFloatNullableMethodName);
                case ReturnType.Double:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDoubleMethodName);
                case ReturnType.DoubleNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDoubleNullableMethodName);
                case ReturnType.Decimal:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDecimalMethodName);
                case ReturnType.DecimalNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetDecimalNullableMethodName);
                case ReturnType.Int32:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetIntMethodName);
                case ReturnType.Int32Nullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetIntNullableMethodName);
                case ReturnType.Int16:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetShortMethodName);
                case ReturnType.Int16Nullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetShortNullableMethodName);
                case ReturnType.Int64:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetLongMethodName);
                case ReturnType.Int64Nullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetLongNullableMethodName);
                case ReturnType.Guid:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetGuidMethodName);
                case ReturnType.GuidNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetGuidNullableMethodName);
                case ReturnType.Boolean:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetBooleanMethodName);
                case ReturnType.BooleanNullable:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetBooleanNullableMethodName);
                case ReturnType.String:
                    return readerWrapperType.GetMethod(_ReaderWrapperGetStringMethodName);
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ReturnTypeIsNotSupported, returnType));
            }
        }

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

        public static ReturnType GetReturnType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!_Map.ContainsKey(type))
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.TypeIsNotSupported, type));
            }

            return _Map[type];
        }

        public static Type GetCollectionElementType(this Type sequenceType)
        {
            Type enumerableType = FindIEnumerable(sequenceType);

            if (enumerableType == null)
            {
                return sequenceType;
            }

            return enumerableType.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type sequenceType)
        {
            if (sequenceType == null || sequenceType == typeof(string))
            {
                return null;
            }

            if (sequenceType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(sequenceType.GetElementType());
            }

            if (sequenceType.IsGenericType)
            {
                foreach (Type arg in sequenceType.GetGenericArguments())
                {
                    Type enumerableType = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (enumerableType.IsAssignableFrom(sequenceType))
                    {
                        return enumerableType;
                    }
                }
            }

            Type[] interfaceTypes = sequenceType.GetInterfaces();

            if (interfaceTypes != null && interfaceTypes.Length > 0)
            {
                foreach (Type interfaceType in interfaceTypes)
                {
                    Type enumerableType = FindIEnumerable(interfaceType);
                    if (enumerableType != null)
                    {
                        return enumerableType;
                    }
                }
            }

            if (sequenceType.BaseType != null && sequenceType.BaseType != typeof(object))
            {
                return FindIEnumerable(sequenceType.BaseType);
            }

            return null;
        }
    }
}
