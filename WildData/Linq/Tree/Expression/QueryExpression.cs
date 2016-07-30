using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Linq.Tree.Expression
{
    public abstract class QueryExpression : QueryElementBase
    {
        public abstract QueryExpressionType ExpressionType
        {
            get;
        }

        public ReturnType Type
        {
            get;
            private set;
        }

        public QueryExpression(ReturnType type)
        {
            Type = type;
        }

        public override QueryElementType ElementType
        {
            get 
            {
                return QueryElementType.QueryExpression;
            }
        }
    }
}
