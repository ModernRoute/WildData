using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    class FieldColumnInfo : ColumnInfo
    {
        public FieldInfo Field
        {
            get;
            private set;
        }

        public FieldColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, VolatileKind volatileKindOnStore, VolatileKind volatileKindOnUpdate, FieldInfo field, int columnIndex = ColumnIndexDefaultValue)
            : base(columnName, columnSize, notNull, returnType, memberType, volatileKindOnStore, volatileKindOnUpdate)
        {
            Field = field;
        }

        internal override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter)
        {
            return Expression.Bind(
                Field,
                Expression.Call(
                    readerWrapperParameter,
                    ReturnType.GetMethodByReturnType(),
                    new Expression[] { Expression.Constant(ColumnIndex, typeof(int)) }
                )
            );
        }

        internal override MemberExpression GetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Field(entityParameter, Field);
        }

        internal override ColumnInfo Clone(int columnIndex)
        {
            return new FieldColumnInfo(ColumnName, ColumnSize, NotNull, ReturnType, MemberType, VolatileKindOnStore, VolatileKindOnUpdate, Field, columnIndex);
        }
    }
}
