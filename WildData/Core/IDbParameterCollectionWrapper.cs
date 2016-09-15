using System;

namespace ModernRoute.WildData.Core
{
    public interface IDbParameterCollectionWrapper
    {
        void AddParamNotNull(string nameBase, byte value);
        void AddParam(string nameBase, byte? value);
        void AddParamNotNull(string nameBase, DateTimeOffset value);
        void AddParam(string nameBase, DateTimeOffset? value);
        void AddParamNotNull(string nameBase, DateTime value);
        void AddParam(string nameBase, DateTime? value);
        void AddParamNotNull(string nameBase, float value);
        void AddParam(string nameBase, float? value);
        void AddParamNotNull(string nameBase, double value);
        void AddParam(string nameBase, double? value);
        void AddParamNotNull(string nameBase, short value);
        void AddParam(string nameBase, short? value);
        void AddParamNotNull(string nameBase, int value);
        void AddParam(string nameBase, int? value);
        void AddParamNotNull(string nameBase, long value);
        void AddParam(string nameBase, long? value);
        void AddParamNotNull(string nameBase, string value, int size);
        void AddParam(string nameBase, string value, int size);
        void AddParamNotNull(string nameBase, Guid value);
        void AddParam(string nameBase, Guid? value, int size);
        void AddParamNotNull(string nameBase, byte[] value, int size);
        void AddParam(string nameBase, byte[] value, int size);
        void AddParamNotNull(string nameBase, bool value);
        void AddParam(string nameBase, bool? value);
        void AddParamNotNull(string nameBase, decimal value);
        void AddParam(string nameBase, decimal? value);
    }
}
