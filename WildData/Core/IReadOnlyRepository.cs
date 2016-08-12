using ModernRoute.WildData.Models;
using System.Linq;

namespace ModernRoute.WildData.Core
{
    public interface IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>, new()
    {
        IQueryable<T> Fetch();

        T Fetch(TKey id);
    }
}
