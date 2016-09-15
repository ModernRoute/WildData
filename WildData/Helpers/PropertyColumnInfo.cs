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

        public PropertyColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, bool volatileOnStore, bool volatileOnUpdate, MethodInfo getMethod, MethodInfo setMethod, int columnIndex = ColumnIndexDefaultValue)
            : base(columnName, columnSize, notNull, returnType, memberType, volatileOnStore, volatileOnUpdate, columnIndex)
        {
            GetMethod = getMethod;
            SetMethod = setMethod;
        }

        internal override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter)
        {
            return Expression.Bind(
                SetMethod,
                Expression.Call(
                    readerWrapperParameter,
                    ReturnType.GetMethodByReturnType(),
                    new Expression[] { Expression.Constant(ColumnIndex, typeof(int)) }
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

        internal override ColumnInfo Clone(int columnIndex)
        {
            return new PropertyColumnInfo(ColumnName, ColumnSize, NotNull, ReturnType, MemberType, VolatileOnStore, VolatileOnUpdate, GetMethod, SetMethod, columnIndex);
        }
    }
}
