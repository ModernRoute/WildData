using ModernRoute.WildData.Attributes;
using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Helpers
{
    public class ReadOnlyRepositoryHelper<T> where T : IReadOnlyModel, new()
    {
        private const string _IReaderWrapperParameterName = "reader";
        
        private readonly Lazy<IEnumerable<ColumnMemberInfo>> _ColumnMemberInfos = new Lazy<IEnumerable<ColumnMemberInfo>>(PopulateColumnMemberInfos);

        public IReadOnlyDictionary<string, ColumnDescriptor> MemberColumnMap
        {
            get;
            private set;
        }

        public Func<IReaderWrapper, T> ReadSingleObject
        {
            get;
            private set;
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

        private static IEnumerable<ColumnMemberInfo> PopulateColumnMemberInfos()
        {
            Type itemType = typeof(T);

            IDictionary<string, ColumnInfo> columnInfoMap = new SortedDictionary<string, ColumnInfo>();

            CollectPropetiesInfo(itemType, columnInfoMap);
            CollectFieldInfo(itemType, columnInfoMap);

            return columnInfoMap.Select((c, i) => new ColumnMemberInfo(c.Key, i, c.Value)).ToList();
        }

        internal IEnumerable<ColumnMemberInfo> ColumnMemberInfos
        {
            get
            {
                return _ColumnMemberInfos.Value;
            }
        }
 
        public ReadOnlyRepositoryHelper()
        {
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

        private static void CollectFieldInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap)
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
                bool volatileOnStore = IsVolatileOnStore(field);
                bool volatileOnUpdate = IsVolatileOnUpdate(field);

                string columnName;
                int columnSize;

                GetColumnNameAndSize(field, out columnName, out columnSize);

                columnInfoMap.Add(field.Name, new FieldColumnInfo(columnName, columnSize, notNull, returnType, fieldType, volatileOnStore, volatileOnUpdate, field));
            }
        }

        private static void CollectPropetiesInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap)
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
                bool volatileOnStore = IsVolatileOnStore(property);
                bool volatileOnUpdate = IsVolatileOnUpdate(property);

                string columnName;
                int columnSize;

                GetColumnNameAndSize(property, out columnName, out columnSize);

                columnInfoMap.Add(property.Name, new PropertyColumnInfo(columnName, columnSize, notNull, returnType, propertyType, volatileOnStore, volatileOnUpdate, getMethod, setMethod));
            }
        }

        private static bool TryGetReturnType(Type type, out ReturnType returnType)
        {
            try
            {
                returnType = type.GetReturnType();
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
            return IsCustomAttribute<NotNullAttribute>(memberInfo);
           }

        private static bool IsIgnored(MemberInfo memberInfo)
        {
            return IsCustomAttribute<IgnoreAttribute>(memberInfo);
        }

        private static bool IsVolatileOnUpdate(MemberInfo memberInfo)
        {
            return IsCustomAttribute<VolatileOnUpdate>(memberInfo);
        }

        private static bool IsVolatileOnStore(MemberInfo memberInfo)
        {
            return IsCustomAttribute<VolatileOnStore>(memberInfo);
        }

        private static bool IsCustomAttribute<A>(MemberInfo memberInfo) where A : Attribute
        {
            return Attribute.GetCustomAttribute(memberInfo, typeof(A)) as A != null;
        }
    }
}
