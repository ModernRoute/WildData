using System;

namespace ModernRoute.WildData.Core
{
    public interface IReaderWrapper
    {
        byte GetByte(int columnIndex);
        byte? GetByteNullable(int columnIndex);
        DateTimeOffset GetDateTimeOffset(int columnIndex);
        DateTimeOffset? GetDateTimeOffsetNullable(int columnIndex);
        DateTime GetDateTime(int columnIndex);
        DateTime? GetDateTimeNullable(int columnIndex);
        float GetFloat(int columnIndex);     
        float? GetFloatNullable(int columnIndex);   
        double GetDouble(int columnIndex);
        double? GetDoubleNullable(int columnIndex);
        short GetShort(int columnIndex);
        short? GetShortNullable(int columnIndex);
        int GetInt(int columnIndex);
        int? GetIntNullable(int columnIndex);
        long GetLong(int columnIndex);
        long? GetLongNullable(int columnIndex);
        string GetString(int columnIndex);
        Guid GetGuid(int columnIndex);
        Guid? GetGuidNullable(int columnIndex);
        byte[] GetBytes(int columnIndex);
        bool GetBoolean(int columnIndex);
        bool? GetBooleanNullable(int columnIndex);
        decimal GetDecimal(int columnIndex);
        decimal? GetDecimalNullable(int columnIndex);
    }
}
