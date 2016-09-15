using ModernRoute.WildData.Core;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Models;
using System;

namespace ModernRoute.WildData.Npgsql.Core
{
    abstract class BaseReadWriteRepository<T,TKey> : BaseReadOnlyRepository<T,TKey>, IReadWriteRepository<T,TKey> where T : IReadWriteModel<TKey>, new()
    {
        protected ReadWriteRepositoryHelper<T,TKey> ReadWriteRepositoryHelper
        {
            get;
            private set;
        }

        public BaseReadWriteRepository(BaseSession session) 
            : this(session, new ReadWriteRepositoryHelper<T,TKey>())
        {

        }

        protected BaseReadWriteRepository(BaseSession session, ReadWriteRepositoryHelper<T,TKey> helper)
            : base(session, helper)
        {
            ReadWriteRepositoryHelper = helper;
        }

        public WriteResult Update(T entity)
        {
            throw new NotImplementedException();
        }

        public WriteResult Store(T entity)
        {
            throw new NotImplementedException();
        }
        
        public WriteResult StoreOrUpdate(T entity)
        {
            if (entity.IsNew)
            {
                return Store(entity);
            }

            return Update(entity);
        }

        public WriteResult Delete(TKey id)
        {
            throw new NotImplementedException();
        }

        public WriteResult Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return Delete(entity.Id);
        }
    }
}
