using System;

namespace ModernRoute.WildData.Npgsql.Extensions
{
    public static class GenericExtensions
    {
        public static object DbNullable<T>(this T value) where T : class
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }

        public static object DbNullable<T>(this T? value) where T : struct
        {
            if (value.HasValue)
            {
                return value.Value;                
            }

            return DBNull.Value;
        }
    }
}
