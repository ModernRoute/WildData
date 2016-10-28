using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq;
using ModernRoute.WildData.Npgsql.Helpers;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ModernRoute.WildData.Npgsql.Linq
{
    class NpgsqlQueryBuilder
    {
        private const char _ParameterNamePart = 'p';
        private const char _ParameterNamePartSeparator = '_';
        private const string _NoLimitValue = "ALL";

        private const string _QueryDelimiterToken = ";";
        private const string _SpaceToken = " ";
        private const string _LeftBracketToken = "(";
        private const string _RightBracketToken = ")";
        private const string _ColumnNameDelimiter = "\"";
        private const string _StringDelimiter = "'";

        private const string _FromToken = "FROM";
        private const string _AsToken = "AS";
        private const string _SelectToken = "SELECT";
        private const string _DistinctToken = "DISTINCT";
        private const string _OrderByToken = "ORDER BY";
        private const string _WhereToken = "WHERE";
        private const string _LimitToken = "LIMIT";
        private const string _OffsetToken = "OFFSET";

        private const string _AscendingToken = "ASC";
        private const string _DescendingToken = "DESC";

        private const string _AverageToken = "AVG";
        private const string _SumToken = "SUM";
        private const string _MinToken = "MIN";
        private const string _MaxToken = "MAX";
        private const string _CountToken = "COUNT";

        private const string _UnaryMinusToken = "-";
        private const string _UnaryBitNotToken = "~";
        private const string _UnaryNotToken = "NOT";

        private const string _BinaryAndToken = "AND";
        private const string _BinaryOrToken = "OR";
        private const string _BinaryExclusiveOrToken = "XOR";
        private const string _BinaryAddToken = "+";
        private const string _BinarySubtractToken = "-";
        private const string _BinaryMultiplyToken = "*";
        private const string _BinaryDivToken = "/";
        private const string _BinaryModToken = "%";
        private const string _BinaryBitAndToken = "&";
        private const string _BinaryBitExclusiveOrToken = "#";
        private const string _BinaryBitOrToken = "|";
        private const string _BinaryCoalesceToken = "COALESCE";
        private const string _BinaryEqualToken = "=";
        private const string _BinaryNotEqualToken = "<>";
        private const string _BinaryGreaterThanToken = ">";
        private const string _BinaryGreaterThanOrEqualToken = ">=";
        private const string _BinaryLessThanToken = "<";
        private const string _BinaryLessThanOrEqualToken = "<=";

        private const string _IsNullToken = "IS NULL";
        private const string _IsNotNullToken = "IS NOT NULL";
        private const string _NullToken = "NULL";

        private const string _LengthFunctionNameToken = "LENGTH";
        private const string _TrimFunctionNameToken = "TRIM";
        private const string _ConcatFunctionNameToken = "CONCAT";
        private const string _UpperFunctionNameToken = "UPPER";
        private const string _LowerFunctionNameToken = "LOWER";
        private const string _ReplaceFunctionNameToken = "REPLACE";
        private const string _PositionFunctionNameToken = "POSITION";

        private const string _InToken = "IN";

        private const string _DayFunctionNameToken = "DAY";
        private const string _MonthFunctionNameToken = "MONTH";
        private const string _YearFunctionNameToken = "YEAR";
        private const string _HourFunctionNameToken = "HOUR";
        private const string _MinuteFunctionNameToken = "MINUTE";
        private const string _SecondFunctionNameToken = "SECOND";
        private const string _MillisecondFunctionNameToken = "MILLISECOND";
        private const string _ExtractDatePartToken = "EXTRACT";

        private const string _LikeToken = "LIKE";
        private const string _LikeAnyNumberCharactersSign = "%";
        private const string _LikeExactlyOneCharacterSign = "_";
        private const string _LikeEscapeCharacter = "\\";

        private const string _CastOperatorToken = "CAST";

        private const string _BinaryTypeToken = "BINARY";
        private const string _TextTypeToken = "TEXT";
        private const string _TimestampTypeToken = "TIMESTAMP";
        private const string _TimestampTzTypeToken = "TIMESTAMPTZ";
        private const string _NumericTypeToken = "NUMERIC";
        private const string _DoubleTypeToken = "FLOAT8";
        private const string _RealTypeToken = "REAL";
        private const string _IntegerTypeToken = "INTEGER";
        private const string _SmallIntTypeToken = "SMALLINT";
        private const string _BigIntTypeToken = "BIGINT";
        private const string _BooleanTypeToken = "BOOLEAN";
        private const string _UuidTypeToken = "UUID";

        private const string _ParameterSeparatorToken = ",";

        private StringBuilder _QueryString;
        private string _SourceQuery;
        private IList<NpgsqlParameter> _Parameters;
        private IAliasGenerator _AliasGenerator;

        public void Build(NpgsqlCommand npgsqlCommand, SourceBase sourceBase, string sourceQuery, int paramPrefixLength)
        {
            if (npgsqlCommand == null)
            {
                throw new ArgumentNullException(nameof(npgsqlCommand));
            }

            if (sourceBase == null)
            {
                throw new ArgumentNullException(nameof(sourceBase));
            }

            if (sourceQuery == null)
            {
                throw new ArgumentNullException(nameof(sourceQuery));
            }

            if (paramPrefixLength < 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    Resources.Strings.ParameterIsLessThanZero,nameof(paramPrefixLength)));
            }

            _QueryString = new StringBuilder();
            _SourceQuery = sourceQuery;
            _Parameters = new List<NpgsqlParameter>();

            _AliasGenerator = new SimpleAliasGenerator(GetParameterAliasPrefix(paramPrefixLength));

            Build(sourceBase);
            _QueryString.Append(_QueryDelimiterToken);

            npgsqlCommand.CommandText = _QueryString.ToString();
            
            foreach (NpgsqlParameter parameter in _Parameters)
            {
                npgsqlCommand.Parameters.Add(parameter);
            }

            _QueryString = null;
            _SourceQuery = null;
            _Parameters = null;
            _AliasGenerator = null;
        }

        private static string GetParameterAliasPrefix(int paramPrefixLength)
        {
            StringBuilder prefix = new StringBuilder();
            prefix.Append(ColumnInfo.ParameterNameBasePrefix);
            int length = Math.Max(0, paramPrefixLength - prefix.Length);
            
            if (length > 0)
            {
                prefix.Append(_ParameterNamePart, length);
                prefix.Append(_ParameterNamePartSeparator);
            }

            return prefix.ToString();
        }

        private void Build(SourceBase sourceBase)
        {
            VisitSourceBase(sourceBase);
        }

        private void VisitSourceBase(SourceBase sourceBase)
        {
            switch (sourceBase.SelectType)
            {
                case SelectType.Projection:
                    VisitProjectionSelect((ProjectionSelect)sourceBase);
                    break;
                case SelectType.Regular:
                    VisitRegularSelect((RegularSelect)sourceBase);
                    break;
                case SelectType.Source:
                    VisitSourceQuery((SourceQuery)sourceBase);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.SelectTypeIsNotSupported, sourceBase.SelectType));
            }
        }

        private void VisitSourceQuery(SourceQuery sourceQuery)
        {
            _QueryString.Append(_SourceQuery);
        }

        private void VisitRegularSelect(RegularSelect regularSelect)
        {
            BuildSelectPart1(regularSelect);

            VisitColumnList(regularSelect.Columns);

            BuildSelectPart2(regularSelect);

            if (VisitOrderCollection(regularSelect.OrderCollection))
            {
                _QueryString.Append(_SpaceToken);
            }

            BuildSelectPart3(regularSelect);
        }

        private void BuildSelectPart3(SelectBase selectBase)
        {
            if (VisitLimitOffset(selectBase.Limit, selectBase.Offset))
            {
                _QueryString.Append(_SpaceToken);
            }

            _QueryString.Remove(_QueryString.Length - 1, 1);
        }

        private void BuildSelectPart2(SelectBase selectBase)
        {
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_FromToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_LeftBracketToken);
            VisitSourceBase(selectBase.Source);
            _QueryString.Append(_RightBracketToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_AsToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_ColumnNameDelimiter);
            _QueryString.Append(selectBase.SourceAlias);
            _QueryString.Append(_ColumnNameDelimiter);
            _QueryString.Append(_SpaceToken);

            if (VisitPredicate(selectBase.Predicate))
            {
                _QueryString.Append(_SpaceToken);
            }
        }

        private void BuildSelectPart1(SelectBase selectBase)
        {
            _QueryString.Append(_SelectToken);
            _QueryString.Append(_SpaceToken);

            if (selectBase.Distinct)
            {
                _QueryString.Append(_DistinctToken);
                _QueryString.Append(_SpaceToken);
            }
        }

        private bool VisitLimitOffset(int? limit, int offset)
        {
            if (limit == null)
            {
                if (offset <= 0)
                {
                    return false;
                }

                _QueryString.Append(_LimitToken);
                _QueryString.Append(_SpaceToken);
                _QueryString.Append(_NoLimitValue);
            }
            else
            {
                _QueryString.Append(_LimitToken);
                _QueryString.Append(_SpaceToken);
                _QueryString.Append(limit.Value);
            }

            if (offset > 0)
            {
                _QueryString.Append(_SpaceToken);
                _QueryString.Append(_OffsetToken);
                _QueryString.Append(_SpaceToken);
                _QueryString.Append(offset);
            }

            return true;
        }

        private bool VisitOrderCollection(IReadOnlyList<Order> orders)
        {
            if (orders.Count > 0)
            {
                _QueryString.Append(_OrderByToken);
                _QueryString.Append(_SpaceToken);
                VisitList(orders, VisitOrder);

                return true;
            }

            return false;
        }

        private void VisitOrder(Order order)
        {
            VisitQueryExpression(order.Expression);
            _QueryString.Append(_SpaceToken);

            switch (order.Type)
            {
                case OrderType.Ascending:
                    _QueryString.Append(_AscendingToken);
                    break;
                case OrderType.Descending:
                    _QueryString.Append(_DescendingToken);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.OrderTypeIsNotSupported,order.Type));
            }
        }

        private void VisitList<T>(IEnumerable<T> list, Action<T> elementVisitor, string separator = _ParameterSeparatorToken)
        {
            bool first = true;

            foreach (T item in list)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    _QueryString.Append(separator);
                    _QueryString.Append(_SpaceToken);
                }

                elementVisitor(item);       
            }
        }

        private bool VisitPredicate(QueryExpression predicate)
        {
            if (predicate != null)
            {
                _QueryString.Append(_WhereToken);
                _QueryString.Append(_SpaceToken);
                VisitQueryExpression(predicate);

                return true;
            }

            return false;
        }

        private void VisitColumnList(IReadOnlyList<Column> columns)
        {
            VisitList(columns, VisitColumn);
        }

        private void VisitColumn(Column column)
        {
            VisitQueryExpression(column.Definition);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_AsToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_ColumnNameDelimiter);
            _QueryString.Append(EscapeHelper.EscapeString(column.Alias));
            _QueryString.Append(_ColumnNameDelimiter);
        }

        private void VisitProjectionSelect(ProjectionSelect projectionSelect)
        {
            BuildSelectPart1(projectionSelect);

            VisitProjectionList(projectionSelect.Projections);

            BuildSelectPart2(projectionSelect);

            BuildSelectPart3(projectionSelect);
        }

        private void VisitProjectionList(IReadOnlyList<Projection> projections)
        {
            VisitList(projections, VisitProjection);
        }

        private void VisitProjection(Projection projection)
        {
            _QueryString.Append(GetProjectionFuncName(projection.ProjectionType));
            _QueryString.Append(_LeftBracketToken);

            if (projection.Distinct)
            {
                _QueryString.Append(_DistinctToken);
                _QueryString.Append(_SpaceToken);
            }

            VisitQueryExpression(projection.Definition);
            _QueryString.Append(_RightBracketToken);

            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_AsToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_ColumnNameDelimiter);
            _QueryString.Append(EscapeHelper.EscapeString(projection.Alias));
            _QueryString.Append(_ColumnNameDelimiter);
        }

        private string GetProjectionFuncName(ProjectionType projectionType)
        {
            switch (projectionType)
            {
                case ProjectionType.Average:
                    return _AverageToken;
                case ProjectionType.Max:
                    return _MaxToken;
                case ProjectionType.Min:
                    return _MinToken;
                case ProjectionType.Sum:
                    return _SumToken;
                case ProjectionType.Count:
                case ProjectionType.LongCount:
                    return _CountToken;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ProjectionTypeIsNotSupported, projectionType));                
            }
        }

        private void VisitQueryExpression(QueryExpression queryExpression)
        {
            switch (queryExpression.ExpressionType)
            {
                case QueryExpressionType.BinaryOperation:
                    VisitBinaryOperation((BinaryQueryExpression)queryExpression);
                    break;
                case QueryExpressionType.ColumnReference:
                    VisitColumnReference((ColumnReference)queryExpression);
                    break;
                case QueryExpressionType.Constant:
                    VisitConstant((QueryConstant)queryExpression);
                    break;
                case QueryExpressionType.FunctionCall:
                    VisitFunctionCall((FunctionCall)queryExpression);
                    break;
                case QueryExpressionType.UnaryOperation:
                    VisitUnaryOperation((UnaryQueryExpression)queryExpression);
                    break;
                case QueryExpressionType.SourceReference:
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.QueryExpressionTypeIsNotSupported, queryExpression.ExpressionType));     
            }
        }

        private string GetUnaryOperator(UnaryOperationType unaryOperationType)
        {
            switch (unaryOperationType)
            {
                case UnaryOperationType.BitNot:
                    return _UnaryBitNotToken;
                case UnaryOperationType.Minus:
                    return _UnaryMinusToken;
                case UnaryOperationType.Not:
                    return _UnaryNotToken + _SpaceToken;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.UnaryExpressionTypeIsNotSupported, unaryOperationType));
            }
        }

        private void VisitUnaryOperation(UnaryQueryExpression unaryQueryExpression)
        {
            string unaryOperator = GetUnaryOperator(unaryQueryExpression.Operation);

            _QueryString.Append(_LeftBracketToken);
            _QueryString.Append(unaryOperator);
            VisitQueryExpression(unaryQueryExpression.Expression);
            _QueryString.Append(_RightBracketToken);            
        }

        private string GetBinaryOperator(BinaryOperationType binaryOperationType, ReturnType returnType)
        {
            switch (binaryOperationType)
            {
                case BinaryOperationType.Add:
                    return _BinaryAddToken;
                case BinaryOperationType.And:
                    return _BinaryAndToken;
                case BinaryOperationType.BitAnd:
                    return _BinaryBitAndToken;
                case BinaryOperationType.BitExclusiveOr:
                    return _BinaryBitExclusiveOrToken;
                case BinaryOperationType.BitOr:
                    return _BinaryBitOrToken;
                case BinaryOperationType.Coalesce:
                    return _BinaryCoalesceToken;
                case BinaryOperationType.Divide:
                    return _BinaryDivToken;
                case BinaryOperationType.Equal:
                    return _BinaryEqualToken;
                case BinaryOperationType.ExclusiveOr:
                    return _BinaryExclusiveOrToken;
                case BinaryOperationType.GreaterThan:
                    return _BinaryGreaterThanToken;
                case BinaryOperationType.GreaterThanOrEqual:
                    return _BinaryGreaterThanOrEqualToken;
                case BinaryOperationType.LessThan:
                    return _BinaryLessThanToken;
                case BinaryOperationType.LessThanOrEqual:
                    return _BinaryLessThanOrEqualToken;
                case BinaryOperationType.Multiply:
                    return _BinaryMultiplyToken;
                case BinaryOperationType.NotEqual:
                    return _BinaryNotEqualToken;
                case BinaryOperationType.Or:
                    return _BinaryOrToken;
                case BinaryOperationType.Remainder:
                    return _BinaryModToken;
                case BinaryOperationType.Subtract:
                    return _BinarySubtractToken;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.BinaryExpressionTypeIsNotSupproted, binaryOperationType));
            }
        }

        private void VisitBinaryOperation(BinaryQueryExpression binaryQueryExpression)
        {
            string binaryOperator = GetBinaryOperator(binaryQueryExpression.Operation, binaryQueryExpression.Type);

            if (binaryQueryExpression.Operation == BinaryOperationType.Coalesce)
            {
                _QueryString.Append(binaryOperator);
                _QueryString.Append(_LeftBracketToken);
                VisitQueryExpression(binaryQueryExpression.Left);
                _QueryString.Append(_ParameterSeparatorToken);
                _QueryString.Append(_SpaceToken);
                VisitQueryExpression(binaryQueryExpression.Right);
                _QueryString.Append(_RightBracketToken);
            }
            else
            {
                if (binaryQueryExpression.Left.Type.IsNullable() && binaryQueryExpression.Right.Type.IsNullable())
                {
                    if (binaryQueryExpression.Operation == BinaryOperationType.Equal || binaryQueryExpression.Operation == BinaryOperationType.NotEqual)
                    {
                        string optionOperator;
                        string nullComparisonOperator;
                        string optionConditionLogicOperator;

                        if (binaryQueryExpression.Operation == BinaryOperationType.Equal)
                        {
                            optionOperator = _BinaryOrToken;
                            nullComparisonOperator = _IsNullToken;
                            optionConditionLogicOperator = _BinaryAndToken;
                        }
                        else
                        {
                            optionOperator = _BinaryAndToken;
                            nullComparisonOperator = _IsNotNullToken;
                            optionConditionLogicOperator = _BinaryOrToken;
                        }

                        _QueryString.Append(_LeftBracketToken);

                        _QueryString.Append(_LeftBracketToken);
                        int leftStartIndex = _QueryString.Length;
                        VisitQueryExpression(binaryQueryExpression.Left);
                        int leftStopExclusive = _QueryString.Length;

                        _QueryString.Append(_SpaceToken);
                        _QueryString.Append(binaryOperator);
                        _QueryString.Append(_SpaceToken);

                        int rightStartIndex = _QueryString.Length;
                        VisitQueryExpression(binaryQueryExpression.Right);
                        int rightStopExclusive = _QueryString.Length;
                        _QueryString.Append(_RightBracketToken);

                        _QueryString.Append(_SpaceToken);
                        _QueryString.Append(optionOperator);
                        _QueryString.Append(_SpaceToken);

                        _QueryString.Append(_LeftBracketToken);

                        for (int i = leftStartIndex; i < leftStopExclusive; i++)
                        {
                            _QueryString.Append(_QueryString[i]);
                        }

                        _QueryString.Append(_SpaceToken);
                        _QueryString.Append(nullComparisonOperator);

                        _QueryString.Append(_SpaceToken);
                        _QueryString.Append(optionConditionLogicOperator);
                        _QueryString.Append(_SpaceToken);
                        
                        for (int i = rightStartIndex; i < rightStopExclusive; i++)
                        {
                            _QueryString.Append(_QueryString[i]);
                        }

                        _QueryString.Append(_SpaceToken);
                        _QueryString.Append(nullComparisonOperator);
                        _QueryString.Append(_RightBracketToken);

                        _QueryString.Append(_RightBracketToken);

                        return;
                    }
                }

                _QueryString.Append(_LeftBracketToken);
                VisitQueryExpression(binaryQueryExpression.Left);
                _QueryString.Append(_SpaceToken);
                _QueryString.Append(binaryOperator);
                _QueryString.Append(_SpaceToken);
                VisitQueryExpression(binaryQueryExpression.Right);
                _QueryString.Append(_RightBracketToken);                
            }
        }

        private void VisitColumnReference(ColumnReference columnReference)
        {
            _QueryString.Append(_ColumnNameDelimiter);
            _QueryString.Append(EscapeHelper.EscapeString(columnReference.ColumnName));
            _QueryString.Append(_ColumnNameDelimiter);            
        }

        private void VisitDatePartFunction(string datePartToken, IEnumerable<QueryExpression> args)
        {
            _QueryString.Append(_CastOperatorToken);
            _QueryString.Append(_LeftBracketToken);
            _QueryString.Append(_ExtractDatePartToken);
            _QueryString.Append(_LeftBracketToken);
            _QueryString.Append(datePartToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_FromToken);
            _QueryString.Append(_SpaceToken);
            VisitList(args, VisitQueryExpression, _ParameterSeparatorToken);
            _QueryString.Append(_RightBracketToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_AsToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_IntegerTypeToken);
            _QueryString.Append(_RightBracketToken);
        }

        private void VisitFunctionCall(FunctionCall functionCall)
        {
            switch (functionCall.FunctionType)
            {
                case FunctionType.Day:
                    VisitDatePartFunction(_DayFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Month:
                    VisitDatePartFunction(_MonthFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Year:
                    VisitDatePartFunction(_YearFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Hour:
                    VisitDatePartFunction(_HourFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Minute:
                    VisitDatePartFunction(_MinuteFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Second:
                    VisitDatePartFunction(_SecondFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Millisecond:
                    VisitDatePartFunction(_MillisecondFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Length:
                    VisitDirectFunction(_LengthFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Trim:
                    VisitDirectFunction(_TrimFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Concat:
                    VisitDirectFunction(_ConcatFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.ToUpper:
                    VisitDirectFunction(_UpperFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.ToLower:
                    VisitDirectFunction(_LowerFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Replace:
                    VisitDirectFunction(_ReplaceFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.IndexOf:
                    VisitIndexOfFunction(functionCall.Arguments);
                    break;
                case FunctionType.Contains:
                case FunctionType.StartsWith:
                case FunctionType.EndsWith:
                    VisitLikeawareFunction(functionCall.FunctionType, functionCall.Arguments[0], functionCall.Arguments[1]);
                    break;
                case FunctionType.IsNullOrEmpty:
                    VisitIsNullOrEmpty(functionCall.Arguments[0]);
                    break;
                case FunctionType.Convert:
                    VisitConvertFunction(functionCall.Arguments.Single(), functionCall.Type);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.FunctionTypeIsNotSupported, functionCall.FunctionType));
            }
        }

        private void VisitIndexOfFunction(IReadOnlyList<QueryExpression> arguments)
        {
            _QueryString.Append(_LeftBracketToken);
            _QueryString.Append(_PositionFunctionNameToken);
            _QueryString.Append(_LeftBracketToken);
            VisitQueryExpression(arguments[1]);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_InToken);
            _QueryString.Append(_SpaceToken);
            VisitQueryExpression(arguments[0]);
            _QueryString.Append(_RightBracketToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_BinarySubtractToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(1);
            _QueryString.Append(_RightBracketToken);
        }

        private void VisitDirectFunction(string functionName, IEnumerable<QueryExpression> args)
        {
            _QueryString.Append(functionName);
            _QueryString.Append(_LeftBracketToken);
            VisitList(args, VisitQueryExpression, _ParameterSeparatorToken);
            _QueryString.Append(_RightBracketToken);
        }

        private bool IsRelatedTypes(ReturnType targetType, ReturnType originType)
        {
            switch (targetType)
            {
                case ReturnType.Binary:
                    return originType == ReturnType.Binary;
                case ReturnType.Boolean:
                case ReturnType.BooleanNullable:
                    return originType == ReturnType.Boolean || originType == ReturnType.BooleanNullable;
                case ReturnType.Int64:
                case ReturnType.Int64Nullable:
                    return originType == ReturnType.Int64 || originType == ReturnType.Int64Nullable ||
                           originType == ReturnType.Int32 || originType == ReturnType.Int32Nullable ||
                           originType == ReturnType.Int16 || originType == ReturnType.Int16Nullable;
                case ReturnType.Int32:
                case ReturnType.Int32Nullable:
                    return originType == ReturnType.Int32 || originType == ReturnType.Int32Nullable ||
                           originType == ReturnType.Int16 || originType == ReturnType.Int16Nullable;
                case ReturnType.Int16:
                case ReturnType.Int16Nullable:
                    return originType == ReturnType.Int16 || originType == ReturnType.Int16Nullable;
                case ReturnType.String:
                    return originType == ReturnType.String;
                case ReturnType.Guid:
                case ReturnType.GuidNullable:
                    return originType == ReturnType.Guid || originType == ReturnType.GuidNullable;
                case ReturnType.DateTime:
                case ReturnType.DateTimeNullable:
                case ReturnType.DateTimeOffset:
                case ReturnType.DateTimeOffsetNullable:
                    return originType == ReturnType.DateTime || originType == ReturnType.DateTimeNullable ||
                           originType == ReturnType.DateTimeOffset || originType == ReturnType.DateTimeOffsetNullable;
                case ReturnType.Double:
                case ReturnType.DoubleNullable:
                    return originType == ReturnType.Double || originType == ReturnType.DoubleNullable ||
                           originType == ReturnType.Float || originType == ReturnType.FloatNullable;
                case ReturnType.Float:
                case ReturnType.FloatNullable:
                    return originType == ReturnType.Float || originType == ReturnType.FloatNullable;
                case ReturnType.Decimal:
                case ReturnType.DecimalNullable:
                    return originType == ReturnType.Decimal || originType == ReturnType.DecimalNullable;
                case ReturnType.Null:
                    return true;
                default:
                    return false;
            }
        }

        private void VisitConvertFunction(QueryExpression queryExpression, ReturnType returnType)
        {
            if (IsRelatedTypes(returnType, queryExpression.Type))
            {
                VisitQueryExpression(queryExpression);
                return;
            }

            _QueryString.Append(_CastOperatorToken);
            _QueryString.Append(_LeftBracketToken);
            VisitQueryExpression(queryExpression);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_AsToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(GetNpgsqlType(returnType));
            _QueryString.Append(_RightBracketToken);
        }

        private string GetNpgsqlType(ReturnType returnType)
        {
            switch (returnType)
            {
                case ReturnType.Boolean:
                case ReturnType.BooleanNullable:
                    return _BooleanTypeToken;
                case ReturnType.Int64:
                case ReturnType.Int64Nullable:
                    return _BigIntTypeToken;
                case ReturnType.Int32:
                case ReturnType.Int32Nullable:
                    return _IntegerTypeToken;
                case ReturnType.Int16:
                case ReturnType.Int16Nullable:
                    return _SmallIntTypeToken;
                case ReturnType.String:
                    return _TextTypeToken;
                case ReturnType.DateTime:
                case ReturnType.DateTimeNullable:
                    return _TimestampTypeToken;
                case ReturnType.DateTimeOffset:
                case ReturnType.DateTimeOffsetNullable:
                    return _TimestampTzTypeToken;
                case ReturnType.Double:
                case ReturnType.DoubleNullable:
                    return _DoubleTypeToken;
                case ReturnType.Float:
                case ReturnType.FloatNullable:
                    return _RealTypeToken;
                case ReturnType.Decimal:
                case ReturnType.DecimalNullable:
                    return _NumericTypeToken;
                case ReturnType.Guid:
                case ReturnType.GuidNullable:
                    return _UuidTypeToken;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.ConvertingToTypeIsNotSupported, returnType));
            }
        }

        private void VisitIsNullOrEmpty(QueryExpression queryExpression)
        {
            _QueryString.Append(_LeftBracketToken);
            int startIndex = _QueryString.Length;
            VisitQueryExpression(queryExpression);
            int endIndex = _QueryString.Length;
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_BinaryEqualToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_BinaryOrToken);
            _QueryString.Append(_SpaceToken);
            for (int i = startIndex; i < endIndex; i++)
            {
                _QueryString.Append(_QueryString[i]);
            }
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_IsNullToken);
            _QueryString.Append(_RightBracketToken);
        }

        private void VisitLikeawareFunction(FunctionType functionType, QueryExpression stringToSearchExpression, QueryExpression patternExpression)
        {
            _QueryString.Append(_LeftBracketToken);
            VisitQueryExpression(stringToSearchExpression);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_LikeToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_ConcatFunctionNameToken);
            _QueryString.Append(_LeftBracketToken);

            if (functionType == FunctionType.Contains || functionType == FunctionType.EndsWith)
            {
                _QueryString.Append(_StringDelimiter);
                _QueryString.Append(_LikeAnyNumberCharactersSign);
                _QueryString.Append(_StringDelimiter);
                _QueryString.Append(_ParameterSeparatorToken);
                _QueryString.Append(_SpaceToken);
            }

            VisitEscapedQueryExpression(patternExpression);

            if (functionType == FunctionType.Contains || functionType == FunctionType.StartsWith)
            {
                _QueryString.Append(_ParameterSeparatorToken);
                _QueryString.Append(_SpaceToken);
                _QueryString.Append(_StringDelimiter);
                _QueryString.Append(_LikeAnyNumberCharactersSign);
                _QueryString.Append(_StringDelimiter);
            }

            _QueryString.Append(_RightBracketToken);
            _QueryString.Append(_RightBracketToken);
        }

        private void VisitEscapedQueryExpression(QueryExpression queryExpression)
        {
            _QueryString.Append(_ReplaceFunctionNameToken);
            _QueryString.Append(_LeftBracketToken);
            _QueryString.Append(_ReplaceFunctionNameToken);
            _QueryString.Append(_LeftBracketToken);
            _QueryString.Append(_ReplaceFunctionNameToken);
            _QueryString.Append(_LeftBracketToken);
            VisitQueryExpression(queryExpression);
            _QueryString.Append(_ParameterSeparatorToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_LikeEscapeCharacter);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_ParameterSeparatorToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_LikeEscapeCharacter);
            _QueryString.Append(_LikeEscapeCharacter);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_RightBracketToken);
            _QueryString.Append(_ParameterSeparatorToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_LikeAnyNumberCharactersSign);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_ParameterSeparatorToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_LikeEscapeCharacter);
            _QueryString.Append(_LikeAnyNumberCharactersSign);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_RightBracketToken);
            _QueryString.Append(_ParameterSeparatorToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_LikeExactlyOneCharacterSign);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_ParameterSeparatorToken);
            _QueryString.Append(_SpaceToken);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_LikeEscapeCharacter);
            _QueryString.Append(_LikeExactlyOneCharacterSign);
            _QueryString.Append(_StringDelimiter);
            _QueryString.Append(_RightBracketToken);
        }

        private object GetDbValue<T>(T? value) where T : struct
        {
            if (value.HasValue)
            {
                return value.Value;
            }

            return DBNull.Value;
        }

        private object GetDbValue<T>(T value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }

        private void VisitConstant(QueryConstant queryConstant)
        {
            if (queryConstant.Type == ReturnType.Null)
            {
                _QueryString.Append(_NullToken);
                return;
            }

            string parameterAlias = _AliasGenerator.GenerateAlias();

            NpgsqlParameter parameter;

            switch (queryConstant.Type)
            {
                case ReturnType.Binary:
                    byte[] byteBinaryValue = queryConstant.GetBytes();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Bytea, byteBinaryValue?.Length ?? 0);
                    parameter.Value = GetDbValue(byteBinaryValue);
                    break;
                case ReturnType.Boolean:
                    bool boolValue = queryConstant.GetBoolean();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Boolean);
                    parameter.Value = boolValue;
                    break;
                case ReturnType.BooleanNullable:
                    bool? boolNullableValue = queryConstant.GetBooleanNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Boolean);
                    parameter.Value = GetDbValue(boolNullableValue);
                    break;
                case ReturnType.Int16:
                    short shortValue = queryConstant.GetShort();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Smallint);
                    parameter.Value = shortValue;
                    break;
                case ReturnType.Int16Nullable:
                    short? shortNullableValue = queryConstant.GetShortNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Smallint);
                    parameter.Value = GetDbValue(shortNullableValue);
                    break;
                case ReturnType.Int32:
                    int intValue = queryConstant.GetInt();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Integer);
                    parameter.Value = intValue;
                    break;
                case ReturnType.Int32Nullable:
                    int? intNullableValue = queryConstant.GetIntNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Integer);
                    parameter.Value = GetDbValue(intNullableValue);
                    break;
                case ReturnType.Int64:
                    long longValue = queryConstant.GetLong();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Bigint);
                    parameter.Value = longValue;
                    break;
                case ReturnType.Int64Nullable:
                    long? longNullableValue = queryConstant.GetLongNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Bigint);
                    parameter.Value = GetDbValue(longNullableValue);
                    break;
                case ReturnType.Float:
                    float floatValue = queryConstant.GetFloat();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Real);
                    parameter.Value = floatValue;
                    break;
                case ReturnType.FloatNullable:
                    float? floatNullableValue = queryConstant.GetFloatNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Real);
                    parameter.Value = GetDbValue(floatNullableValue);
                    break;
                case ReturnType.Double:
                    double doubleValue = queryConstant.GetDouble();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Double);
                    parameter.Value = doubleValue;
                    break;
                case ReturnType.DoubleNullable:
                    double? doubleNullableValue = queryConstant.GetDoubleNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Double);
                    parameter.Value = GetDbValue(doubleNullableValue);
                    break;
                case ReturnType.Decimal:
                    decimal decimalValue = queryConstant.GetDecimal();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Numeric);
                    parameter.Value = decimalValue;
                    break;
                case ReturnType.DecimalNullable:
                    decimal? decimalNullableValue = queryConstant.GetDecimalNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Numeric);
                    parameter.Value = GetDbValue(decimalNullableValue);
                    break;
                case ReturnType.DateTime:
                    DateTime dateTimeValue = queryConstant.GetDateTime();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Timestamp);
                    parameter.Value = dateTimeValue;
                    break;
                case ReturnType.DateTimeNullable:
                    DateTime? dateTimeNullableValue = queryConstant.GetDateTimeNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Timestamp);
                    parameter.Value = GetDbValue(dateTimeNullableValue);
                    break;
                case ReturnType.DateTimeOffset:
                    DateTimeOffset dateTimeOffsetValue = queryConstant.GetDateTimeOffset();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.TimestampTZ);
                    parameter.Value = dateTimeOffsetValue;
                    break;
                case ReturnType.DateTimeOffsetNullable:
                    DateTimeOffset? dateTimeOffsetNullableValue = queryConstant.GetDateTimeOffsetNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.TimestampTZ);
                    parameter.Value = GetDbValue(dateTimeOffsetNullableValue);
                    break;
                case ReturnType.Guid:
                    Guid guidValue = queryConstant.GetGuid();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Uuid);
                    parameter.Value = guidValue;
                    break;
                case ReturnType.GuidNullable:
                    Guid? guidNullableValue = queryConstant.GetGuidNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Uuid);
                    parameter.Value = GetDbValue(guidNullableValue);
                    break;
                case ReturnType.String:
                    string stringValue = queryConstant.GetString();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Varchar, stringValue == null ? 0 : stringValue.Length);
                    parameter.Value = GetDbValue(stringValue);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.ConstantTypeIsNotSupprted, queryConstant.Type));
            }

            _Parameters.Add(parameter);

            _QueryString.Append(parameterAlias);
        }
    }
}
