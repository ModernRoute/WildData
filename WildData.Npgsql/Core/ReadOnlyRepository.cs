﻿using ModernRoute.WildData.Core;
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
    public class ReadOnlyRepository<T> : BaseRepository, IReadOnlyRepository<T> where T : IReadOnlyModel, new()
    {
        protected ReadOnlyRepositoryHelper<T> ReadOnlyRepositoryHelper
        {
            get;
            private set;
        }

        public ReadOnlyRepository(BaseSession session) : this(session, new ReadOnlyRepositoryHelper<T>(new TypeKindInfo()))
        {

        }

        protected ReadOnlyRepository(BaseSession session, ReadOnlyRepositoryHelper<T> helper) : base(session)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            ReadOnlyRepositoryHelper = helper;
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
            return new Queryable<T>(
                new NpgsqlQueryExecutor(
                    Session,
                    query,
                    ReadOnlyRepositoryHelper.MemberColumnMap,
                    ReadOnlyRepositoryHelper.ReadSingleObject,
                    parameters
                )
            );
        }

        public IQueryable<T> Fetch()
        {
            StringBuilder query = new StringBuilder();

            AppendSelectFrom(query);

            return GetQuery(query.ToString());
        }

        protected void AppendSelectFrom(StringBuilder query)
        {
            query.Append(SyntaxHelper.SelectToken);
            query.Append(SyntaxHelper.SpaceToken);

            AppendColumnList(query);

            query.Append(SyntaxHelper.SpaceToken);
            query.Append(SyntaxHelper.FromToken);
            query.Append(SyntaxHelper.SpaceToken);

            AppendStorageName(query);
        }

        protected void AppendStorageName(StringBuilder query)
        {
            if (ReadOnlyRepositoryHelper.StorageSchema != null)
            {
                query.Append(SyntaxHelper.ColumnNameDelimiter);
                query.Append(EscapeHelper.EscapeString(ReadOnlyRepositoryHelper.StorageSchema));
                query.Append(SyntaxHelper.ColumnNameDelimiter);
                query.Append(SyntaxHelper.EntitySeparatorToken);
            }

            query.Append(SyntaxHelper.ColumnNameDelimiter);
            query.Append(EscapeHelper.EscapeString(ReadOnlyRepositoryHelper.StorageName));
            query.Append(SyntaxHelper.ColumnNameDelimiter);
        }

        protected void AppendColumn(StringBuilder query, ColumnInfo columnInfo)
        {
            query.Append(SyntaxHelper.ColumnNameDelimiter);
            query.Append(EscapeHelper.EscapeString(columnInfo.ColumnName));
            query.Append(SyntaxHelper.ColumnNameDelimiter);
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
                    query.Append(SyntaxHelper.CommaToken);
                }

                AppendColumn(query, columnInfo.Value);
            }
        }

        protected void AppendColumnList(StringBuilder query)
        {
            AppendColumnList(query, ReadOnlyRepositoryHelper.MemberColumnMap);
        }
    }

    public class ReadOnlyRepository<T,TKey> : ReadOnlyRepository<T>, IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>, new()
    {
        protected ReadOnlyRepositoryHelper<T,TKey> ReadOnlyRepositoryHelperWithKey
        {
            get;
            private set;
        }

        public ReadOnlyRepository(BaseSession session)
            : this(session, new ReadOnlyRepositoryHelper<T, TKey>(new TypeKindInfo()))
        {

        }

        protected ReadOnlyRepository(BaseSession session, ReadOnlyRepositoryHelper<T,TKey> helper)
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

                AppendSelectFrom(query);

                query.Append(SyntaxHelper.SpaceToken);
                query.Append(SyntaxHelper.WhereToken);
                query.Append(SyntaxHelper.SpaceToken);

                AppendIdColumn(query);
                query.Append(SyntaxHelper.BinaryEqualToken);

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
