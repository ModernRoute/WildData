using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using System.Linq;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Linq
{
    public sealed class Queryable<T> : QueryableBase<T>
    {
        public Queryable(IQueryExecutor queryExecuter) : base(QueryParser.CreateDefault(), queryExecuter)
        {
            
        }

        public Queryable(IQueryProvider queryProvider, Expression expression)
            : base(queryProvider, expression)
        {

        }
    }
}
