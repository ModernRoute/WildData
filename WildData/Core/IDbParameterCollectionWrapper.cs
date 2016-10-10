using System;

namespace ModernRoute.WildData.Core
{
    public interface IDbParameterCollectionWrapper
    {
        void AddParamNotNull(string name, byte value);
        void AddParam(string name, byte? value);
        void AddParamNotNull(string name, DateTimeOffset value);
        void AddParam(string name, DateTimeOffset? value);
        void AddParamNotNull(string name, DateTime value);
        void AddParam(string name, DateTime? value);
        void AddParamNotNull(string name, float value);
        void AddParam(string name, float? value);
        void AddParamNotNull(string name, double value);
        void AddParam(string name, double? value);
        void AddParamNotNull(string name, short value);
        void AddParam(string name, short? value);
        void AddParamNotNull(string name, int value);
        void AddParam(string name, int? value);
        void AddParamNotNull(string name, long value);
        void AddParam(string name, long? value);
        void AddParamNotNull(string name, string value, int size);
        void AddParam(string name, string value, int size);
        void AddParamNotNull(string name, Guid value);
        void AddParam(string name, Guid? value);
        void AddParamNotNull(string name, byte[] value, int size);
        void AddParam(string name, byte[] value, int size);
        void AddParamNotNull(string name, bool value);
        void AddParam(string name, bool? value);
        void AddParamNotNull(string name, decimal value);
        void AddParam(string name, decimal? value);
        void AddParamNotNullBase(string nameBase, byte value);
        void AddParamBase(string nameBase, byte? value);
        void AddParamNotNullBase(string nameBase, DateTimeOffset value);
        void AddParamBase(string nameBase, DateTimeOffset? value);
        void AddParamNotNullBase(string nameBase, DateTime value);
        void AddParamBase(string nameBase, DateTime? value);
        void AddParamNotNullBase(string nameBase, float value);
        void AddParamBase(string nameBase, float? value);
        void AddParamNotNullBase(string nameBase, double value);
        void AddParamBase(string nameBase, double? value);
        void AddParamNotNullBase(string nameBase, short value);
        void AddParamBase(string nameBase, short? value);
        void AddParamNotNullBase(string nameBase, int value);
        void AddParamBase(string nameBase, int? value);
        void AddParamNotNullBase(string nameBase, long value);
        void AddParamBase(string nameBase, long? value);
        void AddParamNotNullBase(string nameBase, string value, int size);
        void AddParamBase(string nameBase, string value, int size);
        void AddParamNotNullBase(string nameBase, Guid value);
        void AddParamBase(string nameBase, Guid? value);
        void AddParamNotNullBase(string nameBase, byte[] value, int size);
        void AddParamBase(string nameBase, byte[] value, int size);
        void AddParamNotNullBase(string nameBase, bool value);
        void AddParamBase(string nameBase, bool? value);
        void AddParamNotNullBase(string nameBase, decimal value);
        void AddParamBase(string nameBase, decimal? value);
    }
}
