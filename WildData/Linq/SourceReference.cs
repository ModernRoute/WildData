using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Linq
{
    public sealed class SourceReference : QueryExpression
    {
        internal SourceReference() : base(TypeKind.AnyNullable)
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
