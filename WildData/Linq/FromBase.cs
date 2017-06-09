using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Collections.Generic;

namespace ModernRoute.WildData.Linq
{
    public abstract class FromBase : QueryElementBase
    {
        public Delegate Projector
        {
            get;
            private set;
        }

        public abstract FromType FromType
        {
            get;
        }

        internal abstract FromSubquery WithDistinct(IAliasGenerator aliasGenerator);

        internal abstract FromSubquery WithOffset(IAliasGenerator aliasGenerator, int offset);

        internal abstract FromSubquery WithLimit(IAliasGenerator aliasGenerator, int limit);

        internal abstract FromSubquery WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders);

        internal abstract FromSubquery WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders);

        internal abstract FromSubquery WithWhere(IAliasGenerator aliasGenerator, QueryExpression expression);

        internal FromSubquery WithOrders(IAliasGenerator aliasGenerator, bool additional, params Order[] orders)
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

        public IReadOnlyList<Column> Columns
        {
            get;
            private set;
        }

        public IReadOnlyDictionary<string, ColumnReference> MemberColumnMap
        {
            get;
            private set;
        }

        internal FromBase(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, IReadOnlyList<Column> columns, Delegate projector)
        {
            if (memberColumnMap == null)
            {
                throw new ArgumentNullException(nameof(memberColumnMap));
            }

            if (projector == null)
            {
                throw new ArgumentNullException(nameof(projector));
            }

            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            MemberColumnMap = memberColumnMap;
            Projector = projector;
            Columns = columns;
            Columns.ThrowIfAnyNull();
        }
    }
}
