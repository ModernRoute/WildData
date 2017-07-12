using ModernRoute.WildData.Linq;
using System;
using ModernRoute.WildData.Core;
using System.Collections.Generic;
using System.Text;
using ModernRoute.WildData.Test.Helpers;

namespace ModernRoute.WildData.Test.Linq
{
    class CanonicalQueryBuilder : QueryVisitor
    {
        private string _SourceQuery;
        private StringBuilder _QueryString;

        public CanonicalQueryBuilder(string sourceQuery)
        {
            if (sourceQuery == null)
            {
                throw new ArgumentNullException(nameof(sourceQuery));
            }

            _SourceQuery = sourceQuery;
        }

        public Tuple<string, Delegate> GetInvariantRepresentation(FromBase query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            _QueryString = new StringBuilder();

            try
            {
                Visit(query);
                return new Tuple<string, Delegate>(_QueryString.ToString(), query.Projector);
            }
            finally
            {
                _QueryString = null;
            }
        }

        private void AppendCommaIfNotFirst(int index)
        {
            if (index != 0)
            {
                _QueryString.Append(SyntaxHelper.CommaToken);
            }
        }

        protected override void VisitAlias(string alias)
        {
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.AsToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.QuoteToken);
            _QueryString.Append(EscapeHelper.EscapeString(alias));
            _QueryString.Append(SyntaxHelper.QuoteToken);
        }

        protected override void VisitBinaryOperationType(BinaryOperationType operation, TypeKind typeKind)
        {
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(operation.ToString());
            _QueryString.Append(SyntaxHelper.SpaceToken);
        }

        protected override void VisitBinaryQueryExpression(BinaryQueryExpression binaryQueryExpression)
        {
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            base.VisitBinaryQueryExpression(binaryQueryExpression);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        protected override void VisitColumn(Column column, int index)
        {
            AppendCommaIfNotFirst(index);
            base.VisitColumn(column, index);
        }

        protected override void VisitColumnReference(ColumnReference columnReference)
        {
            _QueryString.Append(SyntaxHelper.QuoteToken);
            _QueryString.Append(EscapeHelper.EscapeString(columnReference.ColumnName));
            _QueryString.Append(SyntaxHelper.QuoteToken);
        }


        protected override void VisitFromSource(FromSource fromSource)
        {
            _QueryString.Append(_SourceQuery);
        }

        protected override void VisitFromSubquery(FromSubquery fromSubquery)
        {
            _QueryString.Append(SyntaxHelper.SelectToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);

            if (fromSubquery.Distinct)
            {
                _QueryString.Append(SyntaxHelper.DistinctToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
            }

            base.VisitFromSubquery(fromSubquery);
        }

        protected override void VisitFunctionArg(QueryExpression arg, int index)
        {
            AppendCommaIfNotFirst(index);
            base.VisitFunctionArg(arg, index);
        }


        protected override void VisitFunctionCall(FunctionCall functionCall)
        {
            base.VisitFunctionCall(functionCall);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        protected override void VisitFunctionType(FunctionType functionType)
        {
            _QueryString.Append(functionType.ToString());
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            base.VisitFunctionType(functionType);
        }

        protected override void VisitOffsetLimit(int offset, int? limit)
        {
            if (limit == null)
            {
                if (offset <= 0)
                {
                    return;
                }

                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(SyntaxHelper.LimitToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(SyntaxHelper.NoLimitValue);
            }
            else
            {
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(SyntaxHelper.LimitToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(limit.Value);
            }

            if (offset > 0)
            {
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(SyntaxHelper.OffsetToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(offset);
            }
        }

        protected override void VisitOrder(Order order, int index)
        {
            AppendCommaIfNotFirst(index);
            base.VisitOrder(order, index);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(order.Type.ToString());
        }

        protected override void VisitOrders(IReadOnlyList<Order> orders)
        {
            _QueryString.Append(SyntaxHelper.OrderByToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            base.VisitOrders(orders);
        }

        protected override void VisitPredicate(QueryExpression predicate)
        {
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.WhereToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            base.VisitPredicate(predicate);
        }

        protected override void VisitProjection(ProjectionType projectionType, bool projectionDistinct, QueryExpression definition)
        {
            if (projectionType != ProjectionType.None)
            {
                _QueryString.Append(projectionType.ToString());
                _QueryString.Append(SyntaxHelper.LeftParenthesisToken);

                if (projectionDistinct)
                {
                    _QueryString.Append(SyntaxHelper.DistinctToken);
                    _QueryString.Append(SyntaxHelper.SpaceToken);
                }

                base.VisitProjection(projectionType, projectionDistinct, definition);
                _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            }
            else
            {
                base.VisitProjection(projectionType, projectionDistinct, definition);
            }
        }

        protected override void VisitQueryConstant(QueryConstant queryConstant)
        {
            _QueryString.Append(queryConstant.InvariantRepresentation ?? SyntaxHelper.NullToken);
        }

        protected override void VisitSource(FromBase source)
        {
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.FromToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            base.VisitSource(source);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        protected override void VisitUnaryExpression(UnaryQueryExpression unaryQueryExpression)
        {
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            base.VisitUnaryExpression(unaryQueryExpression);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        protected override void VisitUnaryOperationType(UnaryOperationType operation)
        {
            _QueryString.Append(operation.ToString());
            _QueryString.Append(SyntaxHelper.SpaceToken);
        }
    }
}
