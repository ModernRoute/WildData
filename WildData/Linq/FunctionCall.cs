using ModernRoute.WildData.Core;
using ModernRoute.WildData.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ModernRoute.WildData.Linq
{
    public class FunctionCall : QueryExpression
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

        public FunctionCall(FunctionType functionType, ReturnType returnType, params QueryExpression[] args)
            : this(functionType, returnType, (IEnumerable<QueryExpression>)args) 
        {

        }

        public FunctionCall(FunctionType functionType, ReturnType returnType, IEnumerable<QueryExpression> arguments = null)
            : base(returnType)
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
