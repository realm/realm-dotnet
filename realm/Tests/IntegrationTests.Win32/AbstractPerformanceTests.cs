using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealmNet;

namespace IntegrationTests
{
    public abstract class AbstractPerformanceTests
    {
        private string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void BaseSetup()
        {
            Setup();
            _databasePath = GetTempDatabasePath();
            _realm = Realm.GetInstance(_databasePath);
        }

        protected virtual void Setup () { }
        protected abstract string GetTempDatabasePath();

        [TestCase(1000)]
        [TestCase(10000)]
        public void SimplePerformanceTest(int count)
        {
            Debug.WriteLine($"Binding-based performance check for {count:n} entries -------------");

            var s = "String value";

            using (_realm.BeginWrite())
            {
                var sw = Stopwatch.StartNew();

                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    var p = _realm.CreateObject<Person>();
                    p.FirstName = s;
                    p.IsInteresting = true;
                }

                sw.Stop();

                Debug.WriteLine("Time spent: " + sw.Elapsed);
            }
        }
    }
}
