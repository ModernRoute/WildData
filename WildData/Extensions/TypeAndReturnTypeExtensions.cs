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
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetBytes));
                case ReturnType.Byte:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetByte));
                case ReturnType.ByteNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetByteNullable));
                case ReturnType.DateTimeOffset:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTimeOffset));
                case ReturnType.DateTimeOffsetNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTimeOffsetNullable));
                case ReturnType.DateTime:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTime));
                case ReturnType.DateTimeNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTimeNullable));
                case ReturnType.Float:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetFloat));
                case ReturnType.FloatNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetFloatNullable));
                case ReturnType.Double:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDouble));
                case ReturnType.DoubleNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDoubleNullable));
                case ReturnType.Decimal:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDecimal));
                case ReturnType.DecimalNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDecimalNullable));
                case ReturnType.Int32:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetInt));
                case ReturnType.Int32Nullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetIntNullable));
                case ReturnType.Int16:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetShort));
                case ReturnType.Int16Nullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetShortNullable));
                case ReturnType.Int64:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetLong));
                case ReturnType.Int64Nullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetLongNullable));
                case ReturnType.Guid:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetGuid));
                case ReturnType.GuidNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetGuidNullable));
                case ReturnType.Boolean:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetBoolean));
                case ReturnType.BooleanNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetBooleanNullable));
                case ReturnType.String:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetString));
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
