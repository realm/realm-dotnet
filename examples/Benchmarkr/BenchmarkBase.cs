////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace Benchmarkr
{
    public struct EmployeeQuery
    {
        public string Name;

        public int MinAge;
        public int MaxAge;

        public bool IsHired;
    }

    public abstract class BenchmarkBase
    {
        internal TimeSpan PerformTest(Action<BenchmarkBase> test)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            test(this);
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        public abstract string Name { get; }

        public string Path { get; set; }

        public abstract IDisposable OpenDB();
        public abstract void DeleteDB();

        public abstract void RunInTransaction(Action action);

        public abstract void InsertObject(uint index);

        public abstract int Count(EmployeeQuery query);
        public abstract long Enumerate(EmployeeQuery query);

        protected static string NameValue(uint index)
        {
            return $"Foo{index}";
        }

        protected static bool IsHiredValue(uint index)
        {
            return index % 2 == 0;
        }

        protected static int AgeValue(uint index)
        {
            return (int)(index % 15) + 30;
        }
    }
}

