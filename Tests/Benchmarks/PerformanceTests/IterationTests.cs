////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Realms;

namespace PerformanceTests
{
    /// <summary>
    /// This suite evaluates performance of object allocation by iterating
    /// over a large results set. It's intended to diagnose regressions in
    /// case we want to restructure ownership semantics of SharedRealmHandle.
    /// </summary>
    public partial class IterationTests : BenchmarkBase
    {
        [Params(1000, 10_000)]
        public int ObjectCount { get; set; }

        protected override void SeedData()
        {
            base.SeedData();

            _realm.Write(() =>
            {
                for (var i = 0; i < ObjectCount; i++)
                {
                    _realm.Add(new ObjectWithList
                    {
                        Value = i,
                    });
                }
            });

            _realm.Dispose();
            _realm = Realm.GetInstance(_realm.Config.DatabasePath);
        }

        [Benchmark(Description = "Iterate objects")]
        public void Itearation()
        {
            foreach (var value in _realm.All<ObjectWithList>())
            {
                _ = value;
            }

            _realm.Dispose();
            _realm = Realm.GetInstance(_realm.Config.DatabasePath);
        }

        [Benchmark(Description = "Iterate objects + 100x GC.Collect")]
        public void SimpleIteration_WithGC()
        {
            var i = 0;

            foreach (var value in _realm.All<ObjectWithList>())
            {
                _ = value;
                if (i++ == ObjectCount / 100)
                {
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true);
                }
            }

            _realm.Dispose();
            _realm = Realm.GetInstance(_realm.Config.DatabasePath);
        }

        private partial class ObjectWithList : IRealmObject
        {
            public int Value { get; set; }

            public IList<string> Strings { get; } = null!;
        }
    }
}
