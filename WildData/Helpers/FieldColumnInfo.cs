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

        public FieldColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, bool volatileOnStore, bool volatileOnUpdate, FieldInfo field)
            : base(columnName, columnSize, notNull, returnType, memberType, volatileOnStore, volatileOnUpdate)
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

        protected override MemberExpression GetGetMemberExpression(ParameterExpression entityParameter)
        {
            return GetMemberExpression(entityParameter);
        }

        private MemberExpression GetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Field(entityParameter, Field);
        }

        protected override MemberExpression GetSetMemberExpression(ParameterExpression entityParameter)
        {
            return GetMemberExpression(entityParameter);
        }
    }
}
