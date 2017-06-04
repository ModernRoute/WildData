using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
using Remotion.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq
{
    public abstract class QueryExecutor : IQueryExecutor
    {
        private QueryConverter _QueryConverter;
        
        private static SourceQuery CreateSourceQuery(IReadOnlyDictionary<string, ColumnInfo> memberColumnMap, Delegate projector)
        {
            IReadOnlyDictionary<string, ColumnReference> newMemberColumnMap =
                memberColumnMap.ToDictionary(item => item.Key, item => item.Value.ColumnReference).AsReadOnly();

            IEnumerable<Column> columns = newMemberColumnMap.Select(i => new Column(i.Value.ColumnName, i.Value));

            return new SourceQuery(newMemberColumnMap, columns, projector);
        }

        public QueryExecutor(IReadOnlyDictionary<string, ColumnInfo> memberColumnMap, Delegate reader)
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

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(_QueryConverter.Convert(queryModel));
        }

        protected abstract IEnumerable<T> ExecuteCollection<T>(SourceBase sourceBase);

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            IEnumerable<T> sequence = ExecuteCollection<T>(queryModel);

            if (returnDefaultWhenEmpty)
            {
                return sequence.SingleOrDefault();
            }
            else
            {
                return sequence.Single();
            }
        }
    }
}
