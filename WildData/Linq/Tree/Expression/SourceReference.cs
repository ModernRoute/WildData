using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Linq.Tree.Expression
{
    public class SourceReference : QueryExpression
    {
        public SourceReference() : base(ReturnType.Null)
        {

        }

        public override QueryExpressionType ExpressionType
        {
            get
            {
                return QueryExpressionType.SourceReference;
            }
        }
    }
}
