using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Linq
{
    public class ColumnReference : QueryExpression
    {
        public string ColumnName
        {
            get;
            private set;
        }

        public ColumnReference(string columnName, ReturnType returnType)
            : base(returnType)
        {
            if (columnName == null)
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            ColumnName = columnName;
        }

        public override QueryExpressionType ExpressionType
        {
            get 
            {
                return QueryExpressionType.ColumnReference;
            }
        }
    }
}
