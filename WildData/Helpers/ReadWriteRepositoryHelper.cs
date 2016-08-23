using ModernRoute.WildData.Core;
using ModernRoute.WildData.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Helpers
{
    public class ReadWriteRepositoryHelper<T, TKey> : ReadOnlyRepositoryHelper<T> where T : IReadWriteModel<TKey>, new()
    {
        private const string _IDbParameterCollectionWrapperParameterName = "parameters";
        private const string _EntityParameterName = "entity";
        private const string _NameSeparator = "_";
        private const string _ParameterNamePrefix = "@";

        public Action<IDbParameterCollectionWrapper, T> SetParametersFromObject
        {
            get;
            private set;
        }

        public ReadWriteRepositoryHelper()
            : base()
        {
            IAliasGenerator aliasGenerator = new RandomAliasGenerator();

            IList<MethodCallExpression> methodCalls = new List<MethodCallExpression>();

            ParameterExpression parametersParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _IDbParameterCollectionWrapperParameterName);
            ParameterExpression entityParameter = Expression.Parameter(typeof(T), _EntityParameterName);

            foreach (ColumnMemberInfo columnMemberInfo in ColumnMemberInfos)
            {
                methodCalls.Add(columnMemberInfo.ColumnInfo.GetMethodCall(parametersParameter, entityParameter, GenerateAlias(columnMemberInfo.MemberName, aliasGenerator)));
            }

            SetParametersFromObject = CompileSetParametersFromObject(methodCalls, parametersParameter, entityParameter);
        }

        private static Action<IDbParameterCollectionWrapper, T> CompileSetParametersFromObject(IList<MethodCallExpression> methodCalls, ParameterExpression parametersParameter, ParameterExpression entityParameter)
        {
            return Expression.Lambda<Action<IDbParameterCollectionWrapper, T>>(Expression.Block(methodCalls), new ParameterExpression[] { parametersParameter, entityParameter }).Compile();
        }

        private static string GenerateAlias(string name, IAliasGenerator aliasGenerator)
        {
            return string.Concat(_ParameterNamePrefix, _NameSeparator, name, _NameSeparator, aliasGenerator.GenerateAlias());
        }
    }
}
