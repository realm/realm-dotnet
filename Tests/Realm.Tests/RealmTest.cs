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
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Logging;

using static Realms.Tests.TestHelpers;

namespace Realms.Tests
{
    [Preserve(AllMembers = true)]
    public abstract class RealmTest
    {
        private readonly ConcurrentQueue<StrongBox<Realm>> _realms = new();
        private readonly LogCategory _originalLogCategory = LogCategory.Realm;
        private readonly LogLevel _originalLogLevel = Logger.GetLogLevel(LogCategory.Realm);
        private Logger _originalLogger = null!;

        private bool _isSetup;

        protected virtual bool OverrideDefaultConfig => true;

        static RealmTest()
        {
#if !UNITY
            // Store test files in the tmp directory for local development and in the current directory on CI.
            var basePath = Environment.GetEnvironmentVariable("CI") == null ? Path.GetTempPath() : Path.Combine(Directory.GetCurrentDirectory(), "tmp");

#pragma warning disable CA1837 // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
            InteropConfig.SetDefaultStorageFolder(Path.Combine(basePath, $"rt-{System.Diagnostics.Process.GetCurrentProcess().Id}"));
#pragma warning restore CA1837 // Use Environment.ProcessId instead of Process.GetCurrentProcess().Id
            Directory.CreateDirectory(InteropConfig.GetDefaultStorageFolder("No error expected here"));
#endif
        }

        [SetUp]
        public void SetUp()
        {
            if (!_isSetup)
            {
                _originalLogger = Logger.Default;

                if (OverrideDefaultConfig)
                {
                    RealmConfiguration.DefaultConfiguration = new RealmConfiguration(Guid.NewGuid().ToString());
                }

                CustomSetUp();
                _isSetup = true;
            }
        }

        protected virtual void CustomSetUp()
        {
        }

        protected void CleanupOnTearDown(Realm realm)
        {
            _realms.Enqueue(realm);
        }

        [TearDown]
        public void TearDown()
        {
            if (_isSetup)
            {
                CustomTearDown();

                Logger.Default = _originalLogger;
                Logger.SetLogLevel(_originalLogLevel, _originalLogCategory);

#pragma warning disable CS0618 // Type or member is obsolete
                Realm.UseLegacyGuidRepresentation = false;
#pragma warning restore CS0618 // Type or member is obsolete

                _isSetup = false;
                try
                {
                    DeleteRealmWithRetries(RealmConfiguration.DefaultConfiguration);
                }
                catch
                {
                }
            }
        }

        protected virtual void CustomTearDown()
        {
            foreach (var realm in _realms)
            {
                realm.Value!.Dispose();
            }

            _realms.DrainQueue(realm =>
            {
                // TODO: this should be an assertion but fails on our migration tests due to https://github.com/realm/realm-core/issues/4605.
                // Assert.That(DeleteRealmWithRetries(realm.Config), Is.True, "Couldn't delete a Realm on teardown.");
                DeleteRealmWithRetries(realm.Config);
            });
        }

        protected static bool DeleteRealmWithRetries(RealmConfigurationBase config)
        {
            for (var i = 0; i < 100; i++)
            {
                try
                {
                    Realm.DeleteRealm(config);
                    return true;
                }
                catch
                {
                    Task.Delay(50).Wait();
                }
            }

            return false;
        }

        protected Realm GetRealm(RealmConfigurationBase? config = null)
        {
            var result = Realm.GetInstance(config);
            CleanupOnTearDown(result);
            return result;
        }

        protected Realm GetRealm(string path)
        {
            var result = Realm.GetInstance(path);
            CleanupOnTearDown(result);
            return result;
        }

        protected async Task<Realm> GetRealmAsync(RealmConfigurationBase config, int timeout = 10000, CancellationToken? cancellationToken = default)
        {
            using var cts = cancellationToken != null ? null : new CancellationTokenSource(timeout);
            try
            {
                var result = await Realm.GetInstanceAsync(config, cancellationToken ?? cts!.Token);
                CleanupOnTearDown(result);
                return result;
            }
            catch (TaskCanceledException)
            {
                if (cts?.IsCancellationRequested == true)
                {
                    throw new TimeoutException($"Timed out waiting for Realm to open after {timeout} ms");
                }

                throw;
            }
        }

        protected Realm Freeze(Realm realm)
        {
            var result = realm.Freeze();
            CleanupOnTearDown(result);
            return result;
        }
    }
}
