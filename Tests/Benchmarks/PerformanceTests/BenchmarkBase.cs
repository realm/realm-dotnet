////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Bogus;
using Realms;

namespace PerformanceTests
{
    public abstract class BenchmarkBase
    {
        protected readonly Faker _faker = new Faker();

        protected Realm _realm;

        static BenchmarkBase()
        {
            Randomizer.Seed = new Random(12345);

            // Store test files in the tmp directory for local development and in the current directory on CI.
            var basePath = Environment.GetEnvironmentVariable("CI") == null ? Path.GetTempPath() : Path.Combine(Directory.GetCurrentDirectory(), "tmp");
            InteropConfig.SetDefaultStorageFolder(Path.Combine(basePath, $"rt-{System.Diagnostics.Process.GetCurrentProcess().Id}"));
            Directory.CreateDirectory(InteropConfig.GetDefaultStorageFolder("No error expected here"));
        }

        [GlobalSetup]
        public void Setup()
        {
            _realm = Realm.GetInstance(Path.GetTempFileName());
            SeedData();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            CleanupCore();

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

        protected virtual void CleanupCore()
        {
        }
    }
}
