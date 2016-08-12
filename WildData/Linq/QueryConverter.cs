using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
using ModernRoute.WildData.Linq.Tree;
using ModernRoute.WildData.Linq.Tree.Expression;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ModernRoute.WildData.Linq
{
    class QueryConverter : ExpressionVisitor
    {
        private const string _AliasGeneratorPrefix = "a";

        private const string _SelectMethodName = "Select";
        private const string _WhereMethodName = "Where";
        private const string _OrderByMethodName = "OrderBy";
        private const string _OrderByDescendingMethodName = "OrderByDescending";
        private const string _ThenByMethodName = "ThenBy";
        private const string _ThenByDescendingMethodName = "ThenByDescending";
        private const string _TakeMethodName = "Take";
        private const string _SkipMethodName = "Skip";
        private const string _CountMethodName = "Count";
        private const string _LongCountMethodName = "LongCount";
        private const string _MinMethodName = "Min";
        private const string _MaxMethodName = "Max";
        private const string _AverageMethodName = "Average";
        private const string _SumMethodName = "Sum";
        private const string _DistinctMethodName = "Distinct";

        private const string _StringContainsMethodName = "Contains";
        private const string _StringStartsWithMethodName = "StartsWith";
        private const string _StringEndsWithMethodName = "EndsWith";
        private const string _StringTrimMethodName = "Trim";
        private const string _StringToLowerMethodName = "ToLower";
        private const string _StringToUpperMethodName = "ToUpper";

        private const string _StringReplaceMethodName = "Replace";
        private const string _StringIndexOfMethodName = "IndexOf";

        private const string _StringConcatMethodName = "Concat";
        private const string _StringIsNullOrEmptyMethodName = "IsNullOrEmpty";

        private const string _StringLength = "Length";

        private const string _NullableHasValue = "HasValue";
        private const string _NullableValue = "Value";

        private const string _DateTimeDay = "Day";
        private const string _DateTimeMonth = "Month";
        private const string _DateTimeYear = "Year";
        private const string _DateTimeHour = "Hour";
        private const string _DateTimeMinute = "Minute";
        private const string _DateTimeSecond = "Second";
        private const string _DateTimeMillisecond = "Millisecond";

        private Stack<QueryElementBase> _ElementsStack;
        private SourceQuery _SourceQuery;

        private IAliasGenerator _AliasGenerator;

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = e.Of<UnaryExpression>().Operand;
            }

            return e;
        }

        private void CheckStackEmpty()
        {
            if (_ElementsStack.Count < 0)
            {
                throw new InvalidOperationException(Resources.Strings.StackIsEmpty);
            }
        }

        private static void CheckElementSelect(QueryElementBase treeElement)
        {
            if (treeElement.ElementType != QueryElementType.ProjectionSelect &&
                treeElement.ElementType != QueryElementType.RegularSelect &&
                treeElement.ElementType != QueryElementType.SourceQuery)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnexpectedTreeElementTypeWith3Expected,
                    treeElement.ElementType,
                    QueryElementType.ProjectionSelect,
                    QueryElementType.RegularSelect,
                    QueryElementType.SourceQuery));
            }
        }

        private static void CheckElementType(QueryElementBase treeElement, QueryElementType elementType)
        {
            if (treeElement.ElementType != elementType)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnexpectedTreeElementTypeWith1Expected, treeElement.ElementType, elementType));
            }
        }

        private static void CheckQueryExpressionType(QueryExpression queryExpression, QueryExpressionType queryExpressionType)
        {
            if (queryExpression.ExpressionType != queryExpressionType)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnexpectedTreeElementTypeWith1Expected, queryExpression.ExpressionType, queryExpressionType));
            }
        }

        private static void CheckQueryExpressionReturnType(QueryExpression queryExpression, ReturnType returnType)
        {
            CheckReturnType(queryExpression.Type, returnType);
        }

        private static void CheckReturnType(ReturnType returnType,  ReturnType expectedReturnType)
        {   
            if (expectedReturnType != returnType)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnexpectedTreeElementTypeWith1Expected, returnType, expectedReturnType));
            }
        }

        private SourceBase PeekSourceBase()
        {
            CheckStackEmpty();

            QueryElementBase element = _ElementsStack.Peek();

            CheckElementSelect(element);

            return element.Of<SourceBase>();
        }

        private SourceBase PopSourceBase()
        {
            CheckStackEmpty();

            QueryElementBase element = _ElementsStack.Pop();

            CheckElementSelect(element);

            return element.Of<SourceBase>();
        }

        private QueryExpression PopQueryExpression()
        {
            CheckStackEmpty();

            QueryElementBase element = _ElementsStack.Pop();

            CheckElementType(element, QueryElementType.QueryExpression);

            return element.Of<QueryExpression>();
        }

        private QueryConstant PopQueryConstant()
        {
            QueryExpression queryExpression = PopQueryExpression();

            CheckQueryExpressionType(queryExpression, QueryExpressionType.Constant);

            return queryExpression.Of<QueryConstant>();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Visit(node.Left);
            QueryExpression left = PopQueryExpression();
            
            Visit(node.Right);
            QueryExpression right = PopQueryExpression();

            BinaryOperationType binaryOperationType;

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    binaryOperationType = BinaryOperationType.Add;
                    break;
                case ExpressionType.Subtract:
                    binaryOperationType = BinaryOperationType.Subtract;
                    break;
                case ExpressionType.Multiply:
                    binaryOperationType = BinaryOperationType.Multiply;
                    break;
                case ExpressionType.Divide:
                    binaryOperationType = BinaryOperationType.Divide;
                    break;
                case ExpressionType.Modulo:
                    binaryOperationType = BinaryOperationType.Remainder;
                    break;
                case ExpressionType.And:
                    if ((left.Type == ReturnType.Boolean || left.Type == ReturnType.BooleanNullable) &&
                        (right.Type == ReturnType.Boolean || right.Type == ReturnType.BooleanNullable))
                    {
                        binaryOperationType = BinaryOperationType.And;
                    }
                    else
                    {
                        binaryOperationType = BinaryOperationType.BitAnd;
                    }
                    break;
                case ExpressionType.AndAlso:
                    binaryOperationType = BinaryOperationType.And;
                    break;
                case ExpressionType.Or:
                    if ((left.Type == ReturnType.Boolean || left.Type == ReturnType.BooleanNullable) &&
                        (right.Type == ReturnType.Boolean || right.Type == ReturnType.BooleanNullable))
                    {
                        binaryOperationType = BinaryOperationType.Or;
                    }
                    else
                    {
                        binaryOperationType = BinaryOperationType.BitOr;
                    }
                    break;
                case ExpressionType.OrElse:
                    binaryOperationType = BinaryOperationType.Or;
                    break;
                case ExpressionType.ExclusiveOr:
                    if ((left.Type == ReturnType.Boolean || left.Type == ReturnType.BooleanNullable) &&
                        (right.Type == ReturnType.Boolean || right.Type == ReturnType.BooleanNullable))
                    {
                        binaryOperationType = BinaryOperationType.ExclusiveOr;
                    }
                    else
                    {
                        binaryOperationType = BinaryOperationType.BitExclusiveOr;
                    }
                    break;
                case ExpressionType.Equal:
                    binaryOperationType = BinaryOperationType.Equal;
                    break;
                case ExpressionType.NotEqual:
                    binaryOperationType = BinaryOperationType.NotEqual;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    binaryOperationType = BinaryOperationType.GreaterThanOrEqual;
                    break;
                case ExpressionType.GreaterThan:
                    binaryOperationType = BinaryOperationType.GreaterThan;
                    break;
                case ExpressionType.LessThan:
                    binaryOperationType = BinaryOperationType.LessThan;
                    break;
                case ExpressionType.LessThanOrEqual:
                    binaryOperationType = BinaryOperationType.LessThanOrEqual;
                    break;
                case ExpressionType.Coalesce:
                    binaryOperationType = BinaryOperationType.Coalesce;
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.BinaryOperationIsNotSupported, node.NodeType));
            }

            ReturnType binaryExpressionReturnType = ReturnTypeExtensions.GetReturnType(node.Type);

            QueryExpression queryExpression;

            if (left.Type == ReturnType.String && right.Type == ReturnType.String && binaryOperationType == BinaryOperationType.Add)
            {
                IEnumerable<QueryExpression> arguments = GetConcatArguments(left, right);

                queryExpression = new FunctionCall(FunctionType.Concat, binaryExpressionReturnType, arguments);
            }
            else
            {
                queryExpression = new BinaryQueryExpression(left, right, binaryExpressionReturnType, binaryOperationType);
            }

            _ElementsStack.Push(queryExpression);

            return node;
        }

        private IEnumerable<QueryExpression> GetConcatArguments(params QueryExpression[] values)
        {
            foreach (QueryExpression expression in values)
            {
                if (expression.Is<FunctionCall>())
                {
                    FunctionCall functionCall = expression.Of<FunctionCall>();

                    if (functionCall.FunctionType == FunctionType.Concat)
                    {
                        foreach (QueryExpression arg in functionCall.Arguments)
                        {
                            yield return arg;
                        }
                    }
                    else
                    {
                        yield return expression;
                    }
                }
                else
                {
                    yield return expression;
                }
            }
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            throw new NotSupportedException(Resources.Strings.BlockIsNotSupported);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            throw new NotSupportedException(Resources.Strings.CatchBlockIsNotSupported);
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            throw new NotSupportedException(Resources.Strings.ConditionalBlockIsNotSupported);
        }

        private static bool IsQuery(ConstantExpression node)
        {
            return node.Value.As<IQueryable>() != null;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (IsQuery(node))
            {
                _ElementsStack.Push(_SourceQuery);
            }
            else
            {
                QueryConstant constant = QueryConstant.Create(node.Value);

                _ElementsStack.Push(constant);
            }

            return node;
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            throw new NotSupportedException(Resources.Strings.DebugInfoIsNotSupported);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            throw new NotSupportedException(Resources.Strings.DefaultIsNotSupported);
        }

        protected override Expression VisitDynamic(DynamicExpression node)
        {
            throw new NotSupportedException(Resources.Strings.DynamicIsNotSupported);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            throw new NotSupportedException(Resources.Strings.ElementInitIsNotSupportedInThisContext);
        }

        protected override Expression VisitExtension(Expression node)
        {
            throw new NotSupportedException(Resources.Strings.ExtensionIsNotSupported);
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            throw new NotSupportedException(Resources.Strings.GotoIsNotSupported);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            throw new NotSupportedException(Resources.Strings.IndexIsNotSupported);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            throw new NotSupportedException(Resources.Strings.InvocationIsNotSupported);
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            throw new NotSupportedException(Resources.Strings.LabelIsNotSupported);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            throw new NotSupportedException(Resources.Strings.LabelTargetIsNotSupported);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            throw new NotSupportedException(Resources.Strings.LambdaIsNotSupportedInThisContext);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            throw new NotSupportedException(Resources.Strings.ListIntIsNotSupported);
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            throw new NotSupportedException(Resources.Strings.LoopIsNotSupported);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Visit(node.Expression);

            QueryExpression queryExpression = PopQueryExpression();

            switch (queryExpression.ExpressionType)
            { 
                case QueryExpressionType.SourceReference:
                    SourceBase source = PeekSourceBase();

                    string memberName = node.Member.Name;

                    if (source.MemberColumnMap.ContainsKey(memberName))
                    {
                        ColumnReference columnReference = source.MemberColumnMap[memberName];
                        _ElementsStack.Push(columnReference);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.MemberIsNotMappedToTheColumn, memberName));
                    }

                    break;
                default:
                    ConvertMemberAccess(node, queryExpression);
                    break;
            }

            return node;           
        }

        private void ConvertMemberAccess(MemberExpression node, QueryExpression source)
        {
            if (node.Expression.Type.IsGenericType && node.Expression.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                switch (node.Member.Name)
                {
                    case _NullableHasValue:
                        BinaryQueryExpression binaryExpression =
                            new BinaryQueryExpression(source, QueryConstant.CreateNull(), ReturnType.Boolean, BinaryOperationType.Equal);
                        _ElementsStack.Push(binaryExpression);
                        return;
                    case _NullableValue:
                        ReturnType typeToConvertTo = ReturnTypeExtensions.GetReturnType(node.Expression.Type.GetGenericArguments()[0]);
                        FunctionCall functionCall = new FunctionCall(FunctionType.Convert, typeToConvertTo, source);
                        _ElementsStack.Push(functionCall);
                        return;
                }
            }
            else if (node.Expression.Type == typeof(string))
            {
                switch (node.Member.Name)
                {
                    case _StringLength:
                        FunctionCall functionCall = new FunctionCall(FunctionType.Length, ReturnType.Int32, source);
                        _ElementsStack.Push(functionCall);
                        return;
                }
            }
            else if (node.Expression.Type == typeof(DateTime) || node.Expression.Type == typeof(DateTimeOffset))
            {
                FunctionType? functionType = GetDateTimeFunctionTypeByMemberName(node.Member.Name);

                if (functionType.HasValue)
                {
                    FunctionCall functionCall = new FunctionCall(functionType.Value, ReturnType.Int32, source);
                    _ElementsStack.Push(functionCall);
                    return;
                }
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.MemberIsNotSupported, node.Member.Name));            
        }

        private FunctionType? GetDateTimeFunctionTypeByMemberName(string memberName)
        {
            switch (memberName)
            {
                case _DateTimeDay:
                    return FunctionType.Day;
                case _DateTimeMonth:
                    return FunctionType.Month;
                case _DateTimeYear:
                    return FunctionType.Year;
                case _DateTimeHour:
                    return FunctionType.Hour;
                case _DateTimeMinute:
                    return FunctionType.Minute;
                case _DateTimeSecond:
                    return FunctionType.Second;
                case _DateTimeMillisecond:
                    return FunctionType.Millisecond;
                default:
                    return null;
            }
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            throw new NotSupportedException(Resources.Strings.MemberAssignmentIsNotSupportedInThisContext);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            throw new NotSupportedException(Resources.Strings.MemberBindingsIsNotSupportedInThisContext);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            throw new NotSupportedException(Resources.Strings.MemberInitExpressionIsNotSupportedInThisContext);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            throw new NotSupportedException(Resources.Strings.MemberListBindingIsNotSupported);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            throw new NotSupportedException(Resources.Strings.MemberMemberBindingIsNotSupportedInThisContext);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
            {
                switch (node.Method.Name)
                {
                    case _SelectMethodName:
                        ConvertSelect(node.Arguments);
                        break;
                    case _WhereMethodName:
                        ConvertPredicate(node.Arguments);
                        break;
                    case _OrderByMethodName:
                        ConvertOrderBy(node.Arguments, OrderType.Ascending, false);
                        break;
                    case _OrderByDescendingMethodName:
                        ConvertOrderBy(node.Arguments, OrderType.Descending, false);
                        break;
                    case _ThenByMethodName:
                        ConvertOrderBy(node.Arguments, OrderType.Ascending, true);
                        break;
                    case _ThenByDescendingMethodName:
                        ConvertOrderBy(node.Arguments, OrderType.Descending, true);
                        break;
                    case _SkipMethodName:
                        ConvertSkip(node.Arguments);
                        break;
                    case _TakeMethodName:
                        ConvertTake(node.Arguments);
                        break;
                    case _MinMethodName:
                        ConvertAggregate(node.Arguments, ProjectionType.Min, node.Type);
                        break;
                    case _MaxMethodName:
                        ConvertAggregate(node.Arguments, ProjectionType.Max, node.Type);
                        break;
                    case _AverageMethodName:
                        ConvertAggregate(node.Arguments, ProjectionType.Average, node.Type);
                        break;
                    case _SumMethodName:
                        ConvertAggregate(node.Arguments, ProjectionType.Sum, node.Type);
                        break;
                    case _LongCountMethodName:
                        ConvertAggregate(node.Arguments, ProjectionType.LongCount, node.Type);
                        break;
                    case _CountMethodName:
                        ConvertAggregate(node.Arguments, ProjectionType.Count, node.Type);
                        break;
                    case _DistinctMethodName:
                        ConvertDistinct(node.Arguments);
                        break;
                    default:
                        ThrowMethodNotSupported(node);
                        break;
                }
            }
            else if (node.Method.DeclaringType == typeof(string))
            {
                switch (node.Method.Name)
                {
                    case _StringContainsMethodName:
                        ConvertStringStringFunction(node.Arguments, node.Object, node.Type, FunctionType.Contains);
                        break;
                    case _StringStartsWithMethodName:
                        ConvertStringStringFunction(node.Arguments, node.Object, node.Type, FunctionType.StartsWith);
                        break;
                    case _StringEndsWithMethodName:
                        ConvertStringStringFunction(node.Arguments, node.Object, node.Type, FunctionType.EndsWith);
                        break;
                    case _StringTrimMethodName:
                        ConvertStringFunction(node.Object, node.Type, FunctionType.Trim);
                        break;
                    case _StringToUpperMethodName:
                        ConvertStringFunction(node.Object, node.Type, FunctionType.ToUpper);
                        break;
                    case _StringToLowerMethodName:
                        ConvertStringFunction(node.Object, node.Type, FunctionType.ToLower);
                        break;
                    case _StringReplaceMethodName:
                        ConvertReplace(node.Arguments, node.Object, node.Type, FunctionType.Replace);
                        break;
                    case _StringIndexOfMethodName:
                        ConvertIndexOf(node.Arguments, node.Object, node.Type, FunctionType.IndexOf);
                        break;
                    case _StringConcatMethodName:
                        ConvertConcat(node.Arguments, node.Type, FunctionType.Concat);
                        break;
                    case _StringIsNullOrEmptyMethodName:
                        ConvertIsNullOrEmpty(node.Arguments, node.Type, FunctionType.IsNullOrEmpty);
                        break;
                    default:
                        ThrowMethodNotSupported(node);
                        break;
                }
            }
            else
            {
                ThrowMethodNotSupported(node);
            }

            return node;
        }

        private void ConvertIsNullOrEmpty(IReadOnlyList<Expression> args, Type returnType, FunctionType functionType)
        {
            if (args.Count != 1)
            {
                ThrowWrongArgumentCount(args.Count, functionType);
            }

            Visit(args[0]);
            QueryExpression value = PopQueryExpression();
            CheckQueryExpressionReturnType(value, ReturnType.String);

            ReturnType functionReturnType = ReturnTypeExtensions.GetReturnType(returnType);

            CheckReturnType(functionReturnType, ReturnType.Boolean);

            FunctionCall functionCall = new FunctionCall(functionType, functionReturnType, value);

            _ElementsStack.Push(functionCall);
        }

        private void ConvertConcat(IReadOnlyList<Expression> args, Type returnType, FunctionType functionType)
        {
            if (args.Count < 1)
            {
                _ElementsStack.Push(QueryConstant.CreateString(string.Empty));
                return;
            }

            IEnumerable<Expression> expressions;

            IList<QueryExpression> arguments = new List<QueryExpression>();
            
            if (args.Count == 1 && args[0].Type == typeof(string[]) && args[0].NodeType == ExpressionType.NewArrayInit)
            {
                expressions = args[0].Of<NewArrayExpression>().Expressions;
            }
            else
            {
                expressions = args;
            }

            foreach (Expression expression in expressions)
            {
                Visit(expression);

                QueryExpression arg = PopQueryExpression();
                CheckQueryExpressionReturnType(arg, ReturnType.String);

                arguments.Add(arg);
            }

            ReturnType functionReturnType = ReturnTypeExtensions.GetReturnType(returnType);

            FunctionCall functionCall = new FunctionCall(functionType, functionReturnType, GetConcatArguments(arguments.ToArray()));

            _ElementsStack.Push(functionCall);
        }

        private void ConvertIndexOf(IReadOnlyList<Expression> args, Expression source, Type returnType, FunctionType functionType)
        {
            Visit(source);

            QueryExpression value = PopQueryExpression();
            CheckQueryExpressionReturnType(value, ReturnType.String);

            if (args.Count < 1 || args.Count > 3)
            {
                ThrowWrongArgumentCount(args.Count, functionType);
            }

            Visit(args[0]);
            QueryExpression arg0 = PopQueryExpression();
            CheckQueryExpressionReturnType(arg0, ReturnType.String);

            ReturnType functionReturnType = ReturnTypeExtensions.GetReturnType(returnType);

            QueryExpression arg1 = null;

            if (args.Count > 1)
            {
                Visit(args[1]);
                arg1 = PopQueryExpression();
                CheckQueryExpressionReturnType(arg1, ReturnType.Int32);
            }

            QueryExpression arg2 = null;

            if (args.Count > 2)
            {
                Visit(args[2]);
                arg2 = PopQueryExpression();
                CheckQueryExpressionReturnType(arg2, ReturnType.Int32);
            }

            FunctionCall functionCall = 
                new FunctionCall(
                    functionType, 
                    functionReturnType, 
                    EnumerateHelper.Sequence<QueryExpression>(value,arg0,arg1,arg2).Where(arg => arg != null)
                );
            
            _ElementsStack.Push(functionCall);
        }

        private void ThrowWrongArgumentCount(int argumentCount, FunctionType functionType)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.FunctionDoesntTakeArguments, argumentCount, functionType));
        }

        private void ConvertReplace(IReadOnlyList<Expression> args, Expression source, Type returnType, FunctionType functionType)
        {
            Visit(source);

            QueryExpression value = PopQueryExpression();
            CheckQueryExpressionReturnType(value, ReturnType.String);

            Visit(args[0]);

            QueryExpression arg0 = PopQueryExpression();
            CheckQueryExpressionReturnType(arg0, ReturnType.String);

            QueryExpression arg1 = PopQueryExpression();
            CheckQueryExpressionReturnType(arg1, ReturnType.String);

            ReturnType functionReturnType = ReturnTypeExtensions.GetReturnType(returnType);

            FunctionCall functionCall = new FunctionCall(functionType, functionReturnType, value, arg0, arg1);

            _ElementsStack.Push(functionCall);
        }

        private void ConvertStringFunction(Expression source, Type returnType, FunctionType functionType)
        {
            Visit(source);

            QueryExpression valueToTrim = PopQueryExpression();
            CheckQueryExpressionReturnType(valueToTrim, ReturnType.String);

            ReturnType functionReturnType = ReturnTypeExtensions.GetReturnType(returnType);

            CheckReturnType(functionReturnType, ReturnType.String);

            FunctionCall functionCall = new FunctionCall(functionType, functionReturnType, valueToTrim);

            _ElementsStack.Push(functionCall);
        }

        private void ConvertStringStringFunction(IReadOnlyList<Expression> args, Expression source, Type returnType, FunctionType functionType)
        {
            Visit(source);
            Visit(args[0]);

            QueryExpression valueToSearch = PopQueryExpression();
            CheckQueryExpressionReturnType(valueToSearch, ReturnType.String);
            QueryExpression stringToSearch = PopQueryExpression();
            CheckQueryExpressionReturnType(stringToSearch, ReturnType.String);

            ReturnType functionReturnType = ReturnTypeExtensions.GetReturnType(returnType);

            CheckReturnType(functionReturnType, ReturnType.Boolean);

            FunctionCall functionCall = new FunctionCall(functionType, functionReturnType, stringToSearch, valueToSearch);

            _ElementsStack.Push(functionCall);
        }

        private static void ThrowMethodNotSupported(MethodCallExpression methodCall)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.MethodIsNotSupported, 
                methodCall.Method.DeclaringType.FullName, methodCall.Method.Name));
        }

        private void ConvertSelect(IReadOnlyList<Expression> args)
        {
            LambdaExpression conditionExpression = StripQuotes(args[1]).Of<LambdaExpression>();

            if (conditionExpression.Parameters.Count != 1)
            {
                throw new InvalidOperationException(Resources.Strings.OnlyLambdaWithOneParameterIsSupportedInSelectClause);
            }

            Expression source = args[0];

            Visit(source);

            RegularSelect regularSelect;

            if (conditionExpression.Body.NodeType == ExpressionType.New)
            {
                NewExpression newExpression = conditionExpression.Body.Of<NewExpression>();

                regularSelect = ConvertNewExpression(newExpression);
            }
            else if (conditionExpression.Body.NodeType == ExpressionType.MemberInit)
            {
                MemberInitExpression memberInitExpression = conditionExpression.Body.Of<MemberInitExpression>();

                regularSelect = ConvertMemberInitExpression(memberInitExpression);
            }
            else
            {
                Visit(conditionExpression.Body);

                QueryExpression queryExpression = PopQueryExpression();
                SourceBase select = PopSourceBase();

                ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

                MethodCallExpression methodCallExpression = Expression.Call(parameter, ReturnTypeExtensions.GetMethodByReturnType(queryExpression.Type), Expression.Constant(0));

                LambdaExpression lambdaExpression = Expression.Lambda(methodCallExpression, parameter);

                Column column = new Column(_AliasGenerator.GenerateAlias(),queryExpression);
                
                IDictionary<string,ColumnReference> map = new Dictionary<string,ColumnReference>();
                map.Add(string.Empty, column.GetColumnReference());

                regularSelect = new RegularSelect(
                    map.AsReadOnly(),
                    lambdaExpression.Compile(),
                    _AliasGenerator.GenerateAlias(),
                    EnumerateHelper.Sequence(column),
                    select);
            }

            _ElementsStack.Push(regularSelect);
        }

        private RegularSelect ConvertMemberInitExpression(MemberInitExpression memberInitExpression)
        {
            IList<Column> columns = new List<Column>();
            IDictionary<string, ColumnReference> memberColumnMap = new Dictionary<string, ColumnReference>();

            ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

            NewExpression newExpression = ConvertNewExpression(memberInitExpression.NewExpression, columns, memberColumnMap, parameter);

            IList<MethodCallExpression> columnRetrievers = new List<MethodCallExpression>();
            IList<MemberBinding> memberBindings = new List<MemberBinding>();

            int i = columns.Count;

            foreach (MemberBinding memberBinding in memberInitExpression.Bindings)
            {
                if (memberBinding.BindingType != MemberBindingType.Assignment)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.MemberBindingTypeIsNotSupportedInThisContext,memberBinding.GetType().FullName));
                }

                MemberAssignment assignment = memberBinding.Of<MemberAssignment>();

                Visit(assignment.Expression);

                QueryExpression queryExpression = PopQueryExpression();

                MethodCallExpression methodCallExpression = Expression.Call(parameter, ReturnTypeExtensions.GetMethodByReturnType(queryExpression.Type), Expression.Constant(i));
                columnRetrievers.Add(methodCallExpression);

                memberBindings.Add(assignment.Update(methodCallExpression));

                Column column = new Column(_AliasGenerator.GenerateAlias(), queryExpression);
                columns.Add(column);

                memberColumnMap.Add(assignment.Member.Name, column.GetColumnReference());

                i++;
            }

            MemberInitExpression result = Expression.MemberInit(newExpression, memberBindings);
            LambdaExpression lambdaExpression = Expression.Lambda(result, parameter);

            Delegate projector = lambdaExpression.Compile();

            SourceBase source = PopSourceBase();

            RegularSelect select = new RegularSelect(memberColumnMap.AsReadOnly(), projector, _AliasGenerator.GenerateAlias(), columns, source);

            return select;
        }

        private RegularSelect ConvertNewExpression(NewExpression newExpression)
        {
            IList<Column> columns = new List<Column>();
            IDictionary<string, ColumnReference> memberColumnMap = new Dictionary<string, ColumnReference>();

            ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

            NewExpression result = ConvertNewExpression(newExpression, columns, memberColumnMap, parameter);

            LambdaExpression lambdaExpression = Expression.Lambda(result, parameter);

            Delegate projector = lambdaExpression.Compile();

            SourceBase source = PopSourceBase();

            RegularSelect select = new RegularSelect(memberColumnMap.AsReadOnly(), projector, _AliasGenerator.GenerateAlias(), columns, source);

            return select;
        }

        private NewExpression ConvertNewExpression(NewExpression newExpression, 
            IList<Column> columns, 
            IDictionary<string, ColumnReference> memberColumnMap, ParameterExpression parameter)
        {
            IList<MethodCallExpression> columnRetrievers = new List<MethodCallExpression>();

            int i = 0;

            using (IEnumerator<Expression> argumentsEnumerator = newExpression.Arguments.GetEnumerator())
            {
                using (IEnumerator<MemberInfo> membersEnumerator = newExpression.Members == null ? null : newExpression.Members.GetEnumerator())
                {
                    bool hasNextArgument = argumentsEnumerator.MoveNext();
                    bool hasNextMember = (membersEnumerator == null || membersEnumerator.MoveNext());

                    while (hasNextArgument && hasNextMember)
                    {
                        Expression expression = argumentsEnumerator.Current;

                        Visit(expression);
                        QueryExpression queryExpression = PopQueryExpression();

                        MethodCallExpression methodCallExpression = Expression.Call(parameter, ReturnTypeExtensions.GetMethodByReturnType(queryExpression.Type), Expression.Constant(i));
                        columnRetrievers.Add(methodCallExpression);

                        Column column = new Column(_AliasGenerator.GenerateAlias(), queryExpression);
                        columns.Add(column);

                        if (membersEnumerator != null)
                        {
                            memberColumnMap.Add(membersEnumerator.Current.Name, column.GetColumnReference());
                        }

                        i++;

                        hasNextArgument = argumentsEnumerator.MoveNext();
                        hasNextMember = (membersEnumerator == null || membersEnumerator.MoveNext());
                    }

                    if (membersEnumerator != null && hasNextMember != hasNextArgument)
                    {
                        throw new InvalidOperationException(Resources.Strings.ArgumentsAndMemberCollectionHaveDifferentItemCount);
                    }
                }
            }

            return Expression.New(newExpression.Constructor, columnRetrievers, newExpression.Members);
        }

        private void ConvertAggregate(IReadOnlyList<Expression> args, ProjectionType projectionType, Type returnType)
        {
            if (projectionType == ProjectionType.Count || projectionType == ProjectionType.LongCount)
            {
                if (args.Count > 1)
                {
                    ConvertPredicate(args); 
                }
                else
                {
                    Expression source = args[0];
                    Visit(source);
                }

                SourceBase sourceQuery = PopSourceBase();

                string columnAlias = _AliasGenerator.GenerateAlias();
                ReturnType columnType = ReturnTypeExtensions.GetReturnType(returnType);

                IDictionary<string, ColumnReference> map = new Dictionary<string, ColumnReference>();
                map.Add(string.Empty, new ColumnReference(columnAlias, columnType));

                ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

                MethodCallExpression methodCallExpression = Expression.Call(parameter, ReturnTypeExtensions.GetMethodByReturnType(columnType), Expression.Constant(0));

                LambdaExpression lambdaExpression = Expression.Lambda(methodCallExpression, parameter);

                Delegate projector = lambdaExpression.Compile();

                Projection projection = 
                    new Projection(
                        columnAlias,
                        projectionType,
                        QueryConstant.CreateInt(0)
                    );

                ProjectionSelect select = new ProjectionSelect(
                    map.AsReadOnly(),
                    projector,
                    _AliasGenerator.GenerateAlias(),
                    EnumerateHelper.Sequence(projection),
                    sourceQuery);

                _ElementsStack.Push(select);
            }
            else
            {
                if (args.Count > 1)
                {
                    ConvertSelect(args);
                }
                else
                {
                    Expression source = args[0];
                    Visit(source);
                }

                SourceBase sourceQuery = PopSourceBase();

                ColumnReference singleColumnReference = sourceQuery.Fields.Single().GetColumnReference();

                string columnAlias = _AliasGenerator.GenerateAlias();
                ReturnType columnType = ReturnTypeExtensions.GetReturnType(returnType);

                IDictionary<string, ColumnReference> map = new Dictionary<string, ColumnReference>();
                map.Add(string.Empty, new ColumnReference(columnAlias, columnType));

                ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

                MethodCallExpression methodCallExpression = Expression.Call(parameter, ReturnTypeExtensions.GetMethodByReturnType(columnType), Expression.Constant(0));

                LambdaExpression lambdaExpression = Expression.Lambda(methodCallExpression, parameter);

                Delegate projector = lambdaExpression.Compile();

                Projection projection =
                    new Projection(
                        columnAlias,
                        projectionType,
                        singleColumnReference
                    );

                ProjectionSelect select = new ProjectionSelect(
                   map.AsReadOnly(),
                   projector,
                   _AliasGenerator.GenerateAlias(),
                   EnumerateHelper.Sequence(projection),
                   sourceQuery);

                _ElementsStack.Push(select);
            }
        }

        private void ConvertPredicate(IReadOnlyList<Expression> args)
        {
            LambdaExpression conditionExpression = StripQuotes(args[1]).Of<LambdaExpression>();

            if (conditionExpression.Parameters.Count != 1)
            {
                throw new InvalidOperationException(Resources.Strings.OnlyLambdaWithOneParameterIsSupportedInWhereClause);
            }

            Expression source = args[0];

            Visit(source);
            Visit(conditionExpression.Body);

            QueryExpression queryExpression = PopQueryExpression();

            SourceBase sourceBase = PopSourceBase();
           
            SelectBase select = sourceBase.WithWhere(_AliasGenerator, queryExpression);

            _ElementsStack.Push(select);
        }

        private void ConvertDistinct(IReadOnlyList<Expression> args)
        {
            if (args.Count != 1)
            {
                throw new NotSupportedException(Resources.Strings.MethodDistinctWithOneArgIsOnlySupported);
            }

            Expression source = args[0];

            Visit(source);

            SourceBase sourceQuery = PopSourceBase();

            SelectBase select = sourceQuery.WithDistinct(_AliasGenerator);

            _ElementsStack.Push(select);
        }

        private void ConvertTake(IReadOnlyList<Expression> args)
        {
            Expression source = args[0];
            Expression body = args[1];

            Visit(source);  
            Visit(body);

            QueryConstant takeConstant = PopQueryConstant();

            int takeValue = takeConstant.GetInt();

            SourceBase sourceQuery = PopSourceBase();

            SelectBase select = sourceQuery.WithLimit(_AliasGenerator, takeValue);

            _ElementsStack.Push(select);
        }

        private void ConvertSkip(IReadOnlyList<Expression> args)
        {
            Expression source = args[0];
            Expression body = args[1];

            Visit(source);
            Visit(body);

            QueryConstant skipConstant = PopQueryConstant();

            int skipValue = skipConstant.GetInt();

            SourceBase sourceQuery = PopSourceBase();

            SelectBase select = sourceQuery.WithOffset(_AliasGenerator, skipValue);

            _ElementsStack.Push(select);
        }

        private void ConvertOrderBy(IReadOnlyList<Expression> args, OrderType orderType, bool thenBy)
        {
            if (args.Count != 2)
            {
                throw new NotSupportedException(Resources.Strings.MethodsOrderByOrderByDescThenByThenByDescWithTwoArgAreOnlySupported);
            }

            Expression source = args[0];
            Expression body = StripQuotes(args[1]).Of<LambdaExpression>().Body;

            Visit(source);
            Visit(body);

            QueryExpression orderByExpression = PopQueryExpression();

            SourceBase sourceBase = PopSourceBase();

            Order order = new Order(orderType,orderByExpression);

            SelectBase select = sourceBase.WithOrders(_AliasGenerator, thenBy, order);

            _ElementsStack.Push(select);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            throw new NotSupportedException(Resources.Strings.NewIsNotSupportedInThisContext);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            throw new NotSupportedException(Resources.Strings.NewArrayIsNotSupported);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            SourceBase source = PeekSourceBase();
            
            if (source.MemberColumnMap.ContainsKey(string.Empty))
            {
                _ElementsStack.Push(source.MemberColumnMap[string.Empty]);
            }
            else
            {
                _ElementsStack.Push(new SourceReference());
            }

            return node;
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            throw new NotSupportedException(Resources.Strings.RuntimeVariablesAreNotSupported);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw new NotSupportedException(Resources.Strings.SwitchIsNotSupported);
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            throw new NotSupportedException(Resources.Strings.SwitchCaseIsNotSupported);
        }

        protected override Expression VisitTry(TryExpression node)
        {
            throw new NotSupportedException(Resources.Strings.TryIsNotSupported);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            throw new NotSupportedException(Resources.Strings.TryBinaryIsNotSupported);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            Visit(node.Operand);

            QueryExpression queryExpression = PopQueryExpression();

            ReturnType unaryExpressionReturnType = ReturnTypeExtensions.GetReturnType(node.Type);

            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:                    
                    FunctionCall functionCall = new FunctionCall(FunctionType.Convert, unaryExpressionReturnType, queryExpression);
                    _ElementsStack.Push(functionCall);
                    return node;
                case ExpressionType.UnaryPlus:
                    _ElementsStack.Push(queryExpression);
                    return node;
            }

            UnaryOperationType unaryOperationType;

            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    if (queryExpression.Type == ReturnType.Boolean || queryExpression.Type == ReturnType.BooleanNullable)
                    {
                        unaryOperationType = UnaryOperationType.Not;
                    }
                    else
                    {
                        unaryOperationType = UnaryOperationType.BitNot;
                    }                    
                    break;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    unaryOperationType = UnaryOperationType.Minus;
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.UnaryOperationIsNotSupported, node.NodeType));
            }

            UnaryQueryExpression unaryQueryExpression = new UnaryQueryExpression(queryExpression, unaryExpressionReturnType, unaryOperationType);

            _ElementsStack.Push(unaryQueryExpression);

            return node;
        }

        public QueryConverter(SourceQuery sourceQuery)
        {
            if (sourceQuery == null)
            {
                throw new ArgumentNullException(nameof(sourceQuery));
            }

            _SourceQuery = sourceQuery;
            _AliasGenerator = new SimpleAliasGenerator(_AliasGeneratorPrefix);
        }

        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            if (expression.Is<ConstantExpression>())
            {
                ConstantExpression constantExpression = expression.Of<ConstantExpression>();
                return !IsQuery(constantExpression);
            }

            return expression.NodeType != ExpressionType.Parameter && expression.NodeType != ExpressionType.New;
        }

        public SourceBase Convert(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            expression = SubtreeEvaluator.Eval(Nominator.Nominate(CanBeEvaluatedLocally, expression), expression);

            _ElementsStack = new Stack<QueryElementBase>();

            Visit(expression);

            try
            {
                if (_ElementsStack.Count == 1)
                {
                    QueryElementBase treeElement = _ElementsStack.Pop();

                    CheckElementSelect(treeElement);

                    return treeElement.Of<SourceBase>();
                }

                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.UnexpectedStackSize, _ElementsStack.Count));
            }
            finally
            {
                _ElementsStack = null;
            }
        }
    }
}
