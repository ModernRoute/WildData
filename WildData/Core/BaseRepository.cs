using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Linq.Tree.Expression;
using ModernRoute.WildData.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Core
{
    public abstract class BaseRepository<T,TKey> where T : IReadOnlyModel<TKey>
    {
        private const string _IReaderWrapperParameterName = "reader";
        private const string _IDbParameterCollectionWrapperParameterName = "parameters";
        private const string _EntityParameterName = "entity";

        private const string _DbParameterCollectionWrapperAddParamMethodName = "AddParam";
        private const string _DbParameterCollectionWrapperAddParamNotNullMethodName = "AddParamNotNull";

        protected readonly IReadOnlyDictionary<string, ColumnDescriptor> MemberColumnMap;

        private readonly Func<IReaderWrapper, T> _ReadSingleObject;
        private readonly Action<IDbParameterCollectionWrapper, T> _SetParametersFromObject;
        protected readonly string StorageName;
 
        public BaseRepository()
        {
            Type itemType = typeof(T);

            StorageName = GetStorageName(itemType);

            IAliasGenerator aliasGenerator = new RandomAliasGenerator();

            IDictionary<string, ColumnInfo> columnInfoMap = new SortedDictionary<string, ColumnInfo>();
            CollectPropetiesInfo(itemType, columnInfoMap, aliasGenerator);
            CollectFieldInfo(itemType, columnInfoMap, aliasGenerator);

            IDictionary<string, ColumnDescriptor> memberColumnMap = new SortedDictionary<string, ColumnDescriptor>();
            IList<MemberBinding> memberAssignments = new List<MemberBinding>();
            IList<MethodCallExpression> methodCalls = new List<MethodCallExpression>();

            int columnIndex = 0;
            ParameterExpression readerWrapperParameter = Expression.Parameter(typeof(IReaderWrapper), _IReaderWrapperParameterName);

            ParameterExpression parametersParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _IDbParameterCollectionWrapperParameterName);
            ParameterExpression entityParameter = Expression.Parameter(typeof(T), _EntityParameterName);

            foreach (KeyValuePair<string, ColumnInfo> pair in columnInfoMap)
            {
                memberColumnMap.Add(pair.Key, pair.Value.GetColumnDescriptor(columnIndex));
                memberAssignments.Add(pair.Value.GetMemberAssignment(readerWrapperParameter, columnIndex));
                methodCalls.Add(pair.Value.GetMethodCall(parametersParameter, entityParameter));

                columnIndex++;
            }

            MemberColumnMap = memberColumnMap.AsReadOnly();
            _ReadSingleObject = CompileReadSingleObject(memberAssignments, readerWrapperParameter);
            _SetParametersFromObject = CompileSetParametersFromObject(methodCalls, parametersParameter, entityParameter);
        }

        private Action<IDbParameterCollectionWrapper, T> CompileSetParametersFromObject(IList<MethodCallExpression> methodCalls, ParameterExpression parametersParameter, ParameterExpression entityParameter)
        {
            return Expression.Lambda<Action<IDbParameterCollectionWrapper, T>>(Expression.Block(methodCalls), new ParameterExpression[] { parametersParameter, entityParameter }).Compile();
        }

        private static Func<IReaderWrapper, T> CompileReadSingleObject(IList<MemberBinding> memberAssignments, ParameterExpression parameterExpression)
        {
            return Expression.Lambda<Func<IReaderWrapper, T>>(Expression.MemberInit(Expression.New(typeof(T)), memberAssignments), new ParameterExpression[] { parameterExpression }).Compile();
        }

        private void CollectFieldInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap, IAliasGenerator aliasGenerator)
        {
            foreach (FieldInfo field in itemType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsIgnored(field))
                {
                    continue;
                }

                Type fieldType = field.FieldType;
                ReturnType returnType;

                if (!TryGetReturnType(fieldType, out returnType))
                {
                    continue;
                }

                bool notNull = IsNotNull(field);

                string columnName;
                int columnSize;

                GetColumnNameAndSize(field, out columnName, out columnSize);

                columnInfoMap.Add(field.Name, new FieldColumnInfo(columnName, columnSize, notNull, returnType, fieldType, GenerateAlias(field.Name, aliasGenerator), field));
            }
        }

        private static void CollectPropetiesInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap, IAliasGenerator aliasGenerator)
        {
            foreach (PropertyInfo property in itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsIgnored(property))
                {
                    continue;
                }

                MethodInfo getMethod = property.GetGetMethod();
                MethodInfo setMethod = property.GetSetMethod();

                if (getMethod == null || setMethod == null || !getMethod.IsPublic || !setMethod.IsPublic)
                {
                    continue;
                }

                Type propertyType = property.PropertyType;
                ReturnType returnType;

                if (!TryGetReturnType(propertyType, out returnType))
                {
                    continue;
                }
                
                bool notNull = IsNotNull(property);

                string columnName;
                int columnSize;

                GetColumnNameAndSize(property, out columnName, out columnSize);

                columnInfoMap.Add(property.Name, new PropertyColumnInfo(columnName, columnSize, notNull, returnType, propertyType, GenerateAlias(property.Name, aliasGenerator), getMethod, setMethod));
            }
        }

        private static string GenerateAlias(string name, IAliasGenerator aliasGenerator)
        {
            return string.Concat("@_", name, "_", aliasGenerator.GenerateAlias());
        }

        private static bool TryGetReturnType(Type type, out ReturnType returnType)
        {
            try
            {
                returnType = MapHelper.GetReturnType(type);
                return true;
            }
            catch (NotSupportedException)
            {
                returnType = ReturnType.Null;
                return false;
            }
        }

        private static void GetColumnNameAndSize(MemberInfo memberInfo, out string columnName, out int columnSize)
        {
            ColumnAttribute columnAttribute = Attribute.GetCustomAttribute(memberInfo, typeof(ColumnAttribute)) as ColumnAttribute;

            columnName = memberInfo.Name;
            columnSize = 0;

            if (columnAttribute != null)
            {
                columnName = columnAttribute.Name;
                columnSize = columnAttribute.Size;
            }
        }

        private static bool IsNotNull(MemberInfo memberInfo)
        {
            return Attribute.GetCustomAttribute(memberInfo, typeof(NotNullAttribute)) as NotNullAttribute != null;
        }

        private static bool IsIgnored(MemberInfo memberInfo)
        {
            return Attribute.GetCustomAttribute(memberInfo, typeof(IgnoreAttribute)) as IgnoreAttribute != null;
        }

        private static string GetStorageName(Type itemType)
        {
            StorageAttribute storageAtribute = Attribute.GetCustomAttribute(itemType, typeof(StorageAttribute)) as StorageAttribute;

            if (storageAtribute != null)
            {
                return storageAtribute.Name;
            }

            return itemType.Name;
        }

        protected virtual T ReadSingleObject(IReaderWrapper reader)
        {
            return _ReadSingleObject(reader);
        }

        protected virtual void SetParametersFromObject(IDbParameterCollectionWrapper parameters, T entity)
        {
            _SetParametersFromObject(parameters, entity);
        }

        #region ColumnInfo

        abstract class ColumnInfo
        {
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
                string methodName = NotNull || MapHelper.IsNotNullType(ReturnType) ? _DbParameterCollectionWrapperAddParamNotNullMethodName : _DbParameterCollectionWrapperAddParamMethodName;
                
                if (MapHelper.IsSizeableType(ReturnType))
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

            public PropertyColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, string paramName,  MethodInfo getMethod, MethodInfo setMethod)
                : base(columnName, columnSize, notNull, returnType, memberType, paramName)
            {
                GetMethod = getMethod;
                SetMethod = setMethod;
            }

            public override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex)
            {
                return Expression.Bind(
                    SetMethod, 
                    Expression.Call(
                        readerWrapperParameter, 
                        MapHelper.GetMethodByReturnType(ReturnType), 
                        new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                    )
                );
            }

            protected override MemberExpression GetMemberExpression(ParameterExpression entityParameter)
            {
                return Expression.Property(entityParameter, GetMethod);
            }
        }

        class FieldColumnInfo : ColumnInfo
        {
            public FieldInfo Field
            {
                get;
                private set;
            }

            public FieldColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, string paramName, FieldInfo field)
                : base(columnName, columnSize, notNull, returnType, memberType, paramName)
            {
                Field = field;
            }

            public override MemberAssignment GetMemberAssignment(ParameterExpression readerWrapperParameter, int columnIndex)
            {
                return Expression.Bind(
                    Field, 
                    Expression.Call(
                        readerWrapperParameter, 
                        MapHelper.GetMethodByReturnType(ReturnType), 
                        new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                    )
                );
            }

            protected override MemberExpression GetMemberExpression(ParameterExpression entityParameter)
            {
                return Expression.Field(entityParameter, Field);
            }
        }
        #endregion
    }
}
