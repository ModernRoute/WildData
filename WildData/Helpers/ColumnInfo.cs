using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    public abstract class ColumnInfo
    {
        private const string _DbParameterCollectionWrapperAddParamMethodName = "AddParam";
        private const string _DbParameterCollectionWrapperAddParamNotNullMethodName = "AddParamNotNull";

        public string ColumnName
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

        public ReturnType ReturnType
        {
            get;
            private set;
        }

        public Type MemberType
        {
            get;
            private set;
        }

        public bool VolatileOnStore
        {
            get;
            private set;
        }

        public bool VolatileOnUpdate
        {
            get;
            private set;
        }


        internal ColumnReference ColumnReference
        {
            get;
            private set;
        }

        public Expression GetAssignment(ParameterExpression readerWrapperParameter, ParameterExpression entityParameter, int columnIndex)
        {
            return Expression.MakeBinary(
                ExpressionType.Assign,
                GetSetMemberExpression(entityParameter),
                Expression.Call(
                    readerWrapperParameter,
                    ReturnType.GetMethodByReturnType(),
                    new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                ));
        }

        public abstract MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex);

        protected abstract MemberExpression GetGetMemberExpression(ParameterExpression entityParameter);

        protected abstract MemberExpression GetSetMemberExpression(ParameterExpression entityParameter);

        public MethodCallExpression GetMethodCall(ParameterExpression parametersParameter, ParameterExpression entityParameter, string paramName)
        {
            string methodName = NotNull || ReturnType.IsNotNullType() ? _DbParameterCollectionWrapperAddParamNotNullMethodName : _DbParameterCollectionWrapperAddParamMethodName;

            if (ReturnType.IsSizeableType())
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), MemberType, typeof(int) });

                return Expression.Call(parametersParameter, methodInfo, new Expression[]
                {
                        Expression.Constant(paramName, typeof(string)),
                        GetGetMemberExpression(entityParameter),
                        Expression.Constant(ColumnSize, typeof(int))
                });
            }
            else
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), MemberType });

                return Expression.Call(parametersParameter, methodInfo, new Expression[]
                {
                        Expression.Constant(paramName, typeof(string)),
                        GetGetMemberExpression(entityParameter)
                });
            }
        }

        internal ColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, bool volatileOnStore, bool volatileOnUpdate)
        {
            ColumnName = columnName;
            ColumnSize = columnSize;
            NotNull = notNull;
            ReturnType = returnType;
            MemberType = memberType;
            VolatileOnStore = volatileOnStore;
            VolatileOnUpdate = volatileOnUpdate;
            ColumnReference = new ColumnReference(ColumnName, ReturnType);
        }
    }
}
