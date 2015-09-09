using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RealmNet;

namespace IntegrationTests
{
    public class PerformanceTests
    {
        protected string _databasePath;
        protected Realm _realm;

        [SetUp]
        public void Setup()
        {
            Realm.ActiveCoreProvider = new CoreProvider();
            _databasePath = Path.GetTempFileName();
            _realm = Realm.GetInstance(_databasePath);
        }


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


        [TestCase(100)]
        [TestCase(10000)]
        public void RawPerformanceTest(int count)
        {
            Debug.WriteLine($"Raw performance check for {count:n} entries -------------");

            var s = "String value";

            using (_realm.BeginWrite())
            {
                _realm.CreateObject<Person>();

                var gh = _realm.GetPropertyValue<GroupHandle>("TransactionGroupHandle");
                var tablePtr = gh.GetTable("Person");

                var sw = Stopwatch.StartNew();

                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    NativeTable.add_empty_row(tablePtr);
                    NativeTable.set_string(tablePtr, (IntPtr)0, (IntPtr)rowIndex, s, (IntPtr)s.Length);
                    NativeTable.set_bool(tablePtr, (IntPtr)3, (IntPtr)rowIndex, (IntPtr)1);
                }

                sw.Stop();

                Debug.WriteLine("Time spent: " + sw.Elapsed);
            }
        }
    }
}

