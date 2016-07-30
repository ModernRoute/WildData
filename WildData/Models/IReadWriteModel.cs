namespace ModernRoute.WildData.Models
{
    public interface IReadWriteModel<TKey> : IReadOnlyModel<TKey>
    {
        TKey Id { get; set; }

        bool IsNew { get; set; }
    }
}
