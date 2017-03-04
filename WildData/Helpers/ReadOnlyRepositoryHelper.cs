using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Models;
using ModernRoute.WildData.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    public class ReadOnlyRepositoryHelper<T> where T : IReadOnlyModel, new()
    {
        private const string _IReaderWrapperParameterName = "reader";

        public IReadOnlyDictionary<string, ColumnInfo> MemberColumnMap
        {
            get;
            private set;
        }

        public Func<IReaderWrapper, T> ReadSingleObject
        {
            get;
            internal set;
        }

        public string StorageName
        {
            get;
            private set;           
        }

        public string StorageSchema
        {
            get;
            private set;
        }

        private static IReadOnlyDictionary<string, ColumnInfo> GetMemberColumnMap()
        {
            Type itemType = typeof(T);

            IDictionary<string, ColumnInfo> columnInfoMap = new SortedDictionary<string, ColumnInfo>(StringComparer.Ordinal);

            CollectPropetiesInfo(itemType, columnInfoMap);
            CollectFieldInfo(itemType, columnInfoMap);

            if (columnInfoMap.Count <= 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Strings.NoColumnsFoundForType, itemType));
            }

            int columnIndex = 0;

            IDictionary<string, ColumnInfo> result = new SortedDictionary<string, ColumnInfo>(StringComparer.Ordinal);
            
            foreach (KeyValuePair<string, ColumnInfo> memberColumnInfo in columnInfoMap)
            {
                result.Add(memberColumnInfo.Key, memberColumnInfo.Value.Clone(columnIndex++));
            }

            return result.AsReadOnly();
        }

        public ReadOnlyRepositoryHelper(ITypeKindInfo typeKindInfo)
        {
            if (typeKindInfo == null)
            {
                throw new ArgumentNullException(nameof(typeKindInfo));
            }
            
            Type itemType = typeof(T);

            StorageAttribute storageAttribute = Attribute.GetCustomAttribute(itemType, typeof(StorageAttribute)) as StorageAttribute;

            if (storageAttribute != null)
            {
                StorageName = storageAttribute.Name;
                StorageSchema = storageAttribute.Schema;
            }
            else
            {
                StorageName = itemType.Name;
            }

            MemberColumnMap = GetMemberColumnMap();
            
            IList<MemberBinding> memberAssignments = new List<MemberBinding>();

            ParameterExpression readerWrapperParameter = Expression.Parameter(typeof(IReaderWrapper), _IReaderWrapperParameterName);

            foreach (KeyValuePair<string,ColumnInfo> columnMemberInfo in MemberColumnMap)
            {
                if (!typeKindInfo.IsSupported(columnMemberInfo.Value.TypeKind))
                {
                    throw new TypeKindNotSupportedException(columnMemberInfo.Value.TypeKind);
                }

                memberAssignments.Add(columnMemberInfo.Value.GetMemberAssignment(readerWrapperParameter));
            }

            ReadSingleObject = CompileReadSingleObject(memberAssignments, readerWrapperParameter);
        }

        private static Func<IReaderWrapper, T> CompileReadSingleObject(IEnumerable<MemberBinding> memberAssignments, ParameterExpression parameterExpression)
        {
            BlockExpression blockExpression = Expression.Block(GetCheckParameterNullExpression(parameterExpression), Expression.MemberInit(Expression.New(typeof(T)), memberAssignments));
            return Expression.Lambda<Func<IReaderWrapper, T>>(blockExpression, new ParameterExpression[] { parameterExpression }).Compile();
        }

        protected static Expression GetCheckParameterNullExpression(ParameterExpression parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            ConditionalExpression ifThenExpression =
                Expression.IfThen(
                    Expression.Equal(parameter, Expression.Constant(null)),
                    Expression.Throw(
                            Expression.New(
                                typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) }),
                                Expression.Constant(parameter.Name)
                            )
                    )
                );

            return ifThenExpression;
        }

        private static void CollectFieldInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap)
        {
            foreach (FieldInfo field in itemType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsIgnored(field))
                {
                    continue;
                }

                Type fieldType = field.FieldType;
                TypeKind typeKind;

                if (!TryGetTypeKind(fieldType, out typeKind))
                {
                    continue;
                }

                bool notNull = IsNotNull(field);
                VolatileKind volatileKindOnStore = GetVolatileKindOnStore(field);
                VolatileKind volatileKindOnUpdate = GetVolatileKindOnUpdate(field);

                string columnName;
                int columnSize;

                GetColumnNameAndSize(field, out columnName, out columnSize);

                columnInfoMap.Add(field.Name, new FieldColumnInfo(columnName, columnSize, notNull, typeKind, fieldType, volatileKindOnStore, volatileKindOnUpdate, field));
            }
        }

        private static void CollectPropetiesInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap)
        {
            foreach (PropertyInfo property in itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsIgnored(property) || !property.CanRead || !property.CanWrite)
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
                TypeKind typeKind;

                if (!TryGetTypeKind(propertyType, out typeKind))
                {
                    continue;
                }
                
                bool notNull = IsNotNull(property);
                VolatileKind volatileKindOnStore = GetVolatileKindOnStore(property);
                VolatileKind volatileKindOnUpdate = GetVolatileKindOnUpdate(property);

                string columnName;
                int columnSize;

                GetColumnNameAndSize(property, out columnName, out columnSize);

                columnInfoMap.Add(property.Name, new PropertyColumnInfo(columnName, columnSize, notNull, typeKind, propertyType, volatileKindOnStore, volatileKindOnUpdate, property));
            }
        }

        private static bool TryGetTypeKind(Type type, out TypeKind typeKind)
        {
            try
            {
                typeKind = type.GetTypeKind();
                return true;
            }
            catch (NotSupportedException)
            {
                typeKind = TypeKind.Null;
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
            return IsCustomAttribute<NotNullAttribute>(memberInfo);
           }

        private static bool IsIgnored(MemberInfo memberInfo)
        {
            return IsCustomAttribute<IgnoreAttribute>(memberInfo);
        }

        private static VolatileKind GetVolatileKindOnUpdate(MemberInfo memberInfo)
        {
            return GetVolatileAttribute<VolatileOnUpdate>(memberInfo);
        }

        private static VolatileKind GetVolatileKindOnStore(MemberInfo memberInfo)
        {
            return GetVolatileAttribute<VolatileOnStore>(memberInfo);
        }

        private static VolatileKind GetVolatileAttribute<A>(MemberInfo memberInfo) where A : BaseVolatileAttribute
        {
            A volatileAttribute = Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A;

            if (volatileAttribute == null)
            {
                return VolatileKind.None;
            }

            return volatileAttribute.ForcePush ? VolatileKind.ForcePush : VolatileKind.Regular;
        }

        private static bool IsCustomAttribute<A>(MemberInfo memberInfo) where A : Attribute
        {
            return Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A != null;
        }
    }

    public class ReadOnlyRepositoryHelper<T, TKey> : ReadOnlyRepositoryHelper<T> where T : IReadOnlyModel<TKey>, new()
    {
        private const string _CollectionParameterName = "collectionWrapper";
        private const string _ValueParameterName = "value";

        public ReadOnlyRepositoryHelper(ITypeKindInfo typeKindInfo)
            : base(typeKindInfo)
        {
            if (!MemberColumnMap.ContainsKey(nameof(IReadOnlyModel<TKey>.Id)))
            {
                throw new InvalidOperationException(string.Format(Strings.TheModelIgnoresProperty, nameof(IReadOnlyModel<TKey>.Id)));
            }

            InitAddIdParameter();
        }

        private void InitAddIdParameter()
        {
            ParameterExpression collectionParameter = Expression.Parameter(typeof(IDbParameterCollectionWrapper), _CollectionParameterName);
            ParameterExpression valueParameter = Expression.Parameter(typeof(TKey), _ValueParameterName);

            ColumnInfo columnInfo = MemberColumnMap[nameof(IReadOnlyModel<TKey>.Id)];

            string methodName = columnInfo.NotNull || columnInfo.TypeKind.IsNotNullType() ? nameof(IDbParameterCollectionWrapper.AddParamNotNull) : nameof(IDbParameterCollectionWrapper.AddParam);

            MethodCallExpression methodCallExpression;

            if (columnInfo.TypeKind.IsSizeableType())
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), columnInfo.MemberType, typeof(int) });

                methodCallExpression = Expression.Call(collectionParameter, methodInfo, new Expression[]
                {
                    Expression.Constant(columnInfo.ParamNameBase, typeof(string)),
                    valueParameter,
                    Expression.Constant(columnInfo.ColumnSize, typeof(int))
                });
            }
            else
            {
                MethodInfo methodInfo = typeof(IDbParameterCollectionWrapper).GetMethod(methodName, new Type[] { typeof(string), columnInfo.MemberType });

                methodCallExpression = Expression.Call(collectionParameter, methodInfo, new Expression[]
                {
                    Expression.Constant(columnInfo.ParamNameBase, typeof(string)),
                    valueParameter
                });
            }

            BlockExpression blockExpression = Expression.Block(GetCheckParameterNullExpression(collectionParameter), methodCallExpression);

            AddIdParameter = Expression.Lambda<Action<IDbParameterCollectionWrapper, TKey>>(blockExpression, new ParameterExpression[] { collectionParameter, valueParameter }).Compile();
        }

        public string GetIdParamName()
        {
            return MemberColumnMap[nameof(IReadOnlyModel<TKey>.Id)].ParamNameBase;            
        }

        public Action<IDbParameterCollectionWrapper, TKey> AddIdParameter
        {
            get;
            private set;
        }
    }
}
