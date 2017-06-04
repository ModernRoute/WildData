using ModernRoute.WildData.Core;
using Remotion.Linq;
using System;

namespace ModernRoute.WildData.Linq
{
    sealed class QueryConverter : QueryModelVisitorBase
    {
        private const string _AliasGeneratorPrefix = "a";

        private SourceQuery _SourceQuery;

        private IAliasGenerator _AliasGenerator;
        
        public QueryConverter(SourceQuery sourceQuery)
        {
            if (sourceQuery == null)
            {
                throw new ArgumentNullException(nameof(sourceQuery));
            }

            _SourceQuery = sourceQuery;
            _AliasGenerator = new SimpleAliasGenerator(_AliasGeneratorPrefix);
        }
        
        public SourceBase Convert(QueryModel queryModel)
        {
            if (queryModel == null)
            {
                throw new ArgumentNullException(nameof(queryModel));
            }

            throw new NotImplementedException();
        }
    }
}
