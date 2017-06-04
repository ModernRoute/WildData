using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Linq
{
    public abstract class QueryExpression : QueryElementBase
    {
        public abstract QueryExpressionType ExpressionType
        {
            get;
        }

        public TypeKind Type
        {
            get;
            private set;
        }

        internal QueryExpression(TypeKind type)
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
