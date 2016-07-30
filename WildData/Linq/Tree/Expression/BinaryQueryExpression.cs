using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Linq.Tree.Expression
{
    public class BinaryQueryExpression : QueryExpression
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

        public BinaryQueryExpression(QueryExpression left, QueryExpression right, ReturnType returnType, BinaryOperationType binaryOperationType) 
            : base(returnType)
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
