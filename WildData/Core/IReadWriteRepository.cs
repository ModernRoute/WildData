using ModernRoute.WildData.Models;

namespace ModernRoute.WildData.Core
{
    public interface IReadWriteRepository<T, TKey> : IReadOnlyRepository<T, TKey> where T : IReadWriteModel<TKey>, new()
    {
        WriteResult Update(T entity);

        WriteResult Store(T entity);

        WriteResult StoreOrUpdate(T entity);

        WriteResult Delete(T entity);

        WriteResult Delete(TKey id);
    }
}
