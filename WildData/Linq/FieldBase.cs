using ModernRoute.WildData.Core;
using System;

namespace ModernRoute.WildData.Linq
{
    public abstract class FieldBase : QueryElementBase
    {
        public string Alias
        {
            get;
            private set;
        }        
        
        public QueryExpression Definition
        {
            get;
            private set;
        }

        public abstract TypeKind ColumnType
        {
            get;
        }

        public FieldBase(string alias, QueryExpression definition)
        {
            if (alias == null)
            {
                throw new ArgumentNullException(nameof(alias));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Alias = alias;
            Definition = definition;
        }

        internal ColumnReference GetColumnReference()
        {
            return new ColumnReference(Alias, ColumnType);
        }
    }
}
