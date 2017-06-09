using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Linq
{
    public sealed class UnaryQueryExpression : QueryExpression
    {
        public QueryExpression Expression
        {
            get;
            private set;
        }

        public UnaryOperationType Operation
        {
            get;
            private set;
        }

        internal UnaryQueryExpression(QueryExpression expression, TypeKind typeKind, UnaryOperationType unaryOperationType) 
            : base(typeKind)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Operation = unaryOperationType;
            Expression = expression;
        }

        public override QueryExpressionType ExpressionType
        {
            get
            {
                return QueryExpressionType.UnaryOperation;
            }
        }

        public override void Accept(QueryVisitor visitor)
        {
            visitor.VisitUnaryExpression(this);
        }
    }
}
