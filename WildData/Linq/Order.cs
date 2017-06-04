using System;

namespace ModernRoute.WildData.Linq
{
    public sealed class Order : QueryElementBase
    {
        public OrderType Type
        {
            get;
            private set;
        }

        public QueryExpression Expression
        {
            get;
            private set;
        }

        internal Order(OrderType type, QueryExpression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            Type = type;
            Expression = expression;
        }

        public override QueryElementType ElementType
        {
            get 
            {
                return QueryElementType.Order; 
            }
        }
    }
}
