﻿using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using ModernRoute.WildData.Helpers;
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

        private Stack<QueryElementBase> _ElementsStack;
        private SourceQuery _SourceQuery;

        private IAliasGenerator _AliasGenerator;

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
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

        private static void CheckQueryExpressionTypeKind(QueryExpression queryExpression, TypeKind typeKind)
        {
            CheckTypeKind(queryExpression.Type, typeKind);
        }

        private static void CheckTypeKind(TypeKind typeKind,  TypeKind expectedTypeKind)
        {   
            if (expectedTypeKind != typeKind)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnexpectedTreeElementTypeWith1Expected, typeKind, expectedTypeKind));
            }
        }

        private SourceBase PeekSourceBase()
        {
            CheckStackEmpty();

            QueryElementBase element = _ElementsStack.Peek();

            CheckElementSelect(element);

            return (SourceBase)element;
        }

        private SourceBase PopSourceBase()
        {
            CheckStackEmpty();

            QueryElementBase element = _ElementsStack.Pop();

            CheckElementSelect(element);

            return (SourceBase)element;
        }

        private QueryExpression PopQueryExpression()
        {
            CheckStackEmpty();

            QueryElementBase element = _ElementsStack.Pop();

            CheckElementType(element, QueryElementType.QueryExpression);

            return (QueryExpression)element;
        }

        private QueryConstant PopQueryConstant()
        {
            QueryExpression queryExpression = PopQueryExpression();

            CheckQueryExpressionType(queryExpression, QueryExpressionType.Constant);

            return (QueryConstant)queryExpression;
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
                    if ((left.Type == TypeKind.Boolean || left.Type == TypeKind.BooleanNullable) &&
                        (right.Type == TypeKind.Boolean || right.Type == TypeKind.BooleanNullable))
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
                    if ((left.Type == TypeKind.Boolean || left.Type == TypeKind.BooleanNullable) &&
                        (right.Type == TypeKind.Boolean || right.Type == TypeKind.BooleanNullable))
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
                    if ((left.Type == TypeKind.Boolean || left.Type == TypeKind.BooleanNullable) &&
                        (right.Type == TypeKind.Boolean || right.Type == TypeKind.BooleanNullable))
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

            TypeKind binaryExpressionTypeKind = node.Type.GetTypeKind();

            QueryExpression queryExpression;

            if (left.Type == TypeKind.String && right.Type == TypeKind.String && binaryOperationType == BinaryOperationType.Add)
            {
                IEnumerable<QueryExpression> arguments = GetConcatArguments(left, right);

                queryExpression = new FunctionCall(FunctionType.Concat, binaryExpressionTypeKind, arguments);
            }
            else
            {
                queryExpression = new BinaryQueryExpression(left, right, binaryExpressionTypeKind, binaryOperationType);
            }

            _ElementsStack.Push(queryExpression);

            return node;
        }

        private IEnumerable<QueryExpression> GetConcatArguments(params QueryExpression[] values)
        {
            foreach (QueryExpression expression in values)
            {
                FunctionCall functionCall = expression as FunctionCall;

                if (functionCall != null)
                {
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
            return node.Value as IQueryable != null;
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
                    case nameof(Nullable<int>.HasValue):
                        BinaryQueryExpression binaryExpression =
                            new BinaryQueryExpression(source, QueryConstant.CreateNull(), TypeKind.Boolean, BinaryOperationType.Equal);
                        _ElementsStack.Push(binaryExpression);
                        return;
                    case nameof(Nullable<int>.Value):
                        TypeKind typeToConvertTo = node.Expression.Type.GetGenericArguments()[0].GetTypeKind();
                        FunctionCall functionCall = new FunctionCall(FunctionType.Convert, typeToConvertTo, source);
                        _ElementsStack.Push(functionCall);
                        return;
                }
            }
            else if (node.Expression.Type == typeof(string))
            {
                switch (node.Member.Name)
                {
                    case nameof(string.Length):
                        FunctionCall functionCall = new FunctionCall(FunctionType.Length, TypeKind.Int32, source);
                        _ElementsStack.Push(functionCall);
                        return;
                }
            }
            else if (node.Expression.Type == typeof(DateTime) || node.Expression.Type == typeof(DateTimeOffset))
            {
                FunctionType? functionType;

                if (node.Expression.Type == typeof(DateTime))
                {
                    functionType = GetDateTimeFunctionTypeByMemberName(node.Member.Name);
                }
                else
                {
                    functionType = GetDateTimeOffsetFunctionTypeByMemberName(node.Member.Name);
                }

                if (functionType.HasValue)
                {
                    FunctionCall functionCall = new FunctionCall(functionType.Value, TypeKind.Int32, source);
                    _ElementsStack.Push(functionCall);
                    return;
                }
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,Resources.Strings.MemberIsNotSupported, node.Member.Name));            
        }
        private FunctionType? GetDateTimeOffsetFunctionTypeByMemberName(string memberName)
        {
            switch (memberName)
            {
                case nameof(DateTimeOffset.Day):
                    return FunctionType.Day;
                case nameof(DateTimeOffset.Month):
                    return FunctionType.Month;
                case nameof(DateTimeOffset.Year):
                    return FunctionType.Year;
                case nameof(DateTimeOffset.Hour):
                    return FunctionType.Hour;
                case nameof(DateTimeOffset.Minute):
                    return FunctionType.Minute;
                case nameof(DateTimeOffset.Second):
                    return FunctionType.Second;
                case nameof(DateTimeOffset.Millisecond):
                    return FunctionType.Millisecond;
                default:
                    return null;
            }
        }

        private FunctionType? GetDateTimeFunctionTypeByMemberName(string memberName)
        {
            switch (memberName)
            {
                case nameof(DateTime.Day):
                    return FunctionType.Day;
                case nameof(DateTime.Month):
                    return FunctionType.Month;
                case nameof(DateTime.Year):
                    return FunctionType.Year;
                case nameof(DateTime.Hour):
                    return FunctionType.Hour;
                case nameof(DateTime.Minute):
                    return FunctionType.Minute;
                case nameof(DateTime.Second):
                    return FunctionType.Second;
                case nameof(DateTime.Millisecond):
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
            if (node.Method.DeclaringType == typeof(Queryable))
            {
                switch (node.Method.Name)
                {
                    case nameof(Queryable.Select):
                        ConvertSelect(node.Arguments);
                        break;
                    case nameof(Queryable.Where):
                        ConvertPredicate(node.Arguments);
                        break;
                    case nameof(Queryable.OrderBy):
                        ConvertOrderBy(node.Arguments, OrderType.Ascending, false);
                        break;
                    case nameof(Queryable.OrderByDescending):
                        ConvertOrderBy(node.Arguments, OrderType.Descending, false);
                        break;
                    case nameof(Queryable.ThenBy):
                        ConvertOrderBy(node.Arguments, OrderType.Ascending, true);
                        break;
                    case nameof(Queryable.ThenByDescending):
                        ConvertOrderBy(node.Arguments, OrderType.Descending, true);
                        break;
                    case nameof(Queryable.Skip):
                        ConvertSkip(node.Arguments);
                        break;
                    case nameof(Queryable.Take):
                        ConvertTake(node.Arguments);
                        break;
                    case nameof(Queryable.Min):
                        ConvertAggregate(node.Arguments, ProjectionType.Min, node.Type);
                        break;
                    case nameof(Queryable.Max):
                        ConvertAggregate(node.Arguments, ProjectionType.Max, node.Type);
                        break;
                    case nameof(Queryable.Average):
                        ConvertAggregate(node.Arguments, ProjectionType.Average, node.Type);
                        break;
                    case nameof(Queryable.Sum):
                        ConvertAggregate(node.Arguments, ProjectionType.Sum, node.Type);
                        break;
                    case nameof(Queryable.LongCount):
                        ConvertAggregate(node.Arguments, ProjectionType.LongCount, node.Type);
                        break;
                    case nameof(Queryable.Count):
                        ConvertAggregate(node.Arguments, ProjectionType.Count, node.Type);
                        break;
                    case nameof(Queryable.Distinct):
                        ConvertDistinct(node.Arguments);
                        break;
                    default:
                        ThrowMethodNotSupported(node);
                        break;
                }
            }
            else if (node.Method.DeclaringType == typeof(Enumerable))
            {
                switch (node.Method.Name)
                {
                    case nameof(Enumerable.Select):
                        ConvertSelect(node.Arguments);
                        break;
                    case nameof(Enumerable.Where):
                        ConvertPredicate(node.Arguments);
                        break;
                    case nameof(Enumerable.OrderBy):
                        ConvertOrderBy(node.Arguments, OrderType.Ascending, false);
                        break;
                    case nameof(Enumerable.OrderByDescending):
                        ConvertOrderBy(node.Arguments, OrderType.Descending, false);
                        break;
                    case nameof(Enumerable.ThenBy):
                        ConvertOrderBy(node.Arguments, OrderType.Ascending, true);
                        break;
                    case nameof(Enumerable.ThenByDescending):
                        ConvertOrderBy(node.Arguments, OrderType.Descending, true);
                        break;
                    case nameof(Enumerable.Skip):
                        ConvertSkip(node.Arguments);
                        break;
                    case nameof(Enumerable.Take):
                        ConvertTake(node.Arguments);
                        break;
                    case nameof(Enumerable.Min):
                        ConvertAggregate(node.Arguments, ProjectionType.Min, node.Type);
                        break;
                    case nameof(Enumerable.Max):
                        ConvertAggregate(node.Arguments, ProjectionType.Max, node.Type);
                        break;
                    case nameof(Enumerable.Average):
                        ConvertAggregate(node.Arguments, ProjectionType.Average, node.Type);
                        break;
                    case nameof(Enumerable.Sum):
                        ConvertAggregate(node.Arguments, ProjectionType.Sum, node.Type);
                        break;
                    case nameof(Enumerable.LongCount):
                        ConvertAggregate(node.Arguments, ProjectionType.LongCount, node.Type);
                        break;
                    case nameof(Enumerable.Count):
                        ConvertAggregate(node.Arguments, ProjectionType.Count, node.Type);
                        break;
                    case nameof(Enumerable.Distinct):
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
                    case nameof(string.Contains):
                        ConvertStringStringFunction(node.Arguments, node.Object, node.Type, FunctionType.Contains);
                        break;
                    case nameof(string.StartsWith):
                        ConvertStringStringFunction(node.Arguments, node.Object, node.Type, FunctionType.StartsWith);
                        break;
                    case nameof(string.EndsWith):
                        ConvertStringStringFunction(node.Arguments, node.Object, node.Type, FunctionType.EndsWith);
                        break;
                    case nameof(string.Trim):
                        ConvertStringFunction(node.Object, node.Type, FunctionType.Trim);
                        break;
                    case nameof(string.ToUpper):
                        ConvertStringFunction(node.Object, node.Type, FunctionType.ToUpper);
                        break;
                    case nameof(string.ToLower):
                        ConvertStringFunction(node.Object, node.Type, FunctionType.ToLower);
                        break;
                    case nameof(string.Replace):
                        ConvertReplace(node.Arguments, node.Object, node.Type, FunctionType.Replace);
                        break;
                    case nameof(string.IndexOf):
                        ConvertIndexOf(node.Arguments, node.Object, node.Type, FunctionType.IndexOf);
                        break;
                    case nameof(string.Concat):
                        ConvertConcat(node.Arguments, node.Type, FunctionType.Concat);
                        break;
                    case nameof(string.IsNullOrEmpty):
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

        private void ConvertIsNullOrEmpty(IReadOnlyList<Expression> args, Type type, FunctionType functionType)
        {
            if (args.Count != 1)
            {
                ThrowWrongArgumentCount(args.Count, functionType);
            }

            Visit(args[0]);
            QueryExpression value = PopQueryExpression();
            CheckQueryExpressionTypeKind(value, TypeKind.String);

            TypeKind functionTypeKind = type.GetTypeKind();

            CheckTypeKind(functionTypeKind, TypeKind.Boolean);

            FunctionCall functionCall = new FunctionCall(functionType, functionTypeKind, value);

            _ElementsStack.Push(functionCall);
        }

        private void ConvertConcat(IReadOnlyList<Expression> args, Type type, FunctionType functionType)
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
                expressions = ((NewArrayExpression)args[0]).Expressions;
            }
            else
            {
                expressions = args;
            }

            foreach (Expression expression in expressions)
            {
                Visit(expression);

                QueryExpression arg = PopQueryExpression();
                CheckQueryExpressionTypeKind(arg, TypeKind.String);

                arguments.Add(arg);
            }

            TypeKind functionTypeKind = type.GetTypeKind();

            FunctionCall functionCall = new FunctionCall(functionType, functionTypeKind, GetConcatArguments(arguments.ToArray()));

            _ElementsStack.Push(functionCall);
        }

        private void ConvertIndexOf(IReadOnlyList<Expression> args, Expression source, Type type, FunctionType functionType)
        {
            Visit(source);

            QueryExpression value = PopQueryExpression();
            CheckQueryExpressionTypeKind(value, TypeKind.String);

            if (args.Count < 1 || args.Count > 3)
            {
                ThrowWrongArgumentCount(args.Count, functionType);
            }

            Visit(args[0]);
            QueryExpression arg0 = PopQueryExpression();
            CheckQueryExpressionTypeKind(arg0, TypeKind.String);

            TypeKind functionTypeKind = type.GetTypeKind();

            QueryExpression arg1 = null;

            if (args.Count > 1)
            {
                Visit(args[1]);
                arg1 = PopQueryExpression();
                CheckQueryExpressionTypeKind(arg1, TypeKind.Int32);
            }

            QueryExpression arg2 = null;

            if (args.Count > 2)
            {
                Visit(args[2]);
                arg2 = PopQueryExpression();
                CheckQueryExpressionTypeKind(arg2, TypeKind.Int32);
            }

            FunctionCall functionCall = 
                new FunctionCall(
                    functionType, 
                    functionTypeKind, 
                    Enumerate.Sequence<QueryExpression>(value,arg0,arg1,arg2).Where(arg => arg != null)
                );
            
            _ElementsStack.Push(functionCall);
        }

        private void ThrowWrongArgumentCount(int argumentCount, FunctionType functionType)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.FunctionDoesntTakeArguments, argumentCount, functionType));
        }

        private void ConvertReplace(IReadOnlyList<Expression> args, Expression source, Type type, FunctionType functionType)
        {
            Visit(source);

            QueryExpression value = PopQueryExpression();
            CheckQueryExpressionTypeKind(value, TypeKind.String);

            Visit(args[0]);

            QueryExpression arg0 = PopQueryExpression();
            CheckQueryExpressionTypeKind(arg0, TypeKind.String);

            QueryExpression arg1 = PopQueryExpression();
            CheckQueryExpressionTypeKind(arg1, TypeKind.String);

            TypeKind functionTypeKind = type.GetTypeKind();

            FunctionCall functionCall = new FunctionCall(functionType, functionTypeKind, value, arg0, arg1);

            _ElementsStack.Push(functionCall);
        }

        private void ConvertStringFunction(Expression source, Type type, FunctionType functionType)
        {
            Visit(source);

            QueryExpression valueToTrim = PopQueryExpression();
            CheckQueryExpressionTypeKind(valueToTrim, TypeKind.String);

            TypeKind functionTypeKind = type.GetTypeKind();

            CheckTypeKind(functionTypeKind, TypeKind.String);

            FunctionCall functionCall = new FunctionCall(functionType, functionTypeKind, valueToTrim);

            _ElementsStack.Push(functionCall);
        }

        private void ConvertStringStringFunction(IReadOnlyList<Expression> args, Expression source, Type type, FunctionType functionType)
        {
            Visit(source);
            Visit(args[0]);

            QueryExpression valueToSearch = PopQueryExpression();
            CheckQueryExpressionTypeKind(valueToSearch, TypeKind.String);
            QueryExpression stringToSearch = PopQueryExpression();
            CheckQueryExpressionTypeKind(stringToSearch, TypeKind.String);

            TypeKind functionTypeKind = type.GetTypeKind();

            CheckTypeKind(functionTypeKind, TypeKind.Boolean);

            FunctionCall functionCall = new FunctionCall(functionType, functionTypeKind, stringToSearch, valueToSearch);

            _ElementsStack.Push(functionCall);
        }

        private static void ThrowMethodNotSupported(MethodCallExpression methodCall)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.Strings.MethodIsNotSupported, 
                methodCall.Method.DeclaringType.FullName, methodCall.Method.Name));
        }

        private void ConvertSelect(IReadOnlyList<Expression> args)
        {
            LambdaExpression conditionExpression = (LambdaExpression)StripQuotes(args[1]);

            if (conditionExpression.Parameters.Count != 1)
            {
                throw new InvalidOperationException(Resources.Strings.OnlyLambdaWithOneParameterIsSupportedInSelectClause);
            }

            Expression source = args[0];

            Visit(source);

            RegularSelect regularSelect;

            if (conditionExpression.Body.NodeType == ExpressionType.New)
            {
                NewExpression newExpression = (NewExpression)conditionExpression.Body;

                regularSelect = ConvertNewExpression(newExpression);
            }
            else if (conditionExpression.Body.NodeType == ExpressionType.MemberInit)
            {
                MemberInitExpression memberInitExpression = (MemberInitExpression)conditionExpression.Body;

                regularSelect = ConvertMemberInitExpression(memberInitExpression);
            }
            else
            {
                Visit(conditionExpression.Body);

                QueryExpression queryExpression = PopQueryExpression();
                SourceBase select = PopSourceBase();

                ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

                MethodCallExpression methodCallExpression = Expression.Call(parameter, queryExpression.Type.GetMethodByTypeKind(), Expression.Constant(0));

                LambdaExpression lambdaExpression = Expression.Lambda(methodCallExpression, parameter);

                Column column = new Column(_AliasGenerator.GenerateAlias(),queryExpression);
                
                IDictionary<string,ColumnReference> map = new Dictionary<string,ColumnReference>(StringComparer.Ordinal);
                map.Add(string.Empty, column.GetColumnReference());

                regularSelect = new RegularSelect(
                    map.AsReadOnly(),
                    lambdaExpression.Compile(),
                    _AliasGenerator.GenerateAlias(),
                    Enumerate.Sequence(column),
                    select);
            }

            _ElementsStack.Push(regularSelect);
        }

        private RegularSelect ConvertMemberInitExpression(MemberInitExpression memberInitExpression)
        {
            IList<Column> columns = new List<Column>();
            IDictionary<string, ColumnReference> memberColumnMap = new Dictionary<string, ColumnReference>(StringComparer.Ordinal);

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

                MemberAssignment assignment = (MemberAssignment)memberBinding;

                Visit(assignment.Expression);

                QueryExpression queryExpression = PopQueryExpression();

                MethodCallExpression methodCallExpression = Expression.Call(parameter, queryExpression.Type.GetMethodByTypeKind(), Expression.Constant(i));
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
            IDictionary<string, ColumnReference> memberColumnMap = new Dictionary<string, ColumnReference>(StringComparer.Ordinal);

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

                        MethodCallExpression methodCallExpression = Expression.Call(parameter, queryExpression.Type.GetMethodByTypeKind(), Expression.Constant(i));
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

        private void ConvertAggregate(IReadOnlyList<Expression> args, ProjectionType projectionType, Type type)
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
                TypeKind columnType = type.GetTypeKind();

                IDictionary<string, ColumnReference> map = new Dictionary<string, ColumnReference>(StringComparer.Ordinal);
                map.Add(string.Empty, new ColumnReference(columnAlias, columnType));

                ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

                MethodCallExpression methodCallExpression = Expression.Call(parameter, columnType.GetMethodByTypeKind(), Expression.Constant(0));

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
                    Enumerate.Sequence(projection),
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
                TypeKind columnType = type.GetTypeKind();

                IDictionary<string, ColumnReference> map = new Dictionary<string, ColumnReference>(StringComparer.Ordinal);
                map.Add(string.Empty, new ColumnReference(columnAlias, columnType));

                ParameterExpression parameter = Expression.Parameter(typeof(IReaderWrapper));

                MethodCallExpression methodCallExpression = Expression.Call(parameter, columnType.GetMethodByTypeKind(), Expression.Constant(0));

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
                   Enumerate.Sequence(projection),
                   sourceQuery);

                _ElementsStack.Push(select);
            }
        }

        private void ConvertPredicate(IReadOnlyList<Expression> args)
        {
            LambdaExpression conditionExpression = (LambdaExpression)StripQuotes(args[1]);

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
            Expression body = ((LambdaExpression)StripQuotes(args[1])).Body;

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

            TypeKind unaryExpressionTypeKind = node.Type.GetTypeKind();

            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:                    
                    FunctionCall functionCall = new FunctionCall(FunctionType.Convert, unaryExpressionTypeKind, queryExpression);
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
                    if (queryExpression.Type == TypeKind.Boolean || queryExpression.Type == TypeKind.BooleanNullable)
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

            UnaryQueryExpression unaryQueryExpression = new UnaryQueryExpression(queryExpression, unaryExpressionTypeKind, unaryOperationType);

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
            ConstantExpression constantExpression = expression as ConstantExpression;

            if (constantExpression != null)
            {
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

                    return (SourceBase)treeElement;
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
