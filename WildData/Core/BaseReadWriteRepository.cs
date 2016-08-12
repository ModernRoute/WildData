using ModernRoute.WildData.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Core
{
    public abstract class BaseReadWriteRepository<T, TKey> : BaseReadOnlyRepository<T, TKey> where T : IReadWriteModel<TKey>, new()
    {
        private const string _IDbParameterCollectionWrapperParameterName = "parameters";
        private const string _EntityParameterName = "entity";

        public Action<IDbParameterCollectionWrapper, T> SetParametersFromObject
        {
            get;
            protected set;
        }

        public BaseReadWriteRepository()
            : base()
        {
            IList<MethodCallExpression> methodCalls = new List<MethodCallExpression>();

            ParameterExpression parametersParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _IDbParameterCollectionWrapperParameterName);
            ParameterExpression entityParameter = Expression.Parameter(typeof(T), _EntityParameterName);

            foreach (ColumnMemberInfo columnMemberInfo in ColumnMemberInfos)
            {
                methodCalls.Add(columnMemberInfo.ColumnInfo.GetMethodCall(parametersParameter, entityParameter));
            }

            SetParametersFromObject = CompileSetParametersFromObject(methodCalls, parametersParameter, entityParameter);
        }

        private Action<IDbParameterCollectionWrapper, T> CompileSetParametersFromObject(IList<MethodCallExpression> methodCalls, ParameterExpression parametersParameter, ParameterExpression entityParameter)
        {
            return Expression.Lambda<Action<IDbParameterCollectionWrapper, T>>(Expression.Block(methodCalls), new ParameterExpression[] { parametersParameter, entityParameter }).Compile();
        }
    }
}
