namespace RealmNet.Interop
{
    public interface IRowHandle : IRealmHandle
    {
        bool IsAttached { get; }
        long RowIndex { get; }
    }
}