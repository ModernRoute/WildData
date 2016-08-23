using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq
{
    public class SourceQuery : SourceBase
    {
        private IReadOnlyList<FieldBase> _Fields;

        public override IReadOnlyList<FieldBase> Fields
        {
            get 
            {
                return _Fields;
            }
        }

        public IReadOnlyList<Column> Columns
        {
            get;
            private set;
        }

        public SourceQuery(IReadOnlyDictionary<string,ColumnReference> memberColumnMap, IEnumerable<Column> columns, Delegate projector)
            : base(memberColumnMap, projector)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            Columns = columns.ToList().AsReadOnly();

            Columns.ThrowIfAnyNull();

            _Fields = Columns.OfType<FieldBase>().ToList().AsReadOnly();
        }

        public override QueryElementType ElementType
        {
            get
            {
                return QueryElementType.SourceQuery;
            }
        }

        public override SelectType SelectType
        {
            get
            {
                return SelectType.Source;
            }
        }

        internal override SelectBase WithDistinct(IAliasGenerator aliasGenerator)
        {
            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, true);
        }

        internal override SelectBase WithOffset(IAliasGenerator aliasGenerator, int offset)
        {
            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, offset);
        }

        internal override SelectBase WithLimit(IAliasGenerator aliasGenerator, int limit)
        {
            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, 0, limit);
        }

        internal override SelectBase WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, null, orders, false, 0, null);
        }

        internal override SelectBase WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, null, orders, false, 0, null);
        }

        internal override SelectBase WithWhere(IAliasGenerator aliasGenerator, QueryExpression expression)
        {
            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, expression);
        }
    }
}
