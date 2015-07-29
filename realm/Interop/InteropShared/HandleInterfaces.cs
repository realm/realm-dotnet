using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealmNet.Interop
{
    public interface IHandle : IDisposable
    {
        bool IsClosed { get;  }
    }

    public interface ITableHandle : IHandle { }
    public interface IQueryHandle : IHandle { }
    public interface IGroupHandle : IHandle { }
    public interface ISharedGroupHandle : IHandle { }
    public interface ISpecHandle : IHandle { }
    public interface ITableViewHandle : IHandle { }
}
