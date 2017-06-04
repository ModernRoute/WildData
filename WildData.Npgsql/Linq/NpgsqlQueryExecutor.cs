using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Npgsql.Core;
using Npgsql;
using System;
using System.Collections.Generic;

namespace ModernRoute.WildData.Npgsql.Linq
{
    class NpgsqlQueryExecutor : QueryExecutor
    {
        private BaseSession _Session;
        private string _SourceQuery;
        private NpgsqlParameterCollection _Parameters;
        private int _ParameterPrefixLength;

        public NpgsqlQueryExecutor(BaseSession session, string sourceQuery, 
            IReadOnlyDictionary<string, ColumnInfo> memberColumnMap, Delegate reader, 
            NpgsqlParameterCollection parameters = null)
            : base(memberColumnMap, reader)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (sourceQuery == null)
            {
                throw new ArgumentNullException(nameof(sourceQuery));
            }

            _Parameters = parameters;
            _Session = session;
            _SourceQuery = sourceQuery;

            _ParameterPrefixLength = 0;
            
            if (_Parameters != null)
            {
                foreach (NpgsqlParameter parameter in _Parameters)
                {
                    if (parameter.ParameterName.Length > _ParameterPrefixLength)
                    {
                        _ParameterPrefixLength = parameter.ParameterName.Length;
                    }
                }
            }
        }

        protected override IEnumerable<T> ExecuteCollection<T>(SourceBase sourceBase)
        {
            NpgsqlQueryBuilder builder = new NpgsqlQueryBuilder();

            using (NpgsqlCommand npgsqlCommand = _Session.CreateCommand())
            {
                builder.Build(npgsqlCommand, sourceBase, _SourceQuery, _ParameterPrefixLength);

                if (_Parameters != null)
                {
                    foreach (NpgsqlParameter parameter in _Parameters)
                    {
                        npgsqlCommand.Parameters.Add(parameter);
                    }
                }

                npgsqlCommand.Prepare();

                using (NpgsqlDataReader reader = npgsqlCommand.ExecuteReader())
                {
                    ReaderWrapper wrapper = new ReaderWrapper(reader);

                    while (reader.Read())
                    {
                        yield return (T)sourceBase.Projector.DynamicInvoke(wrapper);
                    }
                }
            }
        }
    }
}
