using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Linq
{
    public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        public Type ElementType
        {
            get
            {
                return typeof(T);
            }
        }

        public Expression Expression
        {
            get;
            private set;
        }

        public IQueryProvider Provider
        {
            get
            {
                return QueryProvider;
            }
        }

        public QueryProvider QueryProvider
        {
            get;
            private set;
        }

        public Query(QueryProvider queryProvider)
            : this(queryProvider, null)
        {

        }

        public Query(QueryProvider queryProvider, Expression expression)
        {
            if (queryProvider == null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            Expression = expression ?? Expression.Constant(this);
            QueryProvider = queryProvider;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return QueryProvider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return QueryProvider.Execute<IEnumerable>(Expression).GetEnumerator();
        }
    }
}
