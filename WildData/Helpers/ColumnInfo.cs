using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq;
using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    public abstract class ColumnInfo
    {
        internal const int ColumnIndexDefaultValue = 0;
        public const string ParameterNameBasePrefix = "@__p_";

        public string ColumnName
        {
            get;
            private set;
        }

        public int ColumnIndex
        {
            get;
            private set;
        }

        public int ColumnSize
        {
            get;
            private set;
        }

        public bool NotNull
        {
            get;
            private set;
        }

        public TypeKind TypeKind
        {
            get;
            private set;
        }

        public Type MemberType
        {
            get;
            private set;
        }

        public VolatileKind VolatileKindOnStore
        {
            get;
            private set;
        }

        public VolatileKind VolatileKindOnUpdate
        {
            get;
            private set;
        }

        public string ParamNameBase
        {
            get
            {
                return string.Concat(ParameterNameBasePrefix, ColumnIndex.ToString(CultureInfo.InvariantCulture));
            }
        }

        internal ColumnReference ColumnReference
        {
            get;
            private set;
        }

        internal Expression GetAssignment(ParameterExpression readerWrapperParameter, ParameterExpression entityParameter, int columnIndex)
        {
            return Expression.MakeBinary(
                ExpressionType.Assign,
                GetMemberExpression(entityParameter),
                Expression.Call(
                    readerWrapperParameter,
                    TypeKind.GetMethodByTypeKind(),
                    new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                ));
        }

        internal abstract MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter);

        internal abstract MemberExpression GetMemberExpression(ParameterExpression entityParameter);

        internal abstract ColumnInfo Clone(int columnIndex);

        internal MethodCallExpression GetMethodCall(ParameterExpression parametersParameter, ParameterExpression entityParameter)
        {
            string methodName = NotNull || TypeKind.IsNotNullType() ? nameof(IDbParameterCollectionWrapper.AddParamNotNullBase) : nameof(IDbParameterCollectionWrapper.AddParamBase);

            if (TypeKind.IsSizeableType())
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), MemberType, typeof(int) });

                return Expression.Call(parametersParameter, methodInfo, new Expression[]
                {
                        Expression.Constant(ParamNameBase, typeof(string)),
                        GetMemberExpression(entityParameter),
                        Expression.Constant(ColumnSize, typeof(int))
                });
            }
            else
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), MemberType });

                return Expression.Call(parametersParameter, methodInfo, new Expression[]
                {
                        Expression.Constant(ParamNameBase, typeof(string)),
                        GetMemberExpression(entityParameter)
                });
            }
        }

        internal ColumnInfo(string columnName, int columnSize, bool notNull, TypeKind typeKind, Type memberType, VolatileKind volatileKindOnStore, VolatileKind volatileKindOnUpdate, int columnIndex = ColumnIndexDefaultValue)
        {
            ColumnName = columnName;
            ColumnSize = columnSize;
            NotNull = notNull;
            TypeKind = typeKind;
            MemberType = memberType;
            VolatileKindOnStore = volatileKindOnStore;
            VolatileKindOnUpdate = volatileKindOnUpdate;
            ColumnIndex = columnIndex;
            ColumnReference = new ColumnReference(ColumnName, TypeKind);
        }
    }    
}
