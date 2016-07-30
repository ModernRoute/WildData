using ModernRoute.WildData.Core;
using ModernRoute.WildData.Linq.Tree.Expression;

namespace ModernRoute.WildData.Linq.Tree
{
    public class Column : FieldBase
    {
        public Column(string alias, QueryExpression definition) : base(alias, definition)
        {

        }

        public override ReturnType ColumnType
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
