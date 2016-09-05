using ModernRoute.WildData.Core;
using ModernRoute.WildData.Models;
using Npgsql;
using System;

namespace ModernRoute.WildData.Npgsql.Core
{
    abstract class BaseReadWriteRepository<T,TKey> : BaseReadOnlyRepository<T,TKey>, IReadWriteRepository<T,TKey> where T : IReadWriteModel<TKey>, new()
    {
        public BaseReadWriteRepository(BaseSession session) 
            : base(session)
        {

        }

        public abstract T Fetch(int id);

        public abstract WriteResult Update(T entity);

        public abstract WriteResult Store(T entity);
        
        public WriteResult StoreOrUpdate(T entity)
        {
            if (entity.IsNew)
            {
                return Store(entity);
            }

            return Update(entity);
        }

        public abstract WriteResult Delete(TKey id);

        public WriteResult Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return Delete(entity.Id);
        }

        protected abstract void SetParametersFromObject(NpgsqlParameterCollection parameters, T entity);        

        protected string GetParamName(string fieldName)
        {
            return string.Join("@", fieldName);
        }
    }
}
