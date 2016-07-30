using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModernRoute.WildData.Linq
{
    class Nominator : ExpressionVisitor
    {
        private Func<Expression, bool> _FuncCanBeEvaluated;
        private ISet<Expression> _Candidates;
        private bool _CannotBeEvaluated;

        private Nominator(Func<Expression, bool> funcCanBeEvaluated)
        {
            if (funcCanBeEvaluated == null)
            {
                throw new ArgumentNullException(nameof(funcCanBeEvaluated));
            }

            _Candidates = new HashSet<Expression>();
            _FuncCanBeEvaluated = funcCanBeEvaluated;
        }

        public static ISet<Expression> Nominate(Func<Expression, bool> funcCanBeEvaluated, Expression expression)
        {
            Nominator nominator = new Nominator(funcCanBeEvaluated);
            nominator.Visit(expression);

            return nominator._Candidates;
        }

        public override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = _CannotBeEvaluated;
                _CannotBeEvaluated = false;

                base.Visit(expression);

                if (!_CannotBeEvaluated)
                {
                    if (_FuncCanBeEvaluated(expression))
                    {
                        _Candidates.Add(expression);
                    }
                    else
                    {
                        _CannotBeEvaluated = true;
                    }
                }

                _CannotBeEvaluated |= saveCannotBeEvaluated;
            }

            return expression;
        }
    }
}
