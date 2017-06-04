using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Linq
{
    public sealed class BinaryQueryExpression : QueryExpression
    {
        public QueryExpression Left
        {
            get;
            private set;
        }

        public QueryExpression Right
        {
            get;
            private set;
        }

        public BinaryOperationType Operation
        {
            get;
            private set;
        }

        internal BinaryQueryExpression(QueryExpression left, QueryExpression right, TypeKind typeKind, BinaryOperationType binaryOperationType) 
            : base(typeKind)
        {
            if (left == null)
            {
                throw new ArgumentNullException(nameof(left));
            }

            if (right == null)
            {
                throw new ArgumentNullException(nameof(right));
            }

            Operation = binaryOperationType;
            Left = left;
            Right = right;
        }

        public override QueryExpressionType ExpressionType
        {
            get 
            {
                return QueryExpressionType.BinaryOperation; 
            }
        }
    }
}
