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

        [TestCase(10)]
        [TestCase(100)]
        public void SimplePerformanceTest(int count)
        {
            Debug.WriteLine($"Binding-based performance check for {count:n} entries -------------");

            using (_realm.BeginWrite())
            {
                var sw = Stopwatch.StartNew();

                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    _realm.CreateObject<Person>();
                }

                sw.Stop();

                Debug.WriteLine("Time spent: " + sw.Elapsed);
            }
        }
    }
}
