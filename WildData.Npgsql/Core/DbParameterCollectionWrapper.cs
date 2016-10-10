using ModernRoute.WildData.Core;
using ModernRoute.WildData.Npgsql.Extensions;
using Npgsql;
using NpgsqlTypes;
using System;

namespace ModernRoute.WildData.Npgsql.Core
{
    public class DbParameterCollectionWrapper : BaseDbParameterCollectionWrapper
    {
        private NpgsqlParameterCollection _ParameterCollection;

        public DbParameterCollectionWrapper(NpgsqlParameterCollection parameterCollection)
        {
            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }

            _ParameterCollection = parameterCollection;
        }

        public override void AddParam(string name, DateTime? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Timestamp).Value = value.DbNullable();
        }

        public override void AddParam(string name, double? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Double).Value = value.DbNullable();
        }

        public override void AddParam(string name, int? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Integer).Value = value.DbNullable();
        }

        public override void AddParam(string name, long? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Bigint).Value = value.DbNullable();
        }

        public override void AddParam(string name, short? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Smallint).Value = value.DbNullable();
        }

        public override void AddParam(string name, decimal? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Numeric).Value = value.DbNullable();
        }

        public override void AddParam(string name, bool? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Boolean).Value = value.DbNullable();
        }

        public override void AddParam(string name, float? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Real).Value = value.DbNullable();
        }

        public override void AddParam(string name, DateTimeOffset? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.TimestampTZ).Value = value.DbNullable();
        }

        public override void AddParam(string name, byte? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Smallint).Value = value.DbNullable();
        }

        public override void AddParam(string name, byte[] value, int size)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Bytea, size).Value = value.DbNullable();
        }

        public override void AddParam(string name, Guid? value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Uuid).Value = value.DbNullable();
        }

        public override void AddParam(string name, string value, int size)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Varchar, size).Value = value.DbNullable();
        }

        public override void AddParamNotNull(string name, DateTime value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Timestamp).Value = value;
        }

        public override void AddParamNotNull(string name, double value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Double).Value = value;
        }

        public override void AddParamNotNull(string name, int value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Integer).Value = value;
        }

        public override void AddParamNotNull(string name, long value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Bigint).Value = value;
        }

        public override void AddParamNotNull(string name, Guid value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Uuid).Value = value;
        }

        public override void AddParamNotNull(string name, decimal value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Numeric).Value = value;
        }

        public override void AddParamNotNull(string name, bool value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Boolean).Value = value;
        }

        public override void AddParamNotNull(string name, short value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Smallint).Value = value;
        }

        public override void AddParamNotNull(string name, float value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Real).Value = value;
        }

        public override void AddParamNotNull(string name, DateTimeOffset value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.TimestampTZ).Value = value;
        }

        public override void AddParamNotNull(string name, byte value)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Smallint).Value = value;
        }

        public override void AddParamNotNull(string name, byte[] value, int size)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Bytea, size).Value = value;
        }

        public override void AddParamNotNull(string name, string value, int size)
        {
            _ParameterCollection.Add(name, NpgsqlDbType.Varchar, size).Value = value;
        }
    }
}
