using ModernRoute.WildData.Core;
using System;
using System.Collections.Generic;

namespace ModernRoute.WildData.Linq
{
    public sealed class FromSource : FromBase
    {
        internal FromSource(IReadOnlyDictionary<string,ColumnReference> memberColumnMap, IReadOnlyList<Column> columns, Delegate projector)
            : base(memberColumnMap, columns, projector)
        {

        }

        public override QueryElementType ElementType
        {
            get
            {
                return QueryElementType.FromSource;
            }
        }

        public override FromType FromType
        {
            get
            {
                return FromType.Source;
            }
        }

        internal override FromSubquery WithDistinct(IAliasGenerator aliasGenerator)
        {
            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, true);
        }

        internal override FromSubquery WithOffset(IAliasGenerator aliasGenerator, int offset)
        {
            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, offset);
        }

        internal override FromSubquery WithLimit(IAliasGenerator aliasGenerator, int limit)
        {
            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, 0, limit);
        }

        internal override FromSubquery WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, null, orders, false, 0, null);
        }

        internal override FromSubquery WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, null, orders, false, 0, null);
        }

        internal override FromSubquery WithWhere(IAliasGenerator aliasGenerator, QueryExpression expression)
        {
            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, expression);
        }

        public override void Accept(QueryVisitor visitor)
        {
            visitor.VisitFromSource(this);
        }
    }
}
