using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Helpers
{
    public class ReadWriteRepositoryHelper<T, TKey> : ReadOnlyRepositoryHelper<T, TKey> where T : IReadWriteModel<TKey>, new()
    {
        private const string _IDbParameterCollectionWrapperParameterName = "parameters";
        private const string _EntityParameterName = "entity";
        private const string _ReaderWrapperParameterName = "reader";

        private readonly Lazy<IReadOnlyDictionary<string, ColumnInfo>> _VolatileOnStoreMemberColumnMap;
        private readonly Lazy<IReadOnlyDictionary<string, ColumnInfo>> _VolatileOnUpdateMemberColumnMap;

        private IReadOnlyDictionary<string, ColumnInfo> GetVolatileOnStoreMemberColumnMap()
        {
            return MemberColumnMap.Where(kv => kv.Value.VolatileOnStore).ToDictionary(kv => kv.Key, kv => kv.Value).AsReadOnly();
        }

        private IReadOnlyDictionary<string, ColumnInfo> GetVolatileOnUpdateMemberColumnMap()
        {
            return MemberColumnMap.Where(kv => kv.Value.VolatileOnUpdate).ToDictionary(kv => kv.Key, kv => kv.Value).AsReadOnly();
        }

        public IReadOnlyDictionary<string, ColumnInfo> VolatileOnStoreMemberColumnMap
        {
            get
            {
                return _VolatileOnStoreMemberColumnMap.Value;
            }
        }

        public IReadOnlyDictionary<string, ColumnInfo> VolatileOnUpdateMemberColumnMap
        {
            get
            {
                return _VolatileOnUpdateMemberColumnMap.Value;
            }
        }

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
            IList<MethodCallExpression> methodCalls = new List<MethodCallExpression>();
            IList<Expression> volatileOnUpdateExpression = new List<Expression>();
            IList<Expression> volatileOnStoreExpression = new List<Expression>();

            ParameterExpression parametersParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _IDbParameterCollectionWrapperParameterName);
            ParameterExpression entityParameter = Expression.Parameter(typeof(T), _EntityParameterName);
            ParameterExpression readerWrapperParameter = Expression.Parameter(typeof(IReaderWrapper), _ReaderWrapperParameterName);

            int volatileOnUpdateColumnIndex = 0;
            int volatileOnStoreColumnIndex = 0;

            foreach (KeyValuePair<string, ColumnInfo> memberColumnInfo in MemberColumnMap)
            {
                methodCalls.Add(memberColumnInfo.Value.GetMethodCall(parametersParameter, entityParameter));

                if (memberColumnInfo.Value.VolatileOnStore)
                {
                    volatileOnStoreExpression.Add(memberColumnInfo.Value.GetAssignment(readerWrapperParameter, entityParameter, volatileOnStoreColumnIndex++));
                }

                if (memberColumnInfo.Value.VolatileOnUpdate)
                {
                    volatileOnUpdateExpression.Add(memberColumnInfo.Value.GetAssignment(readerWrapperParameter, entityParameter, volatileOnUpdateColumnIndex++));
                }
            }

            UpdateVolatileColumnsOnStore = CompileUpdateVolatileColumns(volatileOnStoreExpression, readerWrapperParameter, entityParameter);
            UpdateVolatileColumnsOnUpdate = CompileUpdateVolatileColumns(volatileOnUpdateExpression, readerWrapperParameter, entityParameter);
            SetParametersFromObject = CompileSetParametersFromObject(methodCalls, parametersParameter, entityParameter);

            _VolatileOnStoreMemberColumnMap = new Lazy<IReadOnlyDictionary<string, ColumnInfo>>(GetVolatileOnStoreMemberColumnMap);
            _VolatileOnUpdateMemberColumnMap = new Lazy<IReadOnlyDictionary<string, ColumnInfo>>(GetVolatileOnUpdateMemberColumnMap);            
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
    }
}
