using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Linq.Tree.Expression;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Core
{
    abstract class ColumnInfo
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

        public string ParamName
        {
            get;
            private set;
        }

        public ColumnDescriptor GetColumnDescriptor(int columnIndex)
        {
            return new ColumnDescriptor(columnIndex, new ColumnReference(ColumnName, ReturnType));
        }

        public abstract MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex);

        protected abstract MemberExpression GetMemberExpression(ParameterExpression entityParameter);

        public MethodCallExpression GetMethodCall(ParameterExpression parametersParameter, ParameterExpression entityParameter)
        {
            string methodName = NotNull || ReturnType.IsNotNullType() ? _DbParameterCollectionWrapperAddParamNotNullMethodName : _DbParameterCollectionWrapperAddParamMethodName;

            if (ReturnType.IsSizeableType())
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), MemberType, typeof(int) });

                return Expression.Call(parametersParameter, methodInfo, new Expression[]
                {
                        Expression.Constant(ParamName, typeof(string)),
                        GetMemberExpression(entityParameter),
                        Expression.Constant(ColumnSize, typeof(int))
                });
            }
            else
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), MemberType });

                return Expression.Call(parametersParameter, methodInfo, new Expression[]
                {
                        Expression.Constant(ParamName, typeof(string)),
                        GetMemberExpression(entityParameter)
                });
            }
        }

        public ColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, string paramName)
        {
            ColumnName = columnName;
            ColumnSize = columnSize;
            NotNull = notNull;
            ReturnType = returnType;
            MemberType = memberType;
            ParamName = paramName;
        }
    }
}
