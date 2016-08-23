namespace ModernRoute.WildData.Models
{
    public interface IReadWriteModel<TKey> : IReadOnlyModel<TKey>
    {
        bool IsNew { get; }
    }
}
