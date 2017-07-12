using ModernRoute.WildData.Core;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using System;

namespace ModernRoute.WildData.Linq
{
    sealed class QueryConverter : QueryModelVisitorBase
    {
        private const string _AliasGeneratorPrefix = "a";

        private FromSource _FromSource;

        private IAliasGenerator _AliasGenerator;
        
        public QueryConverter(FromSource fromSource)
        {
            if (fromSource == null)
            {
                throw new ArgumentNullException(nameof(fromSource));
            }

            _FromSource = fromSource;
            _AliasGenerator = new SimpleAliasGenerator(_AliasGeneratorPrefix);
        }
        
        public FromBase Convert(QueryModel queryModel)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }

            VisitQueryModel(queryModel);

            throw new NotImplementedException();
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
        {
            base.VisitOrdering(ordering, queryModel, orderByClause, index);
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            base.VisitQueryModel(queryModel);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            base.VisitSelectClause(selectClause, queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            throw new NotSupportedException();
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            throw new NotSupportedException();
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            throw new NotSupportedException();
        }
    }
}
