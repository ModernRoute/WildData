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
            _QueryString.Append(SyntaxHelper.QueryDelimiterToken);

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
                _QueryString.Append(SyntaxHelper.SpaceToken);
            }

            BuildSelectPart3(regularSelect);
        }

        private void BuildSelectPart3(SelectBase selectBase)
        {
            if (VisitLimitOffset(selectBase.Limit, selectBase.Offset))
            {
                _QueryString.Append(SyntaxHelper.SpaceToken);
            }

            _QueryString.Remove(_QueryString.Length - 1, 1);
        }

        private void BuildSelectPart2(SelectBase selectBase)
        {
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.FromToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            VisitSourceBase(selectBase.Source);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.AsToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
            _QueryString.Append(selectBase.SourceAlias);
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
            _QueryString.Append(SyntaxHelper.SpaceToken);

            if (VisitPredicate(selectBase.Predicate))
            {
                _QueryString.Append(SyntaxHelper.SpaceToken);
            }
        }

        private void BuildSelectPart1(SelectBase selectBase)
        {
            _QueryString.Append(SyntaxHelper.SelectToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);

            if (selectBase.Distinct)
            {
                _QueryString.Append(SyntaxHelper.DistinctToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
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

                _QueryString.Append(SyntaxHelper.LimitToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(SyntaxHelper.NoLimitValue);
            }
            else
            {
                _QueryString.Append(SyntaxHelper.LimitToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(limit.Value);
            }

            if (offset > 0)
            {
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(SyntaxHelper.OffsetToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                _QueryString.Append(offset);
            }

            return true;
        }

        private bool VisitOrderCollection(IReadOnlyList<Order> orders)
        {
            if (orders.Count > 0)
            {
                _QueryString.Append(SyntaxHelper.OrderByToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
                VisitList(orders, VisitOrder);

                return true;
            }

            return false;
        }

        private void VisitOrder(Order order)
        {
            VisitQueryExpression(order.Expression);
            _QueryString.Append(SyntaxHelper.SpaceToken);

            switch (order.Type)
            {
                case OrderType.Ascending:
                    _QueryString.Append(SyntaxHelper.AscendingToken);
                    break;
                case OrderType.Descending:
                    _QueryString.Append(SyntaxHelper.DescendingToken);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.OrderTypeIsNotSupported,order.Type));
            }
        }

        private void VisitList<T>(IEnumerable<T> list, Action<T> elementVisitor, string separator = SyntaxHelper.CommaToken)
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
                }

                elementVisitor(item);       
            }
        }

        private bool VisitPredicate(QueryExpression predicate)
        {
            if (predicate != null)
            {
                _QueryString.Append(SyntaxHelper.WhereToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
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
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.AsToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
            _QueryString.Append(EscapeHelper.EscapeString(column.Alias));
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
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
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);

            if (projection.Distinct)
            {
                _QueryString.Append(SyntaxHelper.DistinctToken);
                _QueryString.Append(SyntaxHelper.SpaceToken);
            }

            VisitQueryExpression(projection.Definition);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);

            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.AsToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
            _QueryString.Append(EscapeHelper.EscapeString(projection.Alias));
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
        }

        private string GetProjectionFuncName(ProjectionType projectionType)
        {
            switch (projectionType)
            {
                case ProjectionType.Average:
                    return SyntaxHelper.AverageToken;
                case ProjectionType.Max:
                    return SyntaxHelper.MaxToken;
                case ProjectionType.Min:
                    return SyntaxHelper.MinToken;
                case ProjectionType.Sum:
                    return SyntaxHelper.SumToken;
                case ProjectionType.Count:
                case ProjectionType.LongCount:
                    return SyntaxHelper.CountToken;
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
                    return SyntaxHelper.UnaryBitNotToken;
                case UnaryOperationType.Minus:
                    return SyntaxHelper.UnaryMinusToken;
                case UnaryOperationType.Not:
                    return SyntaxHelper.UnaryNotToken;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.UnaryExpressionTypeIsNotSupported, unaryOperationType));
            }
        }

        private void VisitUnaryOperation(UnaryQueryExpression unaryQueryExpression)
        {
            string unaryOperator = GetUnaryOperator(unaryQueryExpression.Operation);

            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            _QueryString.Append(unaryOperator);
            VisitQueryExpression(unaryQueryExpression.Expression);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);            
        }

        private string GetBinaryOperator(BinaryOperationType binaryOperationType, TypeKind typeKind)
        {
            switch (binaryOperationType)
            {
                case BinaryOperationType.Add:
                    return SyntaxHelper.BinaryAddToken;
                case BinaryOperationType.And:
                    return SyntaxHelper.BinaryAndToken;
                case BinaryOperationType.BitAnd:
                    return SyntaxHelper.BinaryBitAndToken;
                case BinaryOperationType.BitExclusiveOr:
                    return SyntaxHelper.BinaryBitExclusiveOrToken;
                case BinaryOperationType.BitOr:
                    return SyntaxHelper.BinaryBitOrToken;
                case BinaryOperationType.Coalesce:
                    return SyntaxHelper.BinaryCoalesceToken;
                case BinaryOperationType.Divide:
                    return SyntaxHelper.BinaryDivToken;
                case BinaryOperationType.Equal:
                    return SyntaxHelper.BinaryEqualToken;
                case BinaryOperationType.ExclusiveOr:
                    return SyntaxHelper.BinaryExclusiveOrToken;
                case BinaryOperationType.GreaterThan:
                    return SyntaxHelper.BinaryGreaterThanToken;
                case BinaryOperationType.GreaterThanOrEqual:
                    return SyntaxHelper.BinaryGreaterThanOrEqualToken;
                case BinaryOperationType.LessThan:
                    return SyntaxHelper.BinaryLessThanToken;
                case BinaryOperationType.LessThanOrEqual:
                    return SyntaxHelper.BinaryLessThanOrEqualToken;
                case BinaryOperationType.Multiply:
                    return SyntaxHelper.BinaryMultiplyToken;
                case BinaryOperationType.NotEqual:
                    return SyntaxHelper.BinaryNotEqualToken;
                case BinaryOperationType.Or:
                    return SyntaxHelper.BinaryOrToken;
                case BinaryOperationType.Remainder:
                    return SyntaxHelper.BinaryModToken;
                case BinaryOperationType.Subtract:
                    return SyntaxHelper.BinarySubtractToken;
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
                _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
                VisitQueryExpression(binaryQueryExpression.Left);
                _QueryString.Append(SyntaxHelper.CommaToken);
                VisitQueryExpression(binaryQueryExpression.Right);
                _QueryString.Append(SyntaxHelper.RightParenthesisToken);
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
                            optionOperator = SyntaxHelper.BinaryOrToken;
                            nullComparisonOperator = SyntaxHelper.IsNullToken;
                            optionConditionLogicOperator = SyntaxHelper.BinaryAndToken;
                        }
                        else
                        {
                            optionOperator = SyntaxHelper.BinaryAndToken;
                            nullComparisonOperator = SyntaxHelper.IsNotNullToken;
                            optionConditionLogicOperator = SyntaxHelper.BinaryOrToken;
                        }

                        _QueryString.Append(SyntaxHelper.LeftParenthesisToken);

                        _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
                        int leftStartIndex = _QueryString.Length;
                        VisitQueryExpression(binaryQueryExpression.Left);
                        int leftStopExclusive = _QueryString.Length;

                        _QueryString.Append(binaryOperator);

                        int rightStartIndex = _QueryString.Length;
                        VisitQueryExpression(binaryQueryExpression.Right);
                        int rightStopExclusive = _QueryString.Length;
                        _QueryString.Append(SyntaxHelper.RightParenthesisToken);

                        _QueryString.Append(optionOperator);
                    
                        _QueryString.Append(SyntaxHelper.LeftParenthesisToken);

                        for (int i = leftStartIndex; i < leftStopExclusive; i++)
                        {
                            _QueryString.Append(_QueryString[i]);
                        }

                        _QueryString.Append(SyntaxHelper.SpaceToken);
                        _QueryString.Append(nullComparisonOperator);

                        _QueryString.Append(optionConditionLogicOperator);
                        
                        for (int i = rightStartIndex; i < rightStopExclusive; i++)
                        {
                            _QueryString.Append(_QueryString[i]);
                        }

                        _QueryString.Append(SyntaxHelper.SpaceToken);
                        _QueryString.Append(nullComparisonOperator);
                        _QueryString.Append(SyntaxHelper.RightParenthesisToken);

                        _QueryString.Append(SyntaxHelper.RightParenthesisToken);

                        return;
                    }
                }

                _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
                VisitQueryExpression(binaryQueryExpression.Left);
                _QueryString.Append(binaryOperator);
                VisitQueryExpression(binaryQueryExpression.Right);
                _QueryString.Append(SyntaxHelper.RightParenthesisToken);                
            }
        }

        private void VisitColumnReference(ColumnReference columnReference)
        {
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);
            _QueryString.Append(EscapeHelper.EscapeString(columnReference.ColumnName));
            _QueryString.Append(SyntaxHelper.ColumnNameDelimiter);            
        }

        private void VisitDatePartFunction(string datePartToken, IEnumerable<QueryExpression> args)
        {
            _QueryString.Append(SyntaxHelper.CastOperatorToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            _QueryString.Append(SyntaxHelper.ExtractDatePartToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            _QueryString.Append(datePartToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.FromToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            VisitList(args, VisitQueryExpression, SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.AsToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.IntegerTypeToken);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        private void VisitFunctionCall(FunctionCall functionCall)
        {
            switch (functionCall.FunctionType)
            {
                case FunctionType.Day:
                    VisitDatePartFunction(SyntaxHelper.DayFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Month:
                    VisitDatePartFunction(SyntaxHelper.MonthFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Year:
                    VisitDatePartFunction(SyntaxHelper.YearFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Hour:
                    VisitDatePartFunction(SyntaxHelper.HourFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Minute:
                    VisitDatePartFunction(SyntaxHelper.MinuteFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Second:
                    VisitDatePartFunction(SyntaxHelper.SecondFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Millisecond:
                    VisitDatePartFunction(SyntaxHelper.MillisecondFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Length:
                    VisitDirectFunction(SyntaxHelper.LengthFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Trim:
                    VisitDirectFunction(SyntaxHelper.TrimFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Concat:
                    VisitDirectFunction(SyntaxHelper.ConcatFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.ToUpper:
                    VisitDirectFunction(SyntaxHelper.UpperFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.ToLower:
                    VisitDirectFunction(SyntaxHelper.LowerFunctionNameToken, functionCall.Arguments);
                    break;
                case FunctionType.Replace:
                    VisitDirectFunction(SyntaxHelper.ReplaceFunctionNameToken, functionCall.Arguments);
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
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            _QueryString.Append(SyntaxHelper.PositionFunctionNameToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            VisitQueryExpression(arguments[1]);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.InToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            VisitQueryExpression(arguments[0]);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            _QueryString.Append(SyntaxHelper.BinarySubtractToken);
            _QueryString.Append(1);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        private void VisitDirectFunction(string functionName, IEnumerable<QueryExpression> args)
        {
            _QueryString.Append(functionName);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            VisitList(args, VisitQueryExpression, SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        private bool IsRelatedTypes(TypeKind targetType, TypeKind originType)
        {
            switch (targetType)
            {
                case TypeKind.Binary:
                    return originType == TypeKind.Binary;
                case TypeKind.Boolean:
                case TypeKind.BooleanNullable:
                    return originType == TypeKind.Boolean || originType == TypeKind.BooleanNullable;
                case TypeKind.Int64:
                case TypeKind.Int64Nullable:
                    return originType == TypeKind.Int64 || originType == TypeKind.Int64Nullable ||
                           originType == TypeKind.Int32 || originType == TypeKind.Int32Nullable ||
                           originType == TypeKind.Int16 || originType == TypeKind.Int16Nullable;
                case TypeKind.Int32:
                case TypeKind.Int32Nullable:
                    return originType == TypeKind.Int32 || originType == TypeKind.Int32Nullable ||
                           originType == TypeKind.Int16 || originType == TypeKind.Int16Nullable;
                case TypeKind.Int16:
                case TypeKind.Int16Nullable:
                    return originType == TypeKind.Int16 || originType == TypeKind.Int16Nullable;
                case TypeKind.String:
                    return originType == TypeKind.String;
                case TypeKind.Guid:
                case TypeKind.GuidNullable:
                    return originType == TypeKind.Guid || originType == TypeKind.GuidNullable;
                case TypeKind.DateTime:
                case TypeKind.DateTimeNullable:
                case TypeKind.DateTimeOffset:
                case TypeKind.DateTimeOffsetNullable:
                    return originType == TypeKind.DateTime || originType == TypeKind.DateTimeNullable ||
                           originType == TypeKind.DateTimeOffset || originType == TypeKind.DateTimeOffsetNullable;
                case TypeKind.Double:
                case TypeKind.DoubleNullable:
                    return originType == TypeKind.Double || originType == TypeKind.DoubleNullable ||
                           originType == TypeKind.Float || originType == TypeKind.FloatNullable;
                case TypeKind.Float:
                case TypeKind.FloatNullable:
                    return originType == TypeKind.Float || originType == TypeKind.FloatNullable;
                case TypeKind.Decimal:
                case TypeKind.DecimalNullable:
                    return originType == TypeKind.Decimal || originType == TypeKind.DecimalNullable;
                case TypeKind.AnyNullable:
                    return true;
                default:
                    return false;
            }
        }

        private void VisitConvertFunction(QueryExpression queryExpression, TypeKind typeKind)
        {
            if (IsRelatedTypes(typeKind, queryExpression.Type))
            {
                VisitQueryExpression(queryExpression);
                return;
            }

            _QueryString.Append(SyntaxHelper.CastOperatorToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            VisitQueryExpression(queryExpression);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.AsToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(GetNpgsqlType(typeKind));
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        private string GetNpgsqlType(TypeKind typeKind)
        {
            switch (typeKind)
            {
                case TypeKind.Boolean:
                case TypeKind.BooleanNullable:
                    return SyntaxHelper.BooleanTypeToken;
                case TypeKind.Int64:
                case TypeKind.Int64Nullable:
                    return SyntaxHelper.BigIntTypeToken;
                case TypeKind.Int32:
                case TypeKind.Int32Nullable:
                    return SyntaxHelper.IntegerTypeToken;
                case TypeKind.Int16:
                case TypeKind.Int16Nullable:
                    return SyntaxHelper.SmallIntTypeToken;
                case TypeKind.String:
                    return SyntaxHelper.TextTypeToken;
                case TypeKind.DateTime:
                case TypeKind.DateTimeNullable:
                    return SyntaxHelper.TimestampTypeToken;
                case TypeKind.DateTimeOffset:
                case TypeKind.DateTimeOffsetNullable:
                    return SyntaxHelper.TimestampTzTypeToken;
                case TypeKind.Double:
                case TypeKind.DoubleNullable:
                    return SyntaxHelper.DoubleTypeToken;
                case TypeKind.Float:
                case TypeKind.FloatNullable:
                    return SyntaxHelper.RealTypeToken;
                case TypeKind.Decimal:
                case TypeKind.DecimalNullable:
                    return SyntaxHelper.NumericTypeToken;
                case TypeKind.Guid:
                case TypeKind.GuidNullable:
                    return SyntaxHelper.UuidTypeToken;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.ConvertingToTypeIsNotSupported, typeKind));
            }
        }

        private void VisitIsNullOrEmpty(QueryExpression queryExpression)
        {
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            int startIndex = _QueryString.Length;
            VisitQueryExpression(queryExpression);
            int endIndex = _QueryString.Length;
            _QueryString.Append(SyntaxHelper.BinaryEqualToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.BinaryOrToken);
            for (int i = startIndex; i < endIndex; i++)
            {
                _QueryString.Append(_QueryString[i]);
            }
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.IsNullToken);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        private void VisitLikeawareFunction(FunctionType functionType, QueryExpression stringToSearchExpression, QueryExpression patternExpression)
        {
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            VisitQueryExpression(stringToSearchExpression);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.LikeToken);
            _QueryString.Append(SyntaxHelper.SpaceToken);
            _QueryString.Append(SyntaxHelper.ConcatFunctionNameToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);

            if (functionType == FunctionType.Contains || functionType == FunctionType.EndsWith)
            {
                _QueryString.Append(SyntaxHelper.StringDelimiter);
                _QueryString.Append(SyntaxHelper.LikeAnyNumberCharactersSign);
                _QueryString.Append(SyntaxHelper.StringDelimiter);
                _QueryString.Append(SyntaxHelper.CommaToken);
            }

            VisitEscapedQueryExpression(patternExpression);

            if (functionType == FunctionType.Contains || functionType == FunctionType.StartsWith)
            {
                _QueryString.Append(SyntaxHelper.CommaToken);
                _QueryString.Append(SyntaxHelper.StringDelimiter);
                _QueryString.Append(SyntaxHelper.LikeAnyNumberCharactersSign);
                _QueryString.Append(SyntaxHelper.StringDelimiter);
            }

            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
        }

        private void VisitEscapedQueryExpression(QueryExpression queryExpression)
        {
            _QueryString.Append(SyntaxHelper.ReplaceFunctionNameToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            _QueryString.Append(SyntaxHelper.ReplaceFunctionNameToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            _QueryString.Append(SyntaxHelper.ReplaceFunctionNameToken);
            _QueryString.Append(SyntaxHelper.LeftParenthesisToken);
            VisitQueryExpression(queryExpression);
            _QueryString.Append(SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.LikeEscapeCharacter);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.LikeEscapeCharacter);
            _QueryString.Append(SyntaxHelper.LikeEscapeCharacter);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            _QueryString.Append(SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.LikeAnyNumberCharactersSign);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.LikeEscapeCharacter);
            _QueryString.Append(SyntaxHelper.LikeAnyNumberCharactersSign);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
            _QueryString.Append(SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.LikeExactlyOneCharacterSign);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.CommaToken);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.LikeEscapeCharacter);
            _QueryString.Append(SyntaxHelper.LikeExactlyOneCharacterSign);
            _QueryString.Append(SyntaxHelper.StringDelimiter);
            _QueryString.Append(SyntaxHelper.RightParenthesisToken);
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
            if (queryConstant.Type == TypeKind.AnyNullable)
            {
                _QueryString.Append(SyntaxHelper.NullToken);
                return;
            }

            string parameterAlias = _AliasGenerator.GenerateAlias();

            NpgsqlParameter parameter;

            switch (queryConstant.Type)
            {
                case TypeKind.Binary:
                    byte[] byteBinaryValue = queryConstant.GetBytes();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Bytea, byteBinaryValue?.Length ?? 0);
                    parameter.Value = GetDbValue(byteBinaryValue);
                    break;
                case TypeKind.Boolean:
                    bool boolValue = queryConstant.GetBoolean();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Boolean);
                    parameter.Value = boolValue;
                    break;
                case TypeKind.BooleanNullable:
                    bool? boolNullableValue = queryConstant.GetBooleanNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Boolean);
                    parameter.Value = GetDbValue(boolNullableValue);
                    break;
                case TypeKind.Int16:
                    short shortValue = queryConstant.GetShort();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Smallint);
                    parameter.Value = shortValue;
                    break;
                case TypeKind.Int16Nullable:
                    short? shortNullableValue = queryConstant.GetShortNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Smallint);
                    parameter.Value = GetDbValue(shortNullableValue);
                    break;
                case TypeKind.Int32:
                    int intValue = queryConstant.GetInt();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Integer);
                    parameter.Value = intValue;
                    break;
                case TypeKind.Int32Nullable:
                    int? intNullableValue = queryConstant.GetIntNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Integer);
                    parameter.Value = GetDbValue(intNullableValue);
                    break;
                case TypeKind.Int64:
                    long longValue = queryConstant.GetLong();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Bigint);
                    parameter.Value = longValue;
                    break;
                case TypeKind.Int64Nullable:
                    long? longNullableValue = queryConstant.GetLongNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Bigint);
                    parameter.Value = GetDbValue(longNullableValue);
                    break;
                case TypeKind.Float:
                    float floatValue = queryConstant.GetFloat();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Real);
                    parameter.Value = floatValue;
                    break;
                case TypeKind.FloatNullable:
                    float? floatNullableValue = queryConstant.GetFloatNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Real);
                    parameter.Value = GetDbValue(floatNullableValue);
                    break;
                case TypeKind.Double:
                    double doubleValue = queryConstant.GetDouble();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Double);
                    parameter.Value = doubleValue;
                    break;
                case TypeKind.DoubleNullable:
                    double? doubleNullableValue = queryConstant.GetDoubleNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Double);
                    parameter.Value = GetDbValue(doubleNullableValue);
                    break;
                case TypeKind.Decimal:
                    decimal decimalValue = queryConstant.GetDecimal();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Numeric);
                    parameter.Value = decimalValue;
                    break;
                case TypeKind.DecimalNullable:
                    decimal? decimalNullableValue = queryConstant.GetDecimalNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Numeric);
                    parameter.Value = GetDbValue(decimalNullableValue);
                    break;
                case TypeKind.DateTime:
                    DateTime dateTimeValue = queryConstant.GetDateTime();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Timestamp);
                    parameter.Value = dateTimeValue;
                    break;
                case TypeKind.DateTimeNullable:
                    DateTime? dateTimeNullableValue = queryConstant.GetDateTimeNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Timestamp);
                    parameter.Value = GetDbValue(dateTimeNullableValue);
                    break;
                case TypeKind.DateTimeOffset:
                    DateTimeOffset dateTimeOffsetValue = queryConstant.GetDateTimeOffset();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.TimestampTZ);
                    parameter.Value = dateTimeOffsetValue;
                    break;
                case TypeKind.DateTimeOffsetNullable:
                    DateTimeOffset? dateTimeOffsetNullableValue = queryConstant.GetDateTimeOffsetNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.TimestampTZ);
                    parameter.Value = GetDbValue(dateTimeOffsetNullableValue);
                    break;
                case TypeKind.Guid:
                    Guid guidValue = queryConstant.GetGuid();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Uuid);
                    parameter.Value = guidValue;
                    break;
                case TypeKind.GuidNullable:
                    Guid? guidNullableValue = queryConstant.GetGuidNullable();
                    parameter = new NpgsqlParameter(parameterAlias, NpgsqlDbType.Uuid);
                    parameter.Value = GetDbValue(guidNullableValue);
                    break;
                case TypeKind.String:
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
