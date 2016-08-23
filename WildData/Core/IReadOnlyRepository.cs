using ModernRoute.WildData.Models;
using System.Linq;

namespace ModernRoute.WildData.Core
{
    public interface IReadOnlyRepository<T> where T : IReadOnlyModel, new()
    {
        IQueryable<T> Fetch();
    }

    public interface IReadOnlyRepository<T, TKey> : IReadOnlyRepository<T> where T : IReadOnlyModel<TKey>, new()
    {
        T Fetch(TKey id);
    }
}
