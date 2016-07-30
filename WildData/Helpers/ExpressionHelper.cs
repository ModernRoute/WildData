using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Expressions
{
    /// <summary>
    /// Helps to convert member access into string representation and back.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Gets member access string representation.
        /// </summary>
        /// <param name="memberSelector">Field or property selector.</param>
        /// <returns>String representation.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="memberSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">When the expression body is not member expression.</exception>
        public static string GetMemberPath(LambdaExpression memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentNullException(nameof(memberSelector));
            }

            Expression expression = memberSelector.Body;

            return GetMemberPath(expression);
        }

        /// <summary>
        /// Gets member access type.
        /// </summary>
        /// <param name="memberSelector">Field or property selector.</param>
        /// <returns>MemberType.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="memberSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">When the expression body is not member expression.</exception>
        public static Type GetMemberType(LambdaExpression memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentNullException(nameof(memberSelector));
            }

            Expression expression = memberSelector.Body;

            return GetMemberType(expression);
        }

        /// <summary>
        /// Gets member access string representation.
        /// </summary>
        /// <param name="expression">Body of lambda expression.</param>
        /// <returns>String representation.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="expression"/> is null.</exception>
        /// <exception cref="ArgumentException">When the expression body is not member expression.</exception>
        public static string GetMemberPath(Expression expression)
        {
            return GetMemberPathAndType(expression).Item1;
        }

        public static string GetMemberPath(MemberExpression expression)
        {
            return GetMemberPathAndType(expression).Item1;
        }

        /// <summary>
        /// Gets member access type.
        /// </summary>
        /// <param name="expression">Body of lambda expression.</param>
        /// <returns>Member type.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="expression"/> is null.</exception>
        /// <exception cref="ArgumentException">When the expression body is not member expression.</exception>
        public static Type GetMemberType(Expression expression)
        {
            return GetMemberPathAndType(expression).Item2;
        }

        public static Type GetMemberType(MemberExpression expression)
        {
            return GetMemberPathAndType(expression).Item2;
        }

        private static Tuple<string, Type> GetMemberPathAndType(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            UnaryExpression unaryExpression = expression as UnaryExpression;

            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)
            {
                expression = unaryExpression.Operand;
            }

            MemberExpression body = expression as MemberExpression;

            if (body == null)
            {
                throw new ArgumentException("");
            }

            return GetMemberPathAndType(body);
        }

        private static Tuple<string, Type> GetMemberPathAndType(MemberExpression memberExpression)
        {
            if (memberExpression == null)
            {
                throw new ArgumentNullException(nameof(memberExpression));
            }

            List<string> list = new List<string>();

            Expression expression = memberExpression;
            Type type;

            do
            {
                MemberExpression currentMemberExpression = (MemberExpression)expression;
                list.Insert(0, currentMemberExpression.Member.Name);
                type = currentMemberExpression.Type;
                expression = currentMemberExpression.Expression;
            } while (expression is MemberExpression);

            return new Tuple<string, Type>(string.Join(".", list.ToArray()), type);
        }
    }

    /// <summary>
    /// Helps to convert member access into string representation and back.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    public static class ExpressionHelper<T>
    {
        /// <summary>
        /// Gets member access string representation.
        /// </summary>
        /// <typeparam name="TKey">Fuction return type.</typeparam>
        /// <param name="memberSelector">Field or property selector.</param>
        /// <returns>String representation.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="memberSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">When the expression body is not member expression.</exception>
        public static string GetMemberPath<TKey>(Expression<Func<T, TKey>> memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentNullException(nameof(memberSelector));
            }

            Expression expression = memberSelector.Body;

            return ExpressionHelper.GetMemberPath(expression);
        }

        /// <summary>
        /// Gets member access type.
        /// </summary>
        /// <typeparam name="TKey">Fuction return type.</typeparam>
        /// <param name="memberSelector">Field or property selector.</param>
        /// <returns>MemberType.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="memberSelector"/> is null.</exception>
        /// <exception cref="ArgumentException">When the expression body is not member expression.</exception>
        public static Type GetMemberType<TKey>(Expression<Func<T, TKey>> memberSelector)
        {
            if (memberSelector == null)
            {
                throw new ArgumentNullException(nameof(memberSelector));
            }

            Expression expression = memberSelector.Body;

            return ExpressionHelper.GetMemberType(expression);
        }


        /// <summary>
        /// Gets expression by its string representation.
        /// </summary>
        /// <param name="selector">Selector.</param>
        /// <returns>Converted expression.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="selector"/> is null.</exception>
        /// <exception cref="InvalidOperationException">When provided selector is not valid for provided type.</exception>
        public static LambdaExpression GetExpression(string selector)
        {
            if (selector == null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            List<MemberInfo> members = new List<MemberInfo>();
            Type type = typeof(T);

            foreach (string member in selector.Split('.'))
            {
                MemberInfo memberInfo = type.GetMember(member).
                    Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property).
                    Single();

                members.Add(memberInfo);

                if (memberInfo.MemberType == MemberTypes.Property)
                {
                    type = ((PropertyInfo)memberInfo).PropertyType;
                }
                else
                {
                    type = ((FieldInfo)memberInfo).FieldType;
                }
            }

            ParameterExpression param = Expression.Parameter(typeof(T), "o");
            Expression expression = param;

            foreach (MemberInfo memberInfo in members)
            {
                expression = Expression.MakeMemberAccess(expression, memberInfo);
            }

            Type funcType = typeof(Func<,>);

            funcType = funcType.MakeGenericType(typeof(T), type);

            return Expression.Lambda(funcType, expression, param);
        }
    }
}
