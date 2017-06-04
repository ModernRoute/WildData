using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq
{
    public sealed class FunctionCall : QueryExpression
    {
        public FunctionType FunctionType
        {
            get;
            private set;
        }

        public IReadOnlyList<QueryExpression> Arguments
        {
            get;
            private set;
        }

        internal FunctionCall(FunctionType functionType, TypeKind typeKind, params QueryExpression[] args)
            : this(functionType, typeKind, (IEnumerable<QueryExpression>)args) 
        {

        }

        internal FunctionCall(FunctionType functionType, TypeKind typeKind, IEnumerable<QueryExpression> arguments = null)
            : base(typeKind)
        {
            FunctionType = functionType;
            
            if (arguments == null)
            {
                Arguments = Enumerable.Empty<QueryExpression>().ToList().AsReadOnly();
                return;
            }

            Arguments = arguments.ToList().AsReadOnly();

            Arguments.ThrowIfAnyNull();
        }


        public override QueryExpressionType ExpressionType
        {
            get
            {
                return QueryExpressionType.FunctionCall;
            }
        }
    }
}
