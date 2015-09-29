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

        [TestCase(1000000)]
        public void BindingPerformanceTest(int count)
        {
            Console.WriteLine($"Binding-based performance check for {count:n} entries -------------");

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

                Console.WriteLine("Time spent: " + sw.Elapsed);
                Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
            }
        }

        [TestCase(1000000)]
        public void RawPerformanceTest(int count)
        {
            Console.WriteLine($"Raw performance check for {count:n} entries -------------");

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

                Console.WriteLine("Time spent: " + sw.Elapsed);
                Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
            }
        }

        [TestCase(1000000)]
        public void BindingCreateObjectPerformanceTest(int count)
        {
            Console.WriteLine($"Binding-based performance check for {count:n} entries: CreateObject -------------");

            var s = "String value";

            using (_realm.BeginWrite())
            {
                var sw = Stopwatch.StartNew();

                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    var p = _realm.CreateObject<Person>();
                }

                sw.Stop();

                Console.WriteLine("Time spent: " + sw.Elapsed);
                Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
            }
        }

        [TestCase(1000000)]
        public void RawCreateObjectPerformanceTest(int count)
        {
            Console.WriteLine($"Raw performance check for {count:n} entries: add_empty_row -------------");

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
                }

                sw.Stop();

                Console.WriteLine("Time spent: " + sw.Elapsed);
                Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
            }
        }

        [TestCase(1000000)]
        public void BindingSetValuePerformanceTest(int count)
        {
            Console.WriteLine($"Binding-based performance check for {count:n} entries: Set value -------------");

            var s = "String value";

            using (_realm.BeginWrite())
            {
                var sw = Stopwatch.StartNew();

                var p = _realm.CreateObject<Person>();
                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    p.FirstName = s;
                    p.IsInteresting = true;
                }

                sw.Stop();

                Console.WriteLine("Time spent: " + sw.Elapsed);
                Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
            }
        }

        [TestCase(1000000)]
        public void RawSetValuePerformanceTest(int count)
        {
            Console.WriteLine($"Raw performance check for {count:n} entries: set_string/set_bool -------------");

            var s = "String value";

            using (_realm.BeginWrite())
            {
                _realm.CreateObject<Person>();

                var gh = _realm.GetPropertyValue<GroupHandle>("TransactionGroupHandle");
                var tablePtr = gh.GetTable("Person");

                var sw = Stopwatch.StartNew();

                NativeTable.add_empty_row(tablePtr);
                for (var rowIndex = 0; rowIndex < count; rowIndex++)
                {
                    NativeTable.set_string(tablePtr, (IntPtr)0, (IntPtr)0, s, (IntPtr)s.Length);
                    NativeTable.set_bool(tablePtr, (IntPtr)3, (IntPtr)0, (IntPtr)1);
                }

                sw.Stop();

                Console.WriteLine("Time spent: " + sw.Elapsed);
                Console.WriteLine("Kilo-iterations per second: {0:0.00}", ((count/1000) / sw.Elapsed.TotalSeconds));
            }
        }

    }
}

