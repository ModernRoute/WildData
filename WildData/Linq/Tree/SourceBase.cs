using ModernRoute.WildData.Linq.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq.Tree
{
    public abstract class SourceBase : QueryElementBase
    {
        public Delegate Projector
        {
            get;
            private set;
        }

        public abstract SelectType SelectType
        {
            get;
        }

        internal abstract SelectBase WithDistinct(IAliasGenerator aliasGenerator);

        internal abstract SelectBase WithOffset(IAliasGenerator aliasGenerator, int offset);

        internal abstract SelectBase WithLimit(IAliasGenerator aliasGenerator, int limit);

        internal abstract SelectBase WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders);

        internal abstract SelectBase WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders);

        internal abstract SelectBase WithWhere(IAliasGenerator aliasGenerator, QueryExpression expression);

        internal SelectBase WithOrders(IAliasGenerator aliasGenerator, bool additional, params Order[] orders)
        {
            if (additional)
            {
                return WithAdditionalOrders(aliasGenerator, orders);
            }
            else
            {
                return WithNewOrders(aliasGenerator, orders);
            }
        }

        internal IEnumerable<Column> GetColumnReferences()
        {
            return Fields.Select(f => new Column(f.Alias, f.GetColumnReference()));
        }

        public abstract IReadOnlyList<FieldBase> Fields { get; }

        public IReadOnlyDictionary<string, ColumnReference> MemberColumnMap
        {
            get;
            private set;
        }

        public SourceBase(IReadOnlyDictionary<string,ColumnReference> memberColumnMap, Delegate projector)
        {
            if (memberColumnMap == null)
            {
                throw new ArgumentNullException(nameof(memberColumnMap));
            }

            if (projector == null)
            {
                throw new ArgumentNullException(nameof(projector));
            }

            MemberColumnMap = memberColumnMap;
            Projector = projector; 
        }
    }
}
