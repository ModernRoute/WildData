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

        public PropertyColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, MethodInfo getMethod, MethodInfo setMethod)
            : base(columnName, columnSize, notNull, returnType, memberType)
        {
            GetMethod = getMethod;
            SetMethod = setMethod;
        }

        public override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex)
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

        protected override MemberExpression GetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Property(entityParameter, GetMethod);
        }
    }
}
