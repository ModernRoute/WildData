using ModernRoute.WildData.Core;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Npgsql.Helpers;
using ModernRoute.WildData.Npgsql.Linq;
using Npgsql;
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
                ReaderWrapper wrapper = new ReaderWrapper(reader);

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

            query.Append(SyntaxHelper.SelectToken);
            query.Append(SyntaxHelper.Space);

            AppendColumnList(query);

            query.Append(SyntaxHelper.Space);
            query.Append(SyntaxHelper.FromToken);
            query.Append(SyntaxHelper.Space);

            AppendStorageName(query);

            return GetQuery(query.ToString());
        }

        protected void AppendStorageName(StringBuilder query)
        {
            if (ReadOnlyRepositoryHelper.StorageSchema != null)
            {
                query.Append(SyntaxHelper.Quote);
                query.Append(EscapeHelper.EscapeString(ReadOnlyRepositoryHelper.StorageSchema));
                query.Append(SyntaxHelper.Quote);
                query.Append(SyntaxHelper.Dot);
            }

            query.Append(SyntaxHelper.Quote);
            query.Append(EscapeHelper.EscapeString(ReadOnlyRepositoryHelper.StorageName));
            query.Append(SyntaxHelper.Quote);
        }

        protected void AppendColumn(StringBuilder query, ColumnInfo columnInfo)
        {
            query.Append(SyntaxHelper.Quote);
            query.Append(EscapeHelper.EscapeString(columnInfo.ColumnName));
            query.Append(SyntaxHelper.Quote);
        }

        protected void AppendColumn(StringBuilder query, string memberName)
        {
            AppendColumn(query, ReadOnlyRepositoryHelper.MemberColumnMap[memberName]);
        }

        protected void AppendColumnList(StringBuilder query, IEnumerable<KeyValuePair<string, ColumnInfo>> columns)
        {
            bool first = true;

            foreach (KeyValuePair<string, ColumnInfo> columnInfo in columns)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    query.Append(SyntaxHelper.Comma);
                    query.Append(SyntaxHelper.Space);
                }

                AppendColumn(query, columnInfo.Value);
            }
        }

        protected void AppendColumnList(StringBuilder query)
        {
            AppendColumnList(query, ReadOnlyRepositoryHelper.MemberColumnMap);
        }
    }

    abstract class BaseReadOnlyRepository<T,TKey> : BaseReadOnlyRepository<T>, IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>, new()
    {
        protected ReadOnlyRepositoryHelper<T,TKey> ReadOnlyRepositoryHelperWithKey
        {
            get;
            private set;
        }

        public BaseReadOnlyRepository(BaseSession session)
            : this(session, new ReadOnlyRepositoryHelper<T, TKey>())
        {

        }

        protected BaseReadOnlyRepository(BaseSession session, ReadOnlyRepositoryHelper<T,TKey> helper)
            : base(session, helper)
        {
            ReadOnlyRepositoryHelperWithKey = helper;
        }

        protected void AppendIdColumn(StringBuilder query)
        {
            AppendColumn(query, nameof(IReadWriteModel<TKey>.Id));
        }

        public T Fetch(TKey id)
        {
            using (NpgsqlCommand command = Session.CreateCommand())
            {
                StringBuilder query = new StringBuilder();

                query.Append(SyntaxHelper.SelectToken);
                query.Append(SyntaxHelper.Space);

                AppendColumnList(query);

                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.FromToken);
                query.Append(SyntaxHelper.Space);

                AppendStorageName(query);

                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.WhereToken);
                query.Append(SyntaxHelper.Space);

                AppendColumn(query, nameof(IReadOnlyModel<TKey>.Id));
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.EqualSign);
                query.Append(SyntaxHelper.Space);

                string idParamName = ReadOnlyRepositoryHelperWithKey.GetIdParamName();

                ReadOnlyRepositoryHelperWithKey.AddIdParameter(new DbParameterCollectionWrapper(command.Parameters), id);
                query.Append(idParamName);

                command.CommandText = query.ToString();

                command.Prepare();

                return ExecuteReader(command).SingleOrDefault();
            }
        }
    }
}
