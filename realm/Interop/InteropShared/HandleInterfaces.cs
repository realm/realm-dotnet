using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealmNet.Interop
{
    public interface IRealmHandle : IDisposable
    {
        bool IsClosed { get;  }
    }

    public interface ITableHandle : IRealmHandle { }
    public interface IQueryHandle : IRealmHandle { }
    public interface IGroupHandle : IRealmHandle { }
    public interface ISharedGroupHandle : IRealmHandle { }
    public interface ISpecHandle : IRealmHandle { }
    public interface ITableViewHandle : IRealmHandle { }
}
