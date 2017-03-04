using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Linq
{
    public class SourceReference : QueryExpression
    {
        public SourceReference() : base(TypeKind.Null)
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
