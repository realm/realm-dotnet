﻿using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using RealmNet;

namespace IntegrationTests.Win32
{
    [TestFixture]
    public class Win32PerformanceTests : AbstractPerformanceTests
    {
        protected override void Setup()
        {
            Realm.ActiveCoreProvider = new CoreProvider();
        }

        protected override string GetTempDatabasePath()
        {
            return Path.GetTempFileName();
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
                    NativeTable.set_string(tablePtr, (IntPtr) 0, (IntPtr) rowIndex, s, (IntPtr) s.Length);
                    NativeTable.set_bool(tablePtr, (IntPtr) 3, (IntPtr) rowIndex, (IntPtr)1);
                }

                sw.Stop();

                Debug.WriteLine("Time spent: " + sw.Elapsed);
            }
        }
    }
}
