using ModernRoute.WildData.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Core
{
    class FieldColumnInfo : ColumnInfo
    {
        public FieldInfo Field
        {
            get;
            private set;
        }

        public FieldColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, string paramName, FieldInfo field)
            : base(columnName, columnSize, notNull, returnType, memberType, paramName)
        {
            Field = field;
        }

        public override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex)
        {
            return Expression.Bind(
                Field,
                Expression.Call(
                    readerWrapperParameter,
                    ReturnType.GetMethodByReturnType(),
                    new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                )
            );
        }

        protected override MemberExpression GetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Field(entityParameter, Field);
        }
    }
}
