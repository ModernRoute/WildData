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

        public FieldColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, bool volatileOnStore, bool volatileOnUpdate, FieldInfo field, int columnIndex = ColumnIndexDefaultValue)
            : base(columnName, columnSize, notNull, returnType, memberType, volatileOnStore, volatileOnUpdate)
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

        internal override MemberExpression GetGetMemberExpression(ParameterExpression entityParameter)
        {
            return GetMemberExpression(entityParameter);
        }

        private MemberExpression GetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Field(entityParameter, Field);
        }

        internal override MemberExpression GetSetMemberExpression(ParameterExpression entityParameter)
        {
            return GetMemberExpression(entityParameter);
        }

        internal override ColumnInfo Clone(int columnIndex)
        {
            return new FieldColumnInfo(ColumnName, ColumnSize, NotNull, ReturnType, MemberType, VolatileOnStore, VolatileOnUpdate, Field, columnIndex);
        }
    }
}
