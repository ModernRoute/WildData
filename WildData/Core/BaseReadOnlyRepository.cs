using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Core
{
    public abstract class BaseReadOnlyRepository<T,TKey> where T : IReadOnlyModel<TKey>, new()
    {
        private const string _IReaderWrapperParameterName = "reader";

        protected readonly IReadOnlyDictionary<string, ColumnDescriptor> MemberColumnMap;
        private readonly Lazy<IEnumerable<ColumnMemberInfo>> _ColumnMemberInfos = new Lazy<IEnumerable<ColumnMemberInfo>>(PopulateColumnMemberInfos);

        public Func<IReaderWrapper, T> ReadSingleObject
        {
            get;
            protected set;
        }

        public string StorageName
        {
            get;
            private set;           
        }

        private static IEnumerable<ColumnMemberInfo> PopulateColumnMemberInfos()
        {
            Type itemType = typeof(T);

            IAliasGenerator aliasGenerator = new RandomAliasGenerator();

            IDictionary<string, ColumnInfo> columnInfoMap = new SortedDictionary<string, ColumnInfo>();

            CollectPropetiesInfo(itemType, columnInfoMap, aliasGenerator);
            CollectFieldInfo(itemType, columnInfoMap, aliasGenerator);

            return columnInfoMap.Select((c, i) => new ColumnMemberInfo(c.Key, i, c.Value)).ToList();
        }

        internal IEnumerable<ColumnMemberInfo> ColumnMemberInfos
        {
            get
            {
                return _ColumnMemberInfos.Value;
            }
        }
 
        public BaseReadOnlyRepository()
        {
            Type itemType = typeof(T);

            StorageName = GetStorageName(itemType);
            
            IDictionary<string, ColumnDescriptor> memberColumnMap = new SortedDictionary<string, ColumnDescriptor>();
            IList<MemberBinding> memberAssignments = new List<MemberBinding>();

            ParameterExpression readerWrapperParameter = Expression.Parameter(typeof(IReaderWrapper), _IReaderWrapperParameterName);

            foreach (ColumnMemberInfo columnMemberInfo in ColumnMemberInfos)
            {
                memberColumnMap.Add(columnMemberInfo.MemberName, columnMemberInfo.ColumnInfo.GetColumnDescriptor(columnMemberInfo.ColumnIndex));
                memberAssignments.Add(columnMemberInfo.ColumnInfo.GetMemberAssignment(readerWrapperParameter, columnMemberInfo.ColumnIndex));
            }

            MemberColumnMap = memberColumnMap.AsReadOnly();
            ReadSingleObject = CompileReadSingleObject(memberAssignments, readerWrapperParameter);
        }

        private static Func<IReaderWrapper, T> CompileReadSingleObject(IList<MemberBinding> memberAssignments, ParameterExpression parameterExpression)
        {
            return Expression.Lambda<Func<IReaderWrapper, T>>(Expression.MemberInit(Expression.New(typeof(T)), memberAssignments), new ParameterExpression[] { parameterExpression }).Compile();
        }

        private static void CollectFieldInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap, IAliasGenerator aliasGenerator)
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
                returnType = ReturnTypeExtensions.GetReturnType(type);
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
    }
}
