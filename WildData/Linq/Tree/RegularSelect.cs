using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq.Tree
{
    public class RegularSelect : SelectBase
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

        public IReadOnlyList<Order> OrderCollection
        {
            get;
            private set;
        }

        public RegularSelect(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, IEnumerable<Column> columns, SourceBase source, int offset, int? limit = null)
            : this(memberColumnMap, projector, sourceAlias, columns, source, null, Enumerable.Empty<Order>(), false, offset, limit)
        {

        }

        public RegularSelect(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, IEnumerable<Column> columns, SourceBase source, bool distinct)
            : this(memberColumnMap, projector, sourceAlias, columns, source, null, Enumerable.Empty<Order>(), distinct, 0, null)
        {

        }

        public RegularSelect(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, IEnumerable<Column> columns, SourceBase source, QueryExpression predicate = null)
            : this(memberColumnMap, projector, sourceAlias, columns, source, predicate, Enumerable.Empty<Order>(), false, 0, null)
        {

        }

        public RegularSelect WithAdditionalOrder(params Order[] orders)
        {
            return new RegularSelect(MemberColumnMap, Projector, SourceAlias, Columns, Source, Predicate, OrderCollection.Concat(orders), Distinct, Offset, Limit);
        }

        public RegularSelect(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string alias, IEnumerable<Column> columns, SourceBase source, 
            QueryExpression predicate, IEnumerable<Order> orderCollection, bool distinct, int offset, int? limit)
            : base(memberColumnMap, projector, alias, source, predicate, distinct, offset, limit)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (orderCollection == null)
            {
                throw new ArgumentNullException(nameof(orderCollection));
            }
           
            Columns = columns.ToList().AsReadOnly();
            Columns.ThrowIfAnyNull();

            OrderCollection = orderCollection.ToList().AsReadOnly();
            OrderCollection.ThrowIfAnyNull();

            _Fields = Columns.OfType<FieldBase>().ToList().AsReadOnly();
        }

        public override QueryElementType ElementType
        {
            get
            {
                return QueryElementType.RegularSelect; 
            }
        }

        protected override SelectBase Recreate(SourceBase source, QueryExpression predicate, bool distinct, int offset, int? limit)
        {
            return new RegularSelect(MemberColumnMap, Projector, SourceAlias, Columns, source, predicate, OrderCollection, distinct, offset, limit);
        }

        public override SelectType SelectType
        {
            get
            {
                return SelectType.Regular;
            }
        }

        internal override SelectBase WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            if (Offset == 0 && Limit == null)
            {
                return new RegularSelect(MemberColumnMap, Projector, SourceAlias, Columns, Source, Predicate, orders, Distinct, Offset, Limit);
            }

            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, Predicate, orders, Distinct, Offset, Limit);
        }

        internal override SelectBase WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return new RegularSelect(MemberColumnMap, Projector, SourceAlias, Columns, Source, Predicate, OrderCollection.Concat(orders), Distinct, Offset, Limit);        
        }
    }
}
