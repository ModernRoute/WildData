﻿using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    class PropertyColumnInfo : ColumnInfo
    {
        public PropertyInfo PropertyInfo
        {
            get;
            private set;
        }

        public PropertyColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, VolatileKind volatileKindOnStore, VolatileKind volatileKindOnUpdate, PropertyInfo propertyInfo, int columnIndex = ColumnIndexDefaultValue)
            : base(columnName, columnSize, notNull, returnType, memberType, volatileKindOnStore, volatileKindOnUpdate, columnIndex)
        {
            PropertyInfo = propertyInfo;
        }

        internal override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter)
        {
            return Expression.Bind(
                PropertyInfo,
                Expression.Call(
                    readerWrapperParameter,
                    ReturnType.GetMethodByReturnType(),
                    new Expression[] { Expression.Constant(ColumnIndex, typeof(int)) }
                )
            );
        }

        internal override MemberExpression GetMemberExpression(ParameterExpression entityParameter)
        {
            return Expression.Property(entityParameter, PropertyInfo);
        }

        internal override ColumnInfo Clone(int columnIndex)
        {
            return new PropertyColumnInfo(ColumnName, ColumnSize, NotNull, ReturnType, MemberType, VolatileKindOnStore, VolatileKindOnUpdate, PropertyInfo, columnIndex);
        }
    }
}
