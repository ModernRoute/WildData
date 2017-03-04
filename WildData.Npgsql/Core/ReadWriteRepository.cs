using ModernRoute.WildData.Core;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Npgsql.Helpers;
using ModernRoute.WildData.Npgsql.Resources;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModernRoute.WildData.Npgsql.Core
{
    public class ReadWriteRepository<T,TKey> : ReadOnlyRepository<T,TKey>, IReadWriteRepository<T,TKey> where T : IReadWriteModel<TKey>, new()
    {
        protected ReadWriteRepositoryHelper<T,TKey> ReadWriteRepositoryHelper
        {
            get;
            private set;
        }

        public ReadWriteRepository(BaseSession session) 
            : this(session, new ReadWriteRepositoryHelper<T,TKey>(new TypeKindInfo()))
        {

        }

        protected ReadWriteRepository(BaseSession session, ReadWriteRepositoryHelper<T,TKey> helper)
            : base(session, helper)
        {
            ReadWriteRepositoryHelper = helper;
        }

        public WriteResult Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!entity.IsPersistent())
            {
                throw new InvalidOperationException(Strings.TheEntityIsNotPersistent);
            }

            using (NpgsqlCommand command = Session.CreateCommand())
            {
                DbParameterCollectionWrapper collectionWrapper = new DbParameterCollectionWrapper(command.Parameters);

                ReadWriteRepositoryHelper.SetParametersFromObjectForUpdate(collectionWrapper, entity);

                StringBuilder query = new StringBuilder();

                query.Append(SyntaxHelper.UpdateToken);
                query.Append(SyntaxHelper.Space);

                AppendStorageName(query);

                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.SetToken);
                query.Append(SyntaxHelper.Space);

                AppendColumnValuesUpdate(query);

                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.WhereToken);
                query.Append(SyntaxHelper.Space);

                string idParamName = ReadOnlyRepositoryHelperWithKey.GetIdParamName();

                AppendIdColumn(query);
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.EqualSign);
                query.Append(SyntaxHelper.Space);
                query.Append(idParamName);

                ReadOnlyRepositoryHelperWithKey.AddIdParameter(collectionWrapper, entity.Id);

                if (ReadWriteRepositoryHelper.VolatileOnUpdateMemberColumnMap.Count > 0)
                {
                    query.Append(SyntaxHelper.Space);
                    query.Append(SyntaxHelper.ReturningToken);
                    query.Append(SyntaxHelper.Space);

                    AppendColumnList(query, ReadWriteRepositoryHelper.VolatileOnUpdateMemberColumnMap);

                    command.CommandText = query.ToString();
                    command.Prepare();

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        UpdateVolatileColumns(ReadWriteRepositoryHelper.UpdateVolatileColumnsOnUpdate, reader, entity);

                        return WriteResult.ForSingleRow(reader.RecordsAffected);
                    }
                }
                else
                {
                    command.CommandText = query.ToString();
                    command.Prepare();

                    return WriteResult.ForSingleRow(command.ExecuteNonQuery());
                }
            }
        }

        private void UpdateVolatileColumns(Action<IReaderWrapper, T> updateVolatileColumnsAction, NpgsqlDataReader reader, T entity)
        {
            ReaderWrapper readerWrapper = new ReaderWrapper(reader);

            bool gotRow = false;

            while (reader.Read())
            {
                if (gotRow)
                {
                    throw new InvalidOperationException(Strings.ExactlyOneRowExpected);
                }

                gotRow = true;

                updateVolatileColumnsAction(readerWrapper, entity);
            }

            if (!gotRow)
            {
                throw new InvalidOperationException(Strings.ExactlyOneRowExpected);
            }
        }

        public WriteResult Store(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (entity.IsPersistent())
            {
                throw new InvalidOperationException(Strings.TheEntityIsPersistent);
            }

            using (NpgsqlCommand command = Session.CreateCommand())
            {
                DbParameterCollectionWrapper collectionWrapper = new DbParameterCollectionWrapper(command.Parameters);

                ReadWriteRepositoryHelper.SetParametersFromObjectForStore(collectionWrapper, entity);

                StringBuilder query = new StringBuilder();

                query.Append(SyntaxHelper.InsertToken);
                query.Append(SyntaxHelper.Space);

                AppendStorageName(query);

                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.LeftParenthesis);

                AppendColumnList(query);

                query.Append(SyntaxHelper.RightParenthesis);
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.ValuesToken);

                query.Append(SyntaxHelper.LeftParenthesis);

                AppendColumnValuesStore(query);

                query.Append(SyntaxHelper.RightParenthesis);

                int rowsAffected;

                if (ReadWriteRepositoryHelper.VolatileOnStoreMemberColumnMap.Count > 0)
                {
                    query.Append(SyntaxHelper.Space);
                    query.Append(SyntaxHelper.ReturningToken);
                    query.Append(SyntaxHelper.Space);

                    AppendColumnList(query, ReadWriteRepositoryHelper.VolatileOnStoreMemberColumnMap);

                    command.CommandText = query.ToString();
                    command.Prepare();

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        UpdateVolatileColumns(ReadWriteRepositoryHelper.UpdateVolatileColumnsOnStore, reader, entity);

                        rowsAffected = reader.RecordsAffected;
                    }
                }
                else
                {
                    command.CommandText = query.ToString();
                    command.Prepare();

                    rowsAffected = command.ExecuteNonQuery();
                }

                WriteResult result = WriteResult.ForSingleRow(rowsAffected);

                if (result.ResultType == WriteResultType.Ok)
                {
                    entity.SetPersistent(true);
                }

                return result;
            }
        }

        private void AppendColumnValuesUpdate(StringBuilder query)
        {
            bool first = true;

            foreach (KeyValuePair<string, ColumnInfo> columnInfo in ReadWriteRepositoryHelper.MemberColumnMapWithoutId)
            {
                if (columnInfo.Value.VolatileKindOnUpdate == VolatileKind.Regular)
                {
                    continue;
                }

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
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.EqualSign);
                query.Append(SyntaxHelper.Space);
                query.Append(columnInfo.Value.ParamNameBase);
            }

            if (first)
            {
                AppendIdColumn(query);
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.EqualSign);
                query.Append(SyntaxHelper.Space);
                AppendIdColumn(query);
            }
        }

        private void AppendColumnValuesStore(StringBuilder query)
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
                    query.Append(SyntaxHelper.Comma);
                    query.Append(SyntaxHelper.Space);
                }

                if (columnInfo.Value.VolatileKindOnStore == VolatileKind.Regular)
                {
                    query.Append(SyntaxHelper.DefaultToken);
                }
                else
                {
                    query.Append(columnInfo.Value.ParamNameBase);
                }
            }
        }

        public WriteResult StoreOrUpdate(T entity)
        {
            if (!entity.IsPersistent())
            {
                return Store(entity);
            }

            return Update(entity);
        }

        public WriteResult Delete(TKey id)
        {
            using (NpgsqlCommand command = Session.CreateCommand())
            {
                StringBuilder query = new StringBuilder();

                query.Append(SyntaxHelper.DeleteToken);
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.FromToken);
                query.Append(SyntaxHelper.Space);

                AppendStorageName(query);

                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.WhereToken);
                query.Append(SyntaxHelper.Space);

                string idParamName = ReadOnlyRepositoryHelperWithKey.GetIdParamName();

                AppendIdColumn(query);
                query.Append(SyntaxHelper.Space);
                query.Append(SyntaxHelper.EqualSign);
                query.Append(SyntaxHelper.Space);
                query.Append(idParamName);

                ReadOnlyRepositoryHelperWithKey.AddIdParameter(new DbParameterCollectionWrapper(command.Parameters), id);

                command.CommandText = query.ToString();
                command.Prepare();

                return WriteResult.ForSingleRow(command.ExecuteNonQuery());
            }
        }

        public WriteResult Delete(T entity)
        {
            if (!entity.IsPersistent())
            {
                throw new InvalidOperationException(Strings.TheEntityIsNotPersistent);
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            WriteResult result = Delete(entity.Id);

            if (result.ResultType == WriteResultType.Ok)
            {
                entity.SetPersistent(false);
            }

            return result;
        }
    }
}
