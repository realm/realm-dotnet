using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Realms;

namespace PerformanceTests
{
    public abstract class BenchmarkBase
    {
        protected Realm _realm;

        [GlobalSetup]
        public void Setup()
        {
            _realm = Realm.GetInstance(Path.GetTempFileName());
            SeedData();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _realm.Dispose();

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    Realm.DeleteRealm(_realm.Config);
                    break;
                }
                catch
                {
                    Task.Delay(10).Wait();
                }
            }
        }

        protected virtual void SeedData()
        {
        }
    }
}
