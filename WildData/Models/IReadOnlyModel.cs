namespace ModernRoute.WildData.Models
{
    public interface IReadOnlyModel
    {

    }

    public interface IReadOnlyModel<TKey> : IReadOnlyModel
    {
        TKey Id { get; set; }
    }
}
