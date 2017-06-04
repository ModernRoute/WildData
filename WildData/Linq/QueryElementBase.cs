namespace ModernRoute.WildData.Linq
{
    public abstract class QueryElementBase
    {
        internal QueryElementBase() { }

        public abstract QueryElementType ElementType { get; }
    }
}
