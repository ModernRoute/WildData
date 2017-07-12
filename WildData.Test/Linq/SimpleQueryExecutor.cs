using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Test.Linq
{
    class MockQueryExecutor : QueryExecutor
    {
        private CanonicalQueryBuilder queryBuilder;

        public MockQueryExecutor(
            IReadOnlyDictionary<string, ColumnInfo> memberColumnMap, 
            Delegate reader,
            string sourceQuery) : base(memberColumnMap, reader)
        {
            queryBuilder = new CanonicalQueryBuilder(sourceQuery);
        }

        public Delegate Reader
        {
            get;
            private set;
        }

        public string ResultQuery
        {
            get;
            private set;
        }


        protected override IEnumerable<T> ExecuteCollection<T>(FromBase sourceBase)
        {
            Tuple<string, Delegate> resultQueryReaderTuple = queryBuilder.GetInvariantRepresentation(sourceBase);

            Reader = resultQueryReaderTuple.Item2;
            ResultQuery = resultQueryReaderTuple.Item1;
            
            return Enumerable.Repeat(default(T), 1);
         } 
    }
}
