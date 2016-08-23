using ModernRoute.WildData.Core;
using ModernRoute.WildData.Resources;
using System;
using System.Globalization;

namespace ModernRoute.WildData.Linq
{
    public class Projection : FieldBase
    {
        public bool Distinct
        {
            get;
            private set;
        }

        public ProjectionType ProjectionType
        {
            get; 
            private set;
        }

        public Projection(string alias, ProjectionType projectionType, QueryExpression definition, bool distinct = false)
            : base(alias, definition)
        {
            ProjectionType = projectionType;
            Distinct = distinct;
        }

        public override QueryElementType ElementType
        {
            get 
            {
                return QueryElementType.Projection;
            }
        }

        public override ReturnType ColumnType
        {
            get 
            {
                if (ProjectionType == ProjectionType.Count)
                {
                    return ReturnType.Int32;
                }

                if (ProjectionType == ProjectionType.LongCount)
                {
                    return ReturnType.Int64;
                }

                if (ProjectionType != ProjectionType.Average)
                {
                    return Definition.Type;
                }

                switch (Definition.Type)
                {
                    case ReturnType.Byte:
                    case ReturnType.Int16:
                    case ReturnType.Int32:
                    case ReturnType.Int64:
                        return ReturnType.Double;
                    case ReturnType.ByteNullable:
                    case ReturnType.Int16Nullable:
                    case ReturnType.Int32Nullable:
                    case ReturnType.Int64Nullable:
                        return ReturnType.DoubleNullable;
                    case ReturnType.Float:
                    case ReturnType.FloatNullable:
                    case ReturnType.Decimal:
                    case ReturnType.DecimalNullable:
                    case ReturnType.Double:
                    case ReturnType.DoubleNullable:
                        return Definition.Type;
                    default:
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.CurrentCulture, 
                            Strings.AverageAggregationIsNotApplicableToType,
                            Definition.Type));
                }
            }
        }
    }
}
