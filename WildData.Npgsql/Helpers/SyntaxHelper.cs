namespace ModernRoute.WildData.Npgsql.Helpers
{
    public static class SyntaxHelper
    {
        public const string DeleteToken = "DELETE";
        public const string SelectToken = "SELECT";
        public const string SetToken = "SET";
        public const string InsertToken = "INSERT INTO";
        public const string UpdateToken = "UPDATE";
        public const string ValuesToken = "VALUES";
        public const string FromToken = "FROM";
        public const string WhereToken = "WHERE";
        public const string DefaultToken = "DEFAULT";
        public const string ReturningToken = "RETURNING";
        public const string AsToken = "AS";
        public const string DistinctToken = "DISTINCT";

        public const string LimitToken = "LIMIT";
        public const string OffsetToken = "OFFSET";
        public const string NoLimitValue = "ALL";

        public const string OrderByToken = "ORDER BY";
        public const string AscendingToken = "ASC";
        public const string DescendingToken = "DESC";

        public const string AverageToken = "AVG";
        public const string SumToken = "SUM";
        public const string MinToken = "MIN";
        public const string MaxToken = "MAX";
        public const string CountToken = "COUNT";

        public const string UnaryMinusToken = "-";
        public const string UnaryBitNotToken = "~";
        public const string UnaryNotToken = "NOT ";

        public const string BinaryAndToken = " AND ";
        public const string BinaryOrToken = " OR ";
        public const string BinaryExclusiveOrToken = " XOR ";
        public const string BinaryAddToken = " + ";
        public const string BinarySubtractToken = " - ";
        public const string BinaryMultiplyToken = " * ";
        public const string BinaryDivToken = " / ";
        public const string BinaryModToken = " % ";
        public const string BinaryBitAndToken = " & ";
        public const string BinaryBitExclusiveOrToken = " # ";
        public const string BinaryBitOrToken = " | ";
        public const string BinaryEqualToken = " = ";
        public const string BinaryNotEqualToken = " <> ";
        public const string BinaryGreaterThanToken = " > ";
        public const string BinaryGreaterThanOrEqualToken = " >= ";
        public const string BinaryLessThanToken = " < ";
        public const string BinaryLessThanOrEqualToken = " <= ";

        public const string BinaryCoalesceToken = "COALESCE";

        public const string IsNullToken = "IS NULL";
        public const string IsNotNullToken = "IS NOT NULL";
        public const string NullToken = "NULL";

        public const string InToken = "IN";

        public const string LikeToken = "LIKE";

        public const string CastOperatorToken = "CAST";
        public const string BinaryTypeToken = "BINARY";
        public const string TextTypeToken = "TEXT";
        public const string TimestampTypeToken = "TIMESTAMP";
        public const string TimestampTzTypeToken = "TIMESTAMPTZ";
        public const string NumericTypeToken = "NUMERIC";
        public const string DoubleTypeToken = "FLOAT8";
        public const string RealTypeToken = "REAL";
        public const string IntegerTypeToken = "INTEGER";
        public const string SmallIntTypeToken = "SMALLINT";
        public const string BigIntTypeToken = "BIGINT";
        public const string BooleanTypeToken = "BOOLEAN";
        public const string UuidTypeToken = "UUID";

        public const string LengthFunctionNameToken = "LENGTH";
        public const string TrimFunctionNameToken = "TRIM";
        public const string ConcatFunctionNameToken = "CONCAT";
        public const string UpperFunctionNameToken = "UPPER";
        public const string LowerFunctionNameToken = "LOWER";
        public const string ReplaceFunctionNameToken = "REPLACE";
        public const string PositionFunctionNameToken = "POSITION";

        public const string DayFunctionNameToken = "DAY";
        public const string MonthFunctionNameToken = "MONTH";
        public const string YearFunctionNameToken = "YEAR";
        public const string HourFunctionNameToken = "HOUR";
        public const string MinuteFunctionNameToken = "MINUTE";
        public const string SecondFunctionNameToken = "SECOND";
        public const string MillisecondFunctionNameToken = "MILLISECOND";
        public const string ExtractDatePartToken = "EXTRACT";

        public const char LikeAnyNumberCharactersSign = '%';
        public const char LikeExactlyOneCharacterSign = '_';
        public const char LikeEscapeCharacter = '\\';
        
        public const char EntitySeparatorToken = '.';
        public const string CommaToken = ", ";
        public const char LeftParenthesisToken = '(';
        public const char RightParenthesisToken = ')';
        
        public const char QueryDelimiterToken = ';';
        public const char SpaceToken = ' ';
        public const char ColumnNameDelimiter = '\"';
        public const char StringDelimiter = '\'';
    }
}
