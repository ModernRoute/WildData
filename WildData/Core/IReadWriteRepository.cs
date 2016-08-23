using ModernRoute.WildData.Models;

namespace ModernRoute.WildData.Core
{
    public interface IReadWriteRepository<T, TKey> : IReadOnlyRepository<T, TKey> where T : IReadWriteModel<TKey>, new()
    {
        WriteResult Update(T entity);

        WriteResult Save(T entity);

        WriteResult SaveOrUpdate(T entity);

        WriteResult Delete(T entity);

        WriteResult Delete(TKey id);

        // Put additional common methods here
    }
}
