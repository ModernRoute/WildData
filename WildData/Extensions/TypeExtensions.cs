using System;
using System.Collections.Generic;

namespace ModernRoute.WildData.Extensions
{
    public static class TypeExtensions
    {
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
