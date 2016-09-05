using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Linq
{
    public abstract class QueryExecutor
    {
        private QueryConverter _QueryConverter;

        private static SourceQuery CreateSourceQuery(IReadOnlyDictionary<string, ColumnDescriptor> memberColumnMap, Delegate projector)
        {
            IReadOnlyDictionary<string, ColumnReference> newMemberColumnMap = 
                memberColumnMap.ToDictionary(item => item.Key, item => item.Value.ColumnInfo.ColumnReference).AsReadOnly();

            IEnumerable<Column> columns = newMemberColumnMap.Select(i => new Column(i.Value.ColumnName, i.Value));

            return new SourceQuery(newMemberColumnMap, columns, projector);
        }

        public QueryExecutor(IReadOnlyDictionary<string, ColumnDescriptor> memberColumnMap, Delegate reader)
        {
            if (memberColumnMap == null)
            {
                throw new ArgumentNullException(nameof(memberColumnMap));
            }

            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            _QueryConverter = new QueryConverter(CreateSourceQuery(memberColumnMap, reader));
        }

        protected SourceBase ConvertExpression(Expression expression)
        {
            return _QueryConverter.Convert(expression);
        }

        public virtual object Execute(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            SourceBase source = ConvertExpression(expression);

            return Execute(source);
        }

        public abstract object Execute(SourceBase sourceBase);
    }
}
