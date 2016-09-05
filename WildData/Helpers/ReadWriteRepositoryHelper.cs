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
        private const string _ReaderWrapperParameterName = "reader";
        private const string _NameSeparator = "_";
        private const string _ParameterNamePrefix = "@";

        public Action<IReaderWrapper, T> UpdateVolatileColumnsOnStore
        {
            get;
            private set;
        }
        
        public Action<IReaderWrapper, T> UpdateVolatileColumnsOnUpdate
        {
            get;
            private set;
        }

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
            IList<Expression> volatileOnUpdateExpression = new List<Expression>();
            IList<Expression> volatileOnStoreExpression = new List<Expression>();

            ParameterExpression parametersParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _IDbParameterCollectionWrapperParameterName);
            ParameterExpression entityParameter = Expression.Parameter(typeof(T), _EntityParameterName);
            ParameterExpression readerWrapperParameter = Expression.Parameter(typeof(T), _ReaderWrapperParameterName);

            int volatileOnUpdateColumnIndex = 0;
            int volatileOnStoreColumnIndex = 0;

            foreach (ColumnMemberInfo columnMemberInfo in ColumnMemberInfos)
            {
                methodCalls.Add(columnMemberInfo.ColumnInfo.GetMethodCall(parametersParameter, entityParameter, GenerateAlias(columnMemberInfo.MemberName, aliasGenerator)));

                if (columnMemberInfo.ColumnInfo.VolatileOnStore)
                {
                    volatileOnStoreExpression.Add(columnMemberInfo.ColumnInfo.GetAssignment(readerWrapperParameter, entityParameter, volatileOnStoreColumnIndex++));
                }

                if (columnMemberInfo.ColumnInfo.VolatileOnUpdate)
                {
                    volatileOnUpdateExpression.Add(columnMemberInfo.ColumnInfo.GetAssignment(readerWrapperParameter, entityParameter, volatileOnUpdateColumnIndex++));
                }
            }

            UpdateVolatileColumnsOnStore = CompileUpdateVolatileColumns(volatileOnStoreExpression, readerWrapperParameter, entityParameter);
            UpdateVolatileColumnsOnUpdate = CompileUpdateVolatileColumns(volatileOnUpdateExpression, readerWrapperParameter, entityParameter);
            SetParametersFromObject = CompileSetParametersFromObject(methodCalls, parametersParameter, entityParameter);
        }

        private static Action<IReaderWrapper, T> CompileUpdateVolatileColumns(IList<Expression> expressions, ParameterExpression readerWrapperParameter, ParameterExpression entityParameter)
        {
            if (expressions.Count > 0)
            {
                return Expression.Lambda<Action<IReaderWrapper, T>>(Expression.Block(expressions), new ParameterExpression[] { readerWrapperParameter, entityParameter }).Compile();
            }

            return null;
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
