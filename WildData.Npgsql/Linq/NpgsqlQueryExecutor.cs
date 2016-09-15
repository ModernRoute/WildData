using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Npgsql.Core;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModernRoute.WildData.Npgsql.Linq
{
    class NpgsqlQueryExecutor : QueryExecutor
    {
        private const string _ExecuteInternalMethodName = "ExecuteInternal";
        private const string _SingleMethodName = "Single";

        private BaseSession _Session;
        private string _SourceQuery;
        private NpgsqlParameterCollection _Parameters;
        private int _ParameterPrefixLength;

        public NpgsqlQueryExecutor(BaseSession session, string sourceQuery, 
            IReadOnlyDictionary<string, ColumnInfo> memberColumnMap, Delegate reader, NpgsqlParameterCollection parameters = null)
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

        public override object Execute(SourceBase sourceBase)
        {
            MethodInfo method = GetType()
                .GetMethod(
                    _ExecuteInternalMethodName,
                    BindingFlags.NonPublic | BindingFlags.Instance
                 )
                .MakeGenericMethod(sourceBase.Projector.Method.ReturnType);

            object value = method.Invoke(this, new object[] { sourceBase });

            if (sourceBase.SelectType != SelectType.Projection)
            {
                return value;
            }

            Type enumerableType = typeof(IEnumerable<>);

            MethodInfo singleMethod =
                typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == _SingleMethodName && m.GetParameters().Length == 1).Single()
                .MakeGenericMethod(sourceBase.Projector.Method.ReturnType);

            return singleMethod.Invoke(null, new object[] { value });
        }

        private IEnumerable<T> ExecuteInternal<T>(SourceBase sourceBase)
        {
            IList<T> list = new List<T>();

            NpgsqlQueryBuilder builder = new NpgsqlQueryBuilder();

            using (NpgsqlCommand npgsqlCommand = _Session.CreateCommand())
            {
                builder.Build(npgsqlCommand, sourceBase,_SourceQuery, _ParameterPrefixLength);

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
                    NpgsqlReaderWrapper wrapper = new NpgsqlReaderWrapper(reader);

                    while (reader.Read())
                    {
                        list.Add((T)sourceBase.Projector.DynamicInvoke(wrapper));
                    }
                }
            }

            return list;
        }
    }
}
