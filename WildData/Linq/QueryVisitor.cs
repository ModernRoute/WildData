using ModernRoute.WildData.Core;
using System;
using System.Collections.Generic;

namespace ModernRoute.WildData.Linq
{
    public abstract class QueryVisitor
    {
        protected internal virtual void Visit(QueryElementBase queryElementBase)
        {
            queryElementBase.Accept(this);
        }

        protected internal virtual void VisitFromSource(FromSource fromSource)
        {
              
        }

        protected internal virtual void VisitColumns(IReadOnlyList<Column> columns)
        {   
            for (int i = 0; i < columns.Count; i++)
            {            
                VisitColumn(columns[i], i);
            }
        }

        protected internal virtual void VisitColumn(Column column, int index)
        {
            VisitProjection(column.ProjectionType, column.ProjectionDistinct, column.Definition);
            VisitAlias(column.Alias);
        }

        protected internal virtual void VisitProjection(ProjectionType projectionType, bool projectionDistinct, QueryExpression definition)
        {
            Visit(definition);
        }

        protected internal virtual void VisitFromSubquery(FromSubquery fromSubquery)
        {
            VisitColumns(fromSubquery.Columns);
            VisitSource(fromSubquery.Source);
            VisitAlias(fromSubquery.SourceAlias);

            if (fromSubquery.Predicate != null)
            {
                VisitPredicate(fromSubquery.Predicate);
            }

            if (fromSubquery.Orders.Count > 0)
            {
                VisitOrders(fromSubquery.Orders);
            }
            
            VisitOffsetLimit(fromSubquery.Offset, fromSubquery.Limit);
        }

        protected internal virtual void VisitSource(FromBase source)
        {
            Visit(source);
        }

        protected internal virtual void VisitPredicate(QueryExpression predicate)
        {
            Visit(predicate);
        }

        protected internal virtual void VisitOffsetLimit(int offset, int? limit)
        {
            
        }

        protected internal virtual void VisitOrders(IReadOnlyList<Order> orders)
        {
            for (int i = 0; i < orders.Count; i++)
            {
                VisitOrder(orders[i], i);
            } 
        }

        protected internal virtual void VisitOrder(Order order, int index)
        {
            Visit(order.Expression);
        }

        protected internal virtual void VisitAlias(string alias)
        {
            
        }

        protected internal virtual void VisitBinaryQueryExpression(BinaryQueryExpression binaryQueryExpression)
        {
            Visit(binaryQueryExpression.Left);
            VisitBinaryOperationType(binaryQueryExpression.Operation, binaryQueryExpression.Type);
            Visit(binaryQueryExpression.Right);
        }

        protected internal virtual void VisitBinaryOperationType(BinaryOperationType operation, TypeKind typeKind)
        {
            
        }

        protected internal virtual void VisitUnaryExpression(UnaryQueryExpression unaryQueryExpression)
        {
            VisitUnaryOperationType(unaryQueryExpression.Operation);
            Visit(unaryQueryExpression.Expression);
        }

        protected internal virtual void VisitUnaryOperationType(UnaryOperationType operation)
        {
            
        }

        protected internal virtual void VisitColumnReference(ColumnReference columnReference)
        {
            
        }

        protected internal virtual void VisitFunctionCall(FunctionCall functionCall)
        {
            VisitFunctionType(functionCall.FunctionType);
            VisitFunctionArgs(functionCall.Arguments);
        }

        protected internal virtual void VisitFunctionArgs(IEnumerable<QueryExpression> args)
        {
            int i = 0;

            foreach (QueryExpression arg in args)
            {
                VisitFunctionArg(arg, i++);
            }
        }

        protected internal virtual void VisitFunctionArg(QueryExpression arg, int index)
        {
            Visit(arg);
        }

        protected internal virtual void VisitFunctionType(FunctionType functionType)
        {
            
        }

        protected internal virtual void VisitQueryConstant(QueryConstant queryConstant)
        {
            
        }
    }
}
