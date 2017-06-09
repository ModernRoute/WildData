using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ModernRoute.WildData.Linq
{
    public sealed class FromSubquery : FromBase
    {
        public IReadOnlyList<Order> Orders
        {
            get;
            private set;
        }

        public bool Distinct
        {
            get;
            private set;
        }

        public int Offset
        {
            get;
            private set;
        }

        public int? Limit
        {
            get;
            private set;
        }

        public QueryExpression Predicate
        {
            get;
            private set;
        }

        public FromBase Source
        {
            get;
            private set;
        }

        public string SourceAlias
        {
            get;
            private set;
        }

        internal override FromSubquery WithDistinct(IAliasGenerator aliasGenerator)
        {
            return Recreate(Source, Predicate, true, Offset, Limit);
        }

        internal override FromSubquery WithOffset(IAliasGenerator aliasGenerator, int offset)
        {
            if (Offset == 0)
            {
                return Recreate(Source, Predicate, Distinct, offset, Limit);
            }

            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, offset, null);
        }

        internal override FromSubquery WithLimit(IAliasGenerator aliasGenerator, int limit)
        {
            if (Limit <= limit)
            {
                return this;
            }

            return Recreate(Source, Predicate, Distinct, Offset, limit);
        }

        internal override FromSubquery WithNewOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            if (Offset == 0 && Limit == null)
            {
                return new FromSubquery(MemberColumnMap, Projector, SourceAlias, Columns, Source, Predicate, orders, Distinct, Offset, Limit);
            }

            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, 
                this, Predicate, orders, Distinct, Offset, Limit);
        }

        internal override FromSubquery WithAdditionalOrders(IAliasGenerator aliasGenerator, params Order[] orders)
        {
            return new FromSubquery(MemberColumnMap, Projector, SourceAlias, Columns, 
                Source, Predicate, Orders.Concat(orders).ToList().AsReadOnly(), Distinct, Offset, Limit);
        }

        internal FromSubquery WithAdditionalOrder(params Order[] orders)
        {
            return new FromSubquery(MemberColumnMap, Projector, SourceAlias, Columns, 
                Source, Predicate, Orders.Concat(orders).ToList().AsReadOnly(), Distinct, Offset, Limit);
        }

        internal override FromSubquery WithWhere(IAliasGenerator aliasGenerator, QueryExpression expression)
        {
            if (expression == null)
            {
                return this;
            }

            QueryExpression newPredicate;

            if (Predicate == null)
            {
                newPredicate = expression;
            }
            else
            {
                if (expression.Type != TypeKind.Boolean)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Strings.PredicateDoesntReturnBoolValue, nameof(expression)));
                }

                newPredicate = new BinaryQueryExpression(Predicate, expression, TypeKind.Boolean, BinaryOperationType.And);
            }

            if (Offset == 0 && Limit == null)
            {
                return Recreate(Source, newPredicate, Distinct, Offset, Limit);
            }

            return new FromSubquery(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), Columns, this, newPredicate);
        }

        private FromSubquery Recreate(FromBase source, QueryExpression predicate, bool distinct, int offset, int? limit)
        {
            return new FromSubquery(MemberColumnMap, Projector, SourceAlias, Columns, 
                source, predicate, Orders, distinct, offset, limit);
        }

        internal FromSubquery(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, 
            IReadOnlyList<Column> columns, FromBase source, int offset, int? limit = null)
            : this(memberColumnMap, projector, sourceAlias, columns, 
                  source, null, null, false, offset, limit)
        {

        }

        internal FromSubquery(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, 
            IReadOnlyList<Column> columns, FromBase source, bool distinct)
            : this(memberColumnMap, projector, sourceAlias, columns, 
                  source, null, null, distinct, 0, null)
        {

        }

        internal FromSubquery(IReadOnlyDictionary<string, ColumnReference> memberColumnMap, Delegate projector, string sourceAlias, 
            IReadOnlyList<Column> columns, FromBase source, QueryExpression predicate = null)
            : this(memberColumnMap, projector, sourceAlias, columns, 
                  source, predicate, null, false, 0, null)
        {

        }

        internal FromSubquery(
            IReadOnlyDictionary<string,ColumnReference> memberColumnMap,
            Delegate projector,
            string sourceAlias,
            IReadOnlyList<Column> columns,
            FromBase source,
            QueryExpression predicate,
            IReadOnlyList<Order> orders,
            bool distinct, 
            int offset,
            int? limit) : base(memberColumnMap, columns, projector)
        {   
            if (sourceAlias == null)
            {
                throw new ArgumentNullException(nameof(sourceAlias));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (offset < 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.Strings.ParameterIsLessThanZero, nameof(offset)));
            }

            if (limit < 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture,
                    Resources.Strings.ParameterIsLessThanZero, nameof(limit)));
            }

            if (predicate != null && predicate.Type != TypeKind.Boolean)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.Strings.PredicateDoesntReturnBoolValue, nameof(predicate)));
            }

            Predicate = predicate;
            SourceAlias = sourceAlias;
            Source = source;
            Distinct = distinct;
            Offset = offset;
            Limit = limit;

            if (orders == null)
            {
                Orders = Enumerable.Empty<Order>().ToList().AsReadOnly();
            }
            else
            {
                Orders = orders;
                Orders.ThrowIfAnyNull();
            }
        }

        public override QueryElementType ElementType
        {
            get
            {
                return QueryElementType.FromSubquery;
            }
        }


        public override FromType FromType
        {
            get
            {
                return FromType.Regular;
            }
        }

        public override void Accept(QueryVisitor visitor)
        {
            visitor.VisitFromSubquery(this);
        }
    }
}
