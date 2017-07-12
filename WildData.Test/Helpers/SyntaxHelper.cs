using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernRoute.WildData.Test.Helpers
{
    static class SyntaxHelper
    {
        public const string CommaToken = ", ";
        public const char SpaceToken = ' ';
        public const char DotToken = '.';
        public const char QuoteToken = '\"';
        public const char LeftParenthesisToken = '(';
        public const char RightParenthesisToken = ')';

        public const string SelectToken = "select";
        public const string FromToken = "from";
        public const string WhereToken = "where";
        public const string AsToken = "as";
        public const string DistinctToken = "distinct";
        public const string LimitToken = "limit";
        public const string OffsetToken = "offset";
        public const string NoLimitValue = "all";

        public const string OrderByToken = "orderby";

        public const string NullToken = "null";
    }
}
