using ModernRoute.WildData.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ModernRoute.WildData.Linq
{
    public abstract class SelectBase : SourceBase
    {
        public bool Distinct
        {
            get;
            private set;
        }

        public int Offset
        {
            get;
            private set;
        }

        public int? Limit
        {
            get;
            private set;
        }

        public QueryExpression Predicate
        {
            get;
            private set;
        }

        public SourceBase Source
        {
            get;
            private set;
        }

        public string SourceAlias
        {
            get;
            private set;
        }

        internal override SelectBase WithDistinct(IAliasGenerator aliasGenerator)
        {
            return Recreate(Source, Predicate, true, Offset, Limit);
        }

        internal override SelectBase WithOffset(IAliasGenerator aliasGenerator, int offset)
        {
            if (Offset == 0)
            {
                return Recreate(Source, Predicate, Distinct, offset, Limit);
            }

            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, offset, null);
        }

        internal override SelectBase WithLimit(IAliasGenerator aliasGenerator, int limit)
        {
            if (Limit <= limit)
            {
                return this;
            }

            return Recreate(Source, Predicate, Distinct, Offset, limit);
        }

        protected abstract SelectBase Recreate(SourceBase source, QueryExpression predicate, bool distinct, int offset, int? limit);

        public SelectBase(
            IReadOnlyDictionary<string,ColumnReference> memberColumnMap,
            Delegate projector,
            string sourceAlias,  
            SourceBase source, 
            QueryExpression predicate,
            bool distinct, 
            int offset,
            int? limit) : base(memberColumnMap, projector)
        {   
            if (sourceAlias == null)
            {
                throw new ArgumentNullException(nameof(sourceAlias));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (offset < 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.Strings.ParameterIsLessThanZero, nameof(offset)));
            }

            if (limit < 0)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture,
                    Resources.Strings.ParameterIsLessThanZero, nameof(limit)));
            }

            if (predicate != null && predicate.Type != TypeKind.Boolean)
            {
                throw new ArgumentException(
                    string.Format(CultureInfo.CurrentCulture, 
                    Resources.Strings.PredicateDoesntReturnBoolValue, nameof(predicate)));
            }

            Predicate = predicate;

            SourceAlias = sourceAlias;
            Source = source;
            Distinct = distinct;
            Offset = offset;
            Limit = limit;
        }

        internal override SelectBase WithWhere(IAliasGenerator aliasGenerator, QueryExpression expression)
        {
            if (expression == null)
            {
                return this;
            }

            QueryExpression newPredicate;

            if (Predicate == null)
            {
                newPredicate = expression;
            }
            else
            {
                if (expression.Type != TypeKind.Boolean)
                {
                    throw new ArgumentException(
                        string.Format(CultureInfo.CurrentCulture,
                        Resources.Strings.PredicateDoesntReturnBoolValue, nameof(expression)));
                }

                newPredicate = new BinaryQueryExpression(Predicate, expression, TypeKind.Boolean, BinaryOperationType.And);
            }

            if (Offset == 0 && Limit == null)
            {
                return Recreate(Source, newPredicate, Distinct, Offset, Limit);
            }

            return new RegularSelect(MemberColumnMap, Projector, aliasGenerator.GenerateAlias(), GetColumnReferences(), this, newPredicate);        
        }
    }
}
