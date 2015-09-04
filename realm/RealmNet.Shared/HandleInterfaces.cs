using System;

namespace RealmNet
{
    public interface IRealmHandle : IDisposable
    {
        bool IsClosed { get;  }
        bool IsInvalid { get; }
    }

    public interface ITableHandle : IRealmHandle { }
    public interface IQueryHandle : IRealmHandle { }
    public interface IGroupHandle : IRealmHandle { }
    public interface ISpecHandle : IRealmHandle { }
    public interface ITableViewHandle : IRealmHandle { }
}
