using ModernRoute.WildData.Core;
using Npgsql;
using System;
using System.IO;

namespace ModernRoute.WildData.Npgsql.Core
{
    public class ReaderWrapper : IReaderWrapper
    {
        private NpgsqlDataReader _Reader;

        public ReaderWrapper(NpgsqlDataReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            _Reader = reader;
        }

        public byte GetByte(int columnIndex)
        {
            throw new NotSupportedException();
        }

        public byte? GetByteNullable(int columnIndex)
        {
            throw new NotSupportedException();
        }

        public DateTimeOffset GetDateTimeOffset(int columnIndex)
        {
            return _Reader.GetDateTime(columnIndex);
        }

        public DateTimeOffset? GetDateTimeOffsetNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetDateTime(columnIndex);
        }

        public DateTime GetDateTime(int columnIndex)
        {
            return _Reader.GetDateTime(columnIndex);
        }

        public DateTime? GetDateTimeNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetDateTime(columnIndex);
        }

        public float GetFloat(int columnIndex)
        {
            return _Reader.GetFloat(columnIndex);
        }

        public float? GetFloatNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetFloat(columnIndex);
        }

        public double GetDouble(int columnIndex)
        {
            return _Reader.GetDouble(columnIndex);
        }

        public double? GetDoubleNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetDouble(columnIndex);
        }

        public short GetShort(int columnIndex)
        {
            return _Reader.GetInt16(columnIndex);
        }

        public short? GetShortNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetInt16(columnIndex);
        }

        public int GetInt(int columnIndex)
        {
            return _Reader.GetInt32(columnIndex);
        }

        public int? GetIntNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetInt32(columnIndex);
        }

        public long GetLong(int columnIndex)
        {
            return _Reader.GetInt64(columnIndex);
        }

        public long? GetLongNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetInt64(columnIndex);
        }

        public string GetString(int columnIndex)
        {
            return _Reader.GetString(columnIndex);
        }

        public string GetStringNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetString(columnIndex);
        }

        public Guid GetGuid(int columnIndex)
        {
            return _Reader.GetGuid(columnIndex);
        }

        public Guid? GetGuidNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetGuid(columnIndex);
        }

        public byte[] GetBytes(int columnIndex)
        {
            MemoryStream stream = null;

            using (stream = new MemoryStream())
            {
                using (Stream streamToRead = _Reader.GetStream(columnIndex))
                {
                    streamToRead.CopyTo(stream);
                }
            }

            return stream.ToArray();
        }

        public byte[] GetBytesNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return GetBytes(columnIndex);
        }

        public bool GetBoolean(int columnIndex)
        {
            return _Reader.GetBoolean(columnIndex);
        }

        public bool? GetBooleanNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetBoolean(columnIndex);
        }

        public decimal GetDecimal(int columnIndex)
        {
            return _Reader.GetDecimal(columnIndex);
        }

        public decimal? GetDecimalNullable(int columnIndex)
        {
            if (_Reader.IsDBNull(columnIndex))
            {
                return null;
            }

            return _Reader.GetDecimal(columnIndex);
        }
    }
}
