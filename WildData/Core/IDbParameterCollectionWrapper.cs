using System;

namespace ModernRoute.WildData.Core
{
    public interface IDbParameterCollectionWrapper
    {
        void AddParam(string name, byte value);
        void AddParam(string name, byte? value);
        void AddParam(string name, DateTimeOffset value);
        void AddParam(string name, DateTimeOffset? value);
        void AddParam(string name, DateTime value);
        void AddParam(string name, DateTime? value);
        void AddParam(string name, float value);
        void AddParam(string name, float? value);
        void AddParam(string name, double value);
        void AddParam(string name, double? value);
        void AddParam(string name, short value);
        void AddParam(string name, short? value);
        void AddParam(string name, int value);
        void AddParam(string name, int? value);
        void AddParam(string name, long value);
        void AddParam(string name, long? value);
        void AddParam(string name, string value);
        void AddParam(string name, Guid value);
        void AddParam(string name, Guid? value);
        void AddParam(string name, byte[] value);
        void AddParam(string name, bool value);
        void AddParam(string name, bool? value);
        void AddParam(string name, decimal value);
        void AddParam(string name, decimal? value);
    }
}
