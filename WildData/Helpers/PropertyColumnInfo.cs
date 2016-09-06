using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    class PropertyColumnInfo : ColumnInfo
    {
        public MethodInfo GetMethod
        {
            get;
            private set;
        }

        public MethodInfo SetMethod
        {
            get;
            private set;
        }

        public PropertyColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, bool volatileOnStore, bool volatileOnUpdate, MethodInfo getMethod, MethodInfo setMethod)
            : base(columnName, columnSize, notNull, returnType, memberType, volatileOnStore, volatileOnUpdate)
        {
            GetMethod = getMethod;
            SetMethod = setMethod;
        }

        internal override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex)
        {
            return Expression.Bind(
                SetMethod,
                Expression.Call(
                    readerWrapperParameter,
                    ReturnType.GetMethodByReturnType(),
                    new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                )
            );
        }

        internal override MemberExpression GetGetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Property(entityParameter, GetMethod);
        }

        internal override MemberExpression GetSetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Property(entityParameter, SetMethod);
        }

    }
}
