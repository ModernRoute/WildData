﻿using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Linq
{
    public class UnaryQueryExpression : QueryExpression
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

        public UnaryQueryExpression(QueryExpression expression, TypeKind typeKind, UnaryOperationType unaryOperationType) 
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
    }
}
