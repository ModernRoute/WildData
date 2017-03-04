using ModernRoute.WildData.Core;

namespace ModernRoute.WildData.Linq
{
    public class Column : FieldBase
    {
        public Column(string alias, QueryExpression definition) : base(alias, definition)
        {

        }

        public override TypeKind ColumnType
        {
            get 
            {
                return Definition.Type; 
            }
        }

        public override QueryElementType ElementType
        {
            get 
            {
                return QueryElementType.Column;
            }
        }
    }
}
