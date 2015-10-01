namespace RealmNet
{
    public interface IRowHandle : IRealmHandle
    {
        bool IsAttached { get; }
        long RowIndex { get; }
    }
}