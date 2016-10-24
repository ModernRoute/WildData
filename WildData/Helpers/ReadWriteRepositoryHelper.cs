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
        private readonly Lazy<IReadOnlyDictionary<string, ColumnInfo>> _MemberColumnMapWithoutId;

        private IReadOnlyDictionary<string, ColumnInfo> GetVolatileOnStoreMemberColumnMap()
        {
            return MemberColumnMap.Where(kv => kv.Value.VolatileKindOnStore != VolatileKind.None).ToSortedDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal).AsReadOnly();
        }

        private IReadOnlyDictionary<string, ColumnInfo> GetVolatileOnUpdateMemberColumnMap()
        {
            return MemberColumnMap.Where(kv => kv.Value.VolatileKindOnUpdate != VolatileKind.None).ToSortedDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal).AsReadOnly();
        }

        private IReadOnlyDictionary<string, ColumnInfo> GetMemberColumnMapWithoutId()
        {
            return MemberColumnMap.Where(kv => !string.Equals(kv.Key, nameof(IReadOnlyModel<TKey>.Id))).ToSortedDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal).AsReadOnly();
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

        public IReadOnlyDictionary<string, ColumnInfo> MemberColumnMapWithoutId
        {
            get
            {
                return _MemberColumnMapWithoutId.Value;
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

        public Action<IDbParameterCollectionWrapper, T> SetParametersFromObjectForUpdate
        {
            get;
            private set;
        }

        public Action<IDbParameterCollectionWrapper, T> SetParametersFromObjectForStore
        {
            get;
            private set;
        }

        public ReadWriteRepositoryHelper()
            : base()
        {
            IList<MethodCallExpression> methodCallsForUpdate = new List<MethodCallExpression>();
            IList<MethodCallExpression> methodCallsForStore = new List<MethodCallExpression>();
            IList<Expression> volatileOnUpdateExpression = new List<Expression>();
            IList<Expression> volatileOnStoreExpression = new List<Expression>();

            ParameterExpression parametersParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _IDbParameterCollectionWrapperParameterName);
            ParameterExpression entityParameter = Expression.Parameter(typeof(T), _EntityParameterName);
            ParameterExpression readerWrapperParameter = Expression.Parameter(typeof(IReaderWrapper), _ReaderWrapperParameterName);

            int volatileOnUpdateColumnIndex = 0;
            int volatileOnStoreColumnIndex = 0;

            foreach (KeyValuePair<string, ColumnInfo> memberColumnInfo in MemberColumnMap)
            {
                MethodCallExpression methodCall = memberColumnInfo.Value.GetMethodCall(parametersParameter, entityParameter);
                
                if (memberColumnInfo.Value.VolatileKindOnStore != VolatileKind.Regular)
                {
                    methodCallsForStore.Add(methodCall);
                }

                if (memberColumnInfo.Value.VolatileKindOnUpdate != VolatileKind.Regular || string.Equals(memberColumnInfo.Key,nameof(IReadOnlyModel<TKey>.Id), StringComparison.Ordinal))
                {
                    methodCallsForUpdate.Add(methodCall);
                }

                if (memberColumnInfo.Value.VolatileKindOnStore != VolatileKind.None)
                {
                    volatileOnStoreExpression.Add(memberColumnInfo.Value.GetAssignment(readerWrapperParameter, entityParameter, volatileOnStoreColumnIndex++));
                }

                if (memberColumnInfo.Value.VolatileKindOnUpdate != VolatileKind.None)
                {
                    volatileOnUpdateExpression.Add(memberColumnInfo.Value.GetAssignment(readerWrapperParameter, entityParameter, volatileOnUpdateColumnIndex++));
                }
            }

            UpdateVolatileColumnsOnStore = CompileUpdateVolatileColumns(volatileOnStoreExpression, readerWrapperParameter, entityParameter);
            UpdateVolatileColumnsOnUpdate = CompileUpdateVolatileColumns(volatileOnUpdateExpression, readerWrapperParameter, entityParameter);

            SetParametersFromObjectForStore = CompileSetParametersFromObject(methodCallsForStore, parametersParameter, entityParameter);
            SetParametersFromObjectForUpdate = CompileSetParametersFromObject(methodCallsForUpdate, parametersParameter, entityParameter);

            _VolatileOnStoreMemberColumnMap = new Lazy<IReadOnlyDictionary<string, ColumnInfo>>(GetVolatileOnStoreMemberColumnMap);
            _VolatileOnUpdateMemberColumnMap = new Lazy<IReadOnlyDictionary<string, ColumnInfo>>(GetVolatileOnUpdateMemberColumnMap);

            _MemberColumnMapWithoutId = new Lazy<IReadOnlyDictionary<string, ColumnInfo>>(GetMemberColumnMapWithoutId);
        }

        private static Action<IReaderWrapper, T> CompileUpdateVolatileColumns(IList<Expression> expressions, ParameterExpression readerWrapperParameter, ParameterExpression entityParameter)
        {
            BlockExpression blockExpression;

            if (expressions.Count > 0)
            {
                blockExpression = Expression.Block(GetCheckParameterNullExpression(readerWrapperParameter), GetCheckParameterNullExpression(entityParameter), Expression.Block(expressions));
                
            }
            else
            {
                blockExpression = Expression.Block(GetCheckParameterNullExpression(readerWrapperParameter), GetCheckParameterNullExpression(entityParameter));
            }

            return Expression.Lambda<Action<IReaderWrapper, T>>(blockExpression, new ParameterExpression[] { readerWrapperParameter, entityParameter }).Compile();
        }

        private static Action<IDbParameterCollectionWrapper, T> CompileSetParametersFromObject(IList<MethodCallExpression> methodCalls, ParameterExpression parametersParameter, ParameterExpression entityParameter)
        {
            BlockExpression blockExpression;

            if (methodCalls.Count > 0)
            {
                blockExpression = Expression.Block(GetCheckParameterNullExpression(parametersParameter), GetCheckParameterNullExpression(entityParameter), Expression.Block(methodCalls));                                
            }
            else
            {
                blockExpression = Expression.Block(GetCheckParameterNullExpression(parametersParameter), GetCheckParameterNullExpression(entityParameter));
            }

            return Expression.Lambda<Action<IDbParameterCollectionWrapper, T>>(blockExpression, new ParameterExpression[] { parametersParameter, entityParameter }).Compile();
        }
    }
}
