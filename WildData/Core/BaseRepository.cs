using ModernRoute.WildData.Attributes;
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

        private readonly IReadOnlyDictionary<string, ColumnDescriptor> _MemberColumnMap;
        private readonly Func<IReaderWrapper, T> _ReadSingleObject;
        private readonly Action<IDbParameterCollectionWrapper, T> _SetParametersFromObject;
        protected readonly string _StorageName;
 
        public BaseRepository()
        {
            Type itemType = typeof(T);

            _StorageName = GetStorageName(itemType);

            IDictionary<string, ColumnInfo> columnInfoMap = new SortedDictionary<string, ColumnInfo>();
            CollectPropetiesInfo(itemType, columnInfoMap);
            CollectFieldInfo(itemType, columnInfoMap);

            IDictionary<string, ColumnDescriptor> memberColumnMap = new SortedDictionary<string, ColumnDescriptor>();
            IList<MemberBinding> memberAssignments = new List<MemberBinding>();

            int columnIndex = 0;
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IReaderWrapper), _IReaderWrapperParameterName);

            foreach (KeyValuePair<string, ColumnInfo> pair in columnInfoMap)
            {
                memberColumnMap.Add(pair.Key, pair.Value.GetColumnDescriptor(columnIndex));
                memberAssignments.Add(pair.Value.GetMemberAssignment(parameterExpression, columnIndex));

                columnIndex++;
            }

            _ReadSingleObject = CompileReadSingleObject(memberAssignments, parameterExpression);
        }

        private static Func<IReaderWrapper, T> CompileReadSingleObject(IList<MemberBinding> memberAssignments, ParameterExpression parameterExpression)
        {
            return Expression.Lambda<Func<IReaderWrapper, T>>(Expression.MemberInit(Expression.New(typeof(T)), memberAssignments), new ParameterExpression[] { parameterExpression }).Compile();
        }

        private void CollectFieldInfo(Type itemType, IDictionary<string, ColumnInfo> columnInfoMap)
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

                columnInfoMap.Add(field.Name, new FieldColumnInfo(columnName, columnSize, notNull, returnType, fieldType, field));
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

                string columnName;
                int columnSize;

                GetColumnNameAndSize(property, out columnName, out columnSize);

                columnInfoMap.Add(property.Name, new PropertyColumnInfo(columnName, columnSize, notNull, returnType, propertyType, getMethod, setMethod));
            }
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

        private string GetParamName(string fieldName)
        {
            return string.Concat("@", fieldName);
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

            public ColumnDescriptor GetColumnDescriptor(int columnIndex)
            {
                return new ColumnDescriptor(columnIndex, new ColumnReference(ColumnName, ReturnType));
            }

            public abstract MemberAssignment GetMemberAssignment(ParameterExpression parameter, int columnIndex);

            public ColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType)
            {
                ColumnName = columnName;
                ColumnSize = columnSize;
                NotNull = notNull;
                ReturnType = returnType;
                MemberType = memberType;
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

            public PropertyColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, MethodInfo getMethod, MethodInfo setMethod)
                : base(columnName, columnSize, notNull, returnType, memberType)
            {
                GetMethod = getMethod;
                SetMethod = setMethod;
            }

            public override MemberAssignment GetMemberAssignment(ParameterExpression parameter, int columnIndex)
            {
                return Expression.Bind(
                    SetMethod, 
                    Expression.Call(
                        parameter, 
                        MapHelper.GetMethodByReturnType(ReturnType), 
                        new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                    )
                );
            }
        }

        class FieldColumnInfo : ColumnInfo
        {
            public FieldInfo Field
            {
                get;
                private set;
            }

            public FieldColumnInfo(string columnName, int columnSize, bool notNull, ReturnType returnType, Type memberType, FieldInfo field)
                : base(columnName, columnSize, notNull, returnType, memberType)
            {
                Field = field;
            }

            public override MemberAssignment GetMemberAssignment(ParameterExpression parameter, int columnIndex)
            {
                return Expression.Bind(
                    Field, 
                    Expression.Call(
                        parameter, 
                        MapHelper.GetMethodByReturnType(ReturnType), 
                        new Expression[] { Expression.Constant(columnIndex, typeof(int)) }
                    )
                );
            }
        }
        #endregion
    }
}
