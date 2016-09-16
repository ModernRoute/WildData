using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Npgsql.Extensions;
using ModernRoute.WildData.Npgsql.Helpers;
using ModernRoute.WildData.Npgsql.Linq;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModernRoute.WildData.Npgsql.Core
{
    abstract class BaseReadOnlyRepository<T> : IReadOnlyRepository<T> where T : IReadOnlyModel, new()
    {
        protected ReadOnlyRepositoryHelper<T> ReadOnlyRepositoryHelper
        {
            get;
            private set;
        }

        public BaseReadOnlyRepository(BaseSession session) : this(session, new ReadOnlyRepositoryHelper<T>())
        {

        }

        protected BaseReadOnlyRepository(BaseSession session, ReadOnlyRepositoryHelper<T> helper)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            Session = session;
            ReadOnlyRepositoryHelper = helper;
        }

        protected BaseSession Session
        {
            get;
            private set;
        }

        protected IEnumerable<T> ExecuteReader(NpgsqlCommand command)
        {
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                NpgsqlReaderWrapper wrapper = new NpgsqlReaderWrapper(reader);

                while (reader.Read())
                {
                    yield return ReadOnlyRepositoryHelper.ReadSingleObject(wrapper);
                }
            }
        }

        protected IQueryable<T> GetQuery(string query, NpgsqlParameterCollection parameters = null)
        {
            return new Query<T>(
                new QueryProvider(
                    new NpgsqlQueryExecutor(
                        Session,
                        query,
                        ReadOnlyRepositoryHelper.MemberColumnMap,
                        ReadOnlyRepositoryHelper.ReadSingleObject,
                        parameters
                    )
                )
            );
        }

        public IQueryable<T> Fetch()
        {
            StringBuilder query = new StringBuilder();

            query.Append(NpgsqlSyntax.SelectToken);
            query.Append(NpgsqlSyntax.Space);

            AppendColumnList(query);

            query.Append(NpgsqlSyntax.Space);
            query.Append(NpgsqlSyntax.FromToken);
            query.Append(NpgsqlSyntax.Space);

            AppendStorageName(query);

            return GetQuery(query.ToString());
        }

        protected void AppendStorageName(StringBuilder query)
        {
            if (ReadOnlyRepositoryHelper.StorageSchema != null)
            {
                query.Append(NpgsqlSyntax.Quote);
                query.Append(NpgsqlHelper.EscapeString(ReadOnlyRepositoryHelper.StorageSchema));
                query.Append(NpgsqlSyntax.Quote);
                query.Append(NpgsqlSyntax.Dot);
            }

            query.Append(NpgsqlSyntax.Quote);
            query.Append(NpgsqlHelper.EscapeString(ReadOnlyRepositoryHelper.StorageName));
            query.Append(NpgsqlSyntax.Quote);
        }

        private void AppendColumn(StringBuilder query, ColumnInfo columnInfo)
        {
            query.Append(NpgsqlSyntax.Quote);
            query.Append(NpgsqlHelper.EscapeString(columnInfo.ColumnName));
            query.Append(NpgsqlSyntax.Quote);
        }

        protected void AppendColumn(StringBuilder query, string memberName)
        {
            AppendColumn(query, ReadOnlyRepositoryHelper.MemberColumnMap[memberName]);
        }

        protected void AppendColumnList(StringBuilder query)
        {
            bool first = true;

            foreach (KeyValuePair<string, ColumnInfo> columnInfo in ReadOnlyRepositoryHelper.MemberColumnMap)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(NpgsqlSyntax.Comma);
                }

                AppendColumn(query, columnInfo.Value);
            }
        }
    }

    abstract class BaseReadOnlyRepository<T,TKey> : BaseReadOnlyRepository<T>, IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>, new()
    {
        private const string _IdParameterName = "@id";

        public BaseReadOnlyRepository(BaseSession session)
            : base(session)
        {

        }

        protected BaseReadOnlyRepository(BaseSession session, ReadOnlyRepositoryHelper<T> helper)
            : base(session, helper)
        {

        }

        public T Fetch(TKey id)
        {
            using (NpgsqlCommand command = Session.CreateCommand())
            {
                StringBuilder query = new StringBuilder();

                query.Append(NpgsqlSyntax.SelectToken);
                query.Append(NpgsqlSyntax.Space);

                AppendColumnList(query);

                query.Append(NpgsqlSyntax.Space);
                query.Append(NpgsqlSyntax.FromToken);
                query.Append(NpgsqlSyntax.Space);

                AppendStorageName(query);

                query.Append(NpgsqlSyntax.Space);
                query.Append(NpgsqlSyntax.WhereToken);
                query.Append(NpgsqlSyntax.Space);

                AppendColumn(query, nameof(IReadOnlyModel<TKey>.Id));
                query.Append(NpgsqlSyntax.Space);
                query.Append(NpgsqlSyntax.EqualSign);
                query.Append(NpgsqlSyntax.Space);

                AddIdParameter(command, _IdParameterName, id);
                query.Append(_IdParameterName);

                command.CommandText = query.ToString();

                command.Prepare();

                return ExecuteReader(command).SingleOrDefault();
            }
        }

        protected void AddIdParameter(NpgsqlCommand command, string paramName, TKey paramValue)
        {
            ColumnInfo columnInfo = ReadOnlyRepositoryHelper.MemberColumnMap[nameof(IReadOnlyModel<TKey>.Id)];
            NpgsqlDbType type = columnInfo.ReturnType.GetNpgsqlType();

            if (columnInfo.ReturnType.IsSizeableType())
            {
                command.Parameters.Add(paramName, type, columnInfo.ColumnSize).Value = paramValue;
            }
            else
            {
                command.Parameters.Add(paramName, type).Value = paramValue;
            }
        }
    }
}
