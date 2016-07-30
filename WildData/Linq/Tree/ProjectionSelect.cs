using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq.Tree
{
    public class ProjectionSelect : SelectBase
    {
        private IReadOnlyList<FieldBase> _Fields;

        public override IReadOnlyList<FieldBase> Fields
        {
            get
            {
                return _Fields;
            }
        }

        public IReadOnlyList<Projection> Projections
        {
            get;
            private set;
        }

        public ProjectionSelect(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, IEnumerable<Projection> projections, SourceBase source)
            : this(memberColumnMap, projector, sourceAlias, projections, source, null, false, 0, null)
        {

        }

        public ProjectionSelect(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, IEnumerable<Projection> projections, SourceBase source, 
            QueryExpression predicate, bool distinct, int offset, int? limit)
            : base(memberColumnMap, projector, sourceAlias, source, predicate, distinct, offset, limit)
        {
            if (projections == null)
            {
                throw new ArgumentNullException(nameof(projections));
            }

            Projections = projections.ToList().AsReadOnly();

            Projections.ThrowIfAnyNull();

            _Fields = Projections.OfType<FieldBase>().ToList().AsReadOnly();
        }

        public override QueryElementType ElementType
        {
            get
            {
                return QueryElementType.ProjectionSelect;
            }
        }

        protected override SelectBase Recreate(SourceBase source, QueryExpression predicate, bool distinct, int offset, int? limit)
        {
            return new ProjectionSelect(MemberColumnMap, Projector, SourceAlias, Projections, source, predicate, distinct, offset, limit);
        }

        internal override SelectBase WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return this;
        }

        internal override SelectBase WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return this;
        }

        public override SelectType SelectType
        {
            get 
            {
                return SelectType.Projection; 
            }
        }
    }
}
