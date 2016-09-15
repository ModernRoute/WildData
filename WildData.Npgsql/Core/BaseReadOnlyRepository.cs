using ModernRoute.WildData.Core;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Npgsql.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

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
                IReaderWrapper wrapper = new NpgsqlReaderWrapper(reader);

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
            throw new NotImplementedException();
        }
    }

    abstract class BaseReadOnlyRepository<T,TKey> : BaseReadOnlyRepository<T>, IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>, new()
    {
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
            throw new NotImplementedException();
        }
    }
}
