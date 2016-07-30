using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Linq
{
    class SubtreeEvaluator : ExpressionVisitor
    {
        private ISet<Expression> _Candidates;
        
        private SubtreeEvaluator(ISet<Expression> candidates)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            _Candidates = candidates;
        }

        public static Expression Eval(ISet<Expression> candidates, Expression expression)
        {
            return new SubtreeEvaluator(candidates).Visit(expression);
        }

        public override Expression Visit(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (_Candidates.Contains(expression))
            {
                return Evaluate(expression);
            
            }
            return base.Visit(expression);
        }

        private Expression Evaluate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return expression;
            }

            Type type = expression.Type;

            if (type.IsValueType)
            {
                expression = Expression.Convert(expression, typeof(object));
            }

            Expression<Func<object>> lambda = Expression.Lambda<Func<object>>(expression);
            Func<object> func = lambda.Compile();

            return Expression.Constant(func(), type);
        }
    }
}
