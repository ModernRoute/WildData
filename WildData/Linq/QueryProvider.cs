using ModernRoute.WildData.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Linq
{
    public class QueryProvider : IQueryProvider
    {
        private QueryExecutor _QueryExecutor;

        public QueryProvider(QueryExecutor queryExecutor)
        {
            if (queryExecutor == null)
            {
                throw new ArgumentNullException(nameof(queryExecutor));
            }

            _QueryExecutor = queryExecutor;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new Query<TElement>(this, expression);
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Type elementType = expression.Type.GetCollectionElementType();

            return Activator.CreateInstance(typeof(Query<>).MakeGenericType(elementType), new object[] { this, expression }).Of<IQueryable>();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return Execute(expression).Of<TResult>();
        }

        public object Execute(Expression expression)
        {
            return _QueryExecutor.Execute(expression);
        }
    }
}
