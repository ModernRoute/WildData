using ModernRoute.WildData.Core;
using ModernRoute.WildData.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ModernRoute.WildData.Extensions
{
    public static class TypeAndTypeKindExtensions
    {
        private static IReadOnlyDictionary<Type, TypeKind> _Map;

        static TypeAndTypeKindExtensions()
        {
            IDictionary<Type, TypeKind> map = new Dictionary<Type, TypeKind>();

            map.Add(typeof(byte), TypeKind.Byte);
            map.Add(typeof(byte?), TypeKind.ByteNullable);
            map.Add(typeof(int), TypeKind.Int32);
            map.Add(typeof(int?), TypeKind.Int32Nullable);
            map.Add(typeof(long), TypeKind.Int64);
            map.Add(typeof(long?), TypeKind.Int64Nullable);
            map.Add(typeof(short), TypeKind.Int16);
            map.Add(typeof(short?), TypeKind.Int16Nullable);
            map.Add(typeof(string), TypeKind.String);
            map.Add(typeof(Guid), TypeKind.Guid);
            map.Add(typeof(Guid?), TypeKind.GuidNullable);
            map.Add(typeof(float), TypeKind.Float);
            map.Add(typeof(float?), TypeKind.FloatNullable);
            map.Add(typeof(double), TypeKind.Double);
            map.Add(typeof(double?), TypeKind.DoubleNullable);
            map.Add(typeof(decimal), TypeKind.Decimal);
            map.Add(typeof(decimal?), TypeKind.DecimalNullable);
            map.Add(typeof(bool), TypeKind.Boolean);
            map.Add(typeof(bool?), TypeKind.BooleanNullable);
            map.Add(typeof(byte[]), TypeKind.Binary);
            map.Add(typeof(DateTime), TypeKind.DateTime);
            map.Add(typeof(DateTime?), TypeKind.DateTimeNullable);
            map.Add(typeof(DateTimeOffset), TypeKind.DateTimeOffset);
            map.Add(typeof(DateTimeOffset?), TypeKind.DateTimeOffsetNullable);

            _Map = map.AsReadOnly();
        }

        public static bool IsSizeableType(this TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.String:
                case TypeKind.Binary:
                    return true;
                case TypeKind.Byte:
                case TypeKind.DateTimeOffset:
                case TypeKind.DateTime:
                case TypeKind.Float:
                case TypeKind.Double:
                case TypeKind.Decimal:
                case TypeKind.Int32:
                case TypeKind.Int16:
                case TypeKind.Int64:
                case TypeKind.Guid:
                case TypeKind.Boolean:
                case TypeKind.ByteNullable:
                case TypeKind.DateTimeOffsetNullable:
                case TypeKind.DateTimeNullable:
                case TypeKind.FloatNullable:
                case TypeKind.DoubleNullable:
                case TypeKind.DecimalNullable:
                case TypeKind.Int32Nullable:
                case TypeKind.Int16Nullable:
                case TypeKind.Int64Nullable:
                case TypeKind.GuidNullable:
                case TypeKind.BooleanNullable:
                    return false;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TypeKindIsNotSupported, typeKind));
            }
        }

        public static bool IsNotNullType(this TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Byte:
                case TypeKind.DateTimeOffset:
                case TypeKind.DateTime:
                case TypeKind.Float:
                case TypeKind.Double:
                case TypeKind.Decimal:
                case TypeKind.Int32:
                case TypeKind.Int16:
                case TypeKind.Int64:
                case TypeKind.Guid:
                case TypeKind.Boolean:
                    return true;
                case TypeKind.String:
                case TypeKind.Binary:
                case TypeKind.ByteNullable:
                case TypeKind.DateTimeOffsetNullable:
                case TypeKind.DateTimeNullable:
                case TypeKind.FloatNullable:
                case TypeKind.DoubleNullable:
                case TypeKind.DecimalNullable:
                case TypeKind.Int32Nullable:
                case TypeKind.Int16Nullable:
                case TypeKind.Int64Nullable:
                case TypeKind.GuidNullable:
                case TypeKind.BooleanNullable:
                    return false;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TypeKindIsNotSupported, typeKind));
            }
        }

        public static MethodInfo GetMethodByTypeKind(this TypeKind typekind)
        {
            Type readerWrapperType = typeof(IReaderWrapper);

            switch (typekind)
            {
                case TypeKind.Binary:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetBytes));
                case TypeKind.Byte:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetByte));
                case TypeKind.ByteNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetByteNullable));
                case TypeKind.DateTimeOffset:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTimeOffset));
                case TypeKind.DateTimeOffsetNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTimeOffsetNullable));
                case TypeKind.DateTime:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTime));
                case TypeKind.DateTimeNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDateTimeNullable));
                case TypeKind.Float:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetFloat));
                case TypeKind.FloatNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetFloatNullable));
                case TypeKind.Double:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDouble));
                case TypeKind.DoubleNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDoubleNullable));
                case TypeKind.Decimal:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDecimal));
                case TypeKind.DecimalNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetDecimalNullable));
                case TypeKind.Int32:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetInt));
                case TypeKind.Int32Nullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetIntNullable));
                case TypeKind.Int16:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetShort));
                case TypeKind.Int16Nullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetShortNullable));
                case TypeKind.Int64:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetLong));
                case TypeKind.Int64Nullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetLongNullable));
                case TypeKind.Guid:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetGuid));
                case TypeKind.GuidNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetGuidNullable));
                case TypeKind.Boolean:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetBoolean));
                case TypeKind.BooleanNullable:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetBooleanNullable));
                case TypeKind.String:
                    return readerWrapperType.GetMethod(nameof(IReaderWrapper.GetString));
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TypeKindIsNotSupported, typekind));
            }
        }

        public static bool IsNullable(this TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Binary:
                case TypeKind.BooleanNullable:
                case TypeKind.ByteNullable:
                case TypeKind.DateTimeNullable:
                case TypeKind.DateTimeOffsetNullable:
                case TypeKind.DecimalNullable:
                case TypeKind.DoubleNullable:
                case TypeKind.FloatNullable:
                case TypeKind.GuidNullable:
                case TypeKind.Int16Nullable:
                case TypeKind.Int32Nullable:
                case TypeKind.Int64Nullable:
                case TypeKind.Null:
                case TypeKind.String:
                    return true;
                default:
                    return false;
            }
        }

        public static bool TryGetTypeKind(this Type type, out TypeKind typeKind)
        {   
            if (_Map.ContainsKey(type))
            {
                typeKind = _Map[type];
                return true;
            }

            typeKind = TypeKind.Null;
            return false;
        }

        public static TypeKind GetTypeKind(this Type type)
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
