using ModernRoute.WildData.Core;
using ModernRoute.WildData.Resources;
using System;
using System.Globalization;

namespace ModernRoute.WildData.Linq
{
    public sealed class Column
    {
        public string Alias
        {
            get;
            private set;
        }

        public QueryExpression Definition
        {
            get;
            private set;
        }

        public TypeKind ColumnType
        {
            get
            {
                switch (ProjectionType)
                {
                    case ProjectionType.None:
                    case ProjectionType.Max:
                    case ProjectionType.Min:
                    case ProjectionType.Sum:
                        return Definition.Type;
                    case ProjectionType.Count:
                        return TypeKind.Int32;
                    case ProjectionType.LongCount:
                        return TypeKind.Int64;
                    case ProjectionType.Average:
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
                    default:
                        throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture,
                            Strings.ProjectionTypeIsNotSupported, ProjectionType));

                }
            }
        }

        public bool ProjectionDistinct
        {
            get;
            private set;
        }

        public ProjectionType ProjectionType
        {
            get;
            private set;
        }

        internal ColumnReference ColumnReference
        {
            get;
            private set;
        }

        internal Column(string alias, QueryExpression definition, ProjectionType projectionType = ProjectionType.None, 
            bool projectionDistinct = false)
        {
            if (alias == null)
            {
                throw new ArgumentNullException(nameof(alias));
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Alias = alias;
            Definition = definition;
            ProjectionType = projectionType;
            ProjectionDistinct = projectionDistinct;
            ColumnReference = new ColumnReference(Alias, ColumnType);
        }
    }
}
