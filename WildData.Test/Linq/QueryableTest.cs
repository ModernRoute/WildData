using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Test.Core;
using ModernRoute.WildData.Test.Helpers;
using ModernRoute.WildData.Test.Models;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;

namespace ModernRoute.WildData.Test.Linq
{
    [TestFixture]
    class QueryableTest
    {
        private ReadOnlyRepositoryHelper<AllTypesModel> _Helper;
        private string _SourceQuery;
        private MockQueryExecutor _MockExecutor;
        private Queryable<AllTypesModel> _Queryable;

        [Test]
        public void NonTouchedQueryTest()
        {
            CheckQuery(_SourceQuery, q => q.ToList());
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            _Helper = new ReadOnlyRepositoryHelper<AllTypesModel>(new SupportEverythingTypeKindInfo());

            StringBuilder sourceQueryBuilder = new StringBuilder();
            sourceQueryBuilder.Append(SyntaxHelper.SelectToken);
            sourceQueryBuilder.Append(SyntaxHelper.SpaceToken);
            sourceQueryBuilder.Append(
                string.Join(
                    SyntaxHelper.CommaToken, 
                    _Helper
                        .MemberColumnMap
                        .Values
                        .OrderBy(v => v.ColumnIndex)
                        .Select(v => string.Concat(SyntaxHelper.QuoteToken, EscapeHelper.EscapeString(v.ColumnName), SyntaxHelper.QuoteToken))
                     )
            );
            sourceQueryBuilder.Append(SyntaxHelper.SpaceToken);
            sourceQueryBuilder.Append(SyntaxHelper.FromToken);
            sourceQueryBuilder.Append(SyntaxHelper.SpaceToken);

            if (_Helper.StorageSchema != null)
            {
                sourceQueryBuilder.Append(SyntaxHelper.SpaceToken);
                sourceQueryBuilder.Append(SyntaxHelper.QuoteToken);
                sourceQueryBuilder.Append(EscapeHelper.EscapeString(_Helper.StorageSchema));
                sourceQueryBuilder.Append(SyntaxHelper.QuoteToken);
                sourceQueryBuilder.Append(SyntaxHelper.DotToken);
            }

            sourceQueryBuilder.Append(SyntaxHelper.QuoteToken);
            sourceQueryBuilder.Append(_Helper.StorageName);
            sourceQueryBuilder.Append(SyntaxHelper.QuoteToken);

            _SourceQuery = sourceQueryBuilder.ToString();

            _MockExecutor = new MockQueryExecutor(_Helper.MemberColumnMap, _Helper.ReadSingleObject, _SourceQuery);
            _Queryable = new Queryable<AllTypesModel>(_MockExecutor);
        }

        private void CheckQuery<T>(string expectedQuery, Func<Queryable<AllTypesModel>,T> queryRunner)
        {
            queryRunner(_Queryable);
            Assert.AreEqual(expectedQuery, _MockExecutor.ResultQuery);
        }
    }
}
