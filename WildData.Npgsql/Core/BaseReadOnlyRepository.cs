using ModernRoute.WildData.Core;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Npgsql.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Npgsql.Core
{
    abstract class BaseReadOnlyRepository<T,TKey> : IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>
    {
        public BaseReadOnlyRepository(BaseSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            _Session = session;
        }
        
        internal readonly BaseSession _Session;

        internal abstract T ReadSingleObject(IReaderWrapper reader);

        protected IEnumerable<T> ExecuteReader(NpgsqlCommand command)
        {
            using (NpgsqlDataReader reader = command.ExecuteReader())
            {
                IReaderWrapper wrapper = new NpgsqlReaderWrapper(reader);

                while (reader.Read())
                {
                    yield return ReadSingleObject(wrapper);
                }
            }
        }

        public abstract IQueryable<T> Fetch();

        public abstract T Fetch(TKey id);
    }
}
