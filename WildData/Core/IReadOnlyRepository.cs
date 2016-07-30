using ModernRoute.WildData.Models;
using System.Linq;

namespace ModernRoute.WildData.Core
{
    public interface IReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>
    {
        IQueryable<T> Fetch();

        T Fetch(TKey id);
    }
}
