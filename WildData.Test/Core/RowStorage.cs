using ModernRoute.WildData.Core;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ModernRoute.WildData.Test.Core
{
    class RowStorage : IDbParameterCollectionWrapper, IReaderWrapper
    {
        private Regex regex = new Regex("@__p_([0-9]+)", RegexOptions.Compiled);
        private IDictionary<int, object> _Values;

        public RowStorage()
        {
            _Values = new Dictionary<int, object>();
        }

        private int GetParamIndex(string name)
        {
            Match match = regex.Match(name);

            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return -1;
        }

        public void AddParam(string name, DateTimeOffset? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, float? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, DateTime? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, short? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, long? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, decimal? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, bool? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, int? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, double? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, byte? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, byte[] value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, Guid? value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParam(string name, string value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, bool value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, float value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, Guid value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, DateTime value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, long value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, decimal value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, int value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, DateTimeOffset value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, short value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, double value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, byte value)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, byte[] value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        public void AddParamNotNull(string name, string value, int size)
        {
            _Values[GetParamIndex(name)] = value;
        }

        private T GetValue<T>(int columnIndex)
        {
            object value = _Values[columnIndex];

            if (value is T)
            {
                return (T)value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public bool GetBoolean(int columnIndex)
        {
            return GetValue<bool>(columnIndex);
        }

        public bool? GetBooleanNullable(int columnIndex)
        {
            return GetValue<bool?>(columnIndex);
        }

        public byte GetByte(int columnIndex)
        {
            return GetValue<byte>(columnIndex);
        }

        public byte? GetByteNullable(int columnIndex)
        {
            return GetValue<byte?>(columnIndex);
        }

        public byte[] GetBytes(int columnIndex)
        {
            return GetValue<byte[]>(columnIndex);
        }

        public byte[] GetBytesNullable(int columnIndex)
        {
            return GetValue<byte[]>(columnIndex);
        }

        public DateTime GetDateTime(int columnIndex)
        {
            return GetValue<DateTime>(columnIndex);
        }

        public DateTime? GetDateTimeNullable(int columnIndex)
        {
            return GetValue<DateTime?>(columnIndex);
        }

        public DateTimeOffset GetDateTimeOffset(int columnIndex)
        {
            return GetValue<DateTimeOffset>(columnIndex);
        }

        public DateTimeOffset? GetDateTimeOffsetNullable(int columnIndex)
        {
            return GetValue<DateTimeOffset?>(columnIndex);
        }

        public decimal GetDecimal(int columnIndex)
        {
            return GetValue<decimal>(columnIndex);
        }

        public decimal? GetDecimalNullable(int columnIndex)
        {
            return GetValue<decimal?>(columnIndex);
        }

        public double GetDouble(int columnIndex)
        {
            return GetValue<double>(columnIndex);
        }

        public double? GetDoubleNullable(int columnIndex)
        {
            return GetValue<double?>(columnIndex);
        }

        public float GetFloat(int columnIndex)
        {
            return GetValue<float>(columnIndex);
        }

        public float? GetFloatNullable(int columnIndex)
        {
            return GetValue<float?>(columnIndex);
        }

        public Guid GetGuid(int columnIndex)
        {
            return GetValue<Guid>(columnIndex);
        }

        public Guid? GetGuidNullable(int columnIndex)
        {
            return GetValue<Guid?>(columnIndex);
        }

        public int GetInt(int columnIndex)
        {
            return GetValue<int>(columnIndex);
        }

        public int? GetIntNullable(int columnIndex)
        {
            return GetValue<int?>(columnIndex);
        }

        public long GetLong(int columnIndex)
        {
            return GetValue<long>(columnIndex);
        }

        public long? GetLongNullable(int columnIndex)
        {
            return GetValue<long?>(columnIndex);
        }

        public short GetShort(int columnIndex)
        {
            return GetValue<short>(columnIndex);
        }

        public short? GetShortNullable(int columnIndex)
        {
            return GetValue<short?>(columnIndex);
        }

        public string GetString(int columnIndex)
        {
            return GetValue<string>(columnIndex);
        }

        public string GetStringNullable(int columnIndex)
        {
            return GetValue<string>(columnIndex);
        }
    }
}
