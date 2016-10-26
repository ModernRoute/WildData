namespace ModernRoute.WildData.Models
{
    public interface IReadWriteModel<TKey> : IReadOnlyModel<TKey>
    {
        bool IsPersistent();
        void SetPersistent(bool value);
    }
}
