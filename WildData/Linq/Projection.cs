using ModernRoute.WildData.Core;
using ModernRoute.WildData.Resources;
using System;
using System.Globalization;

namespace ModernRoute.WildData.Linq
{
    public sealed class Projection : FieldBase
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

        internal Projection(string alias, ProjectionType projectionType, QueryExpression definition, bool distinct = false)
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

        public override TypeKind ColumnType
        {
            get 
            {
                if (ProjectionType == ProjectionType.Count)
                {
                    return TypeKind.Int32;
                }

                if (ProjectionType == ProjectionType.LongCount)
                {
                    return TypeKind.Int64;
                }

                if (ProjectionType != ProjectionType.Average)
                {
                    return Definition.Type;
                }

                switch (Definition.Type)
                {
                    case TypeKind.Byte:
                    case TypeKind.Int16:
                    case TypeKind.Int32:
                    case TypeKind.Int64:
                        return TypeKind.Double;
                    case TypeKind.ByteNullable:
                    case TypeKind.Int16Nullable:
                    case TypeKind.Int32Nullable:
                    case TypeKind.Int64Nullable:
                        return TypeKind.DoubleNullable;
                    case TypeKind.Float:
                    case TypeKind.FloatNullable:
                    case TypeKind.Decimal:
                    case TypeKind.DecimalNullable:
                    case TypeKind.Double:
                    case TypeKind.DoubleNullable:
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
