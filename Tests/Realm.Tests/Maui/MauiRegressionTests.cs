////////////////////////////////////////////////////////////////////////////
//
// Copyright 2026 Realm Inc.
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

using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Realms.Tests.Maui
{
    /// <summary>
    /// MAUI-specific regression tests for storage path resolution, native wrapper
    /// loading, and headless-run argument detection.
    /// These tests run on all active MAUI platforms (Android, iOS, macOS/Catalyst,
    /// Windows) via the Tests.Maui headless harness.
    /// </summary>
    [Preserve(AllMembers = true)]
    [Category("MAUI")]
    public class MauiRegressionTests
    {
        // ----------------------------------------------------------------
        // Storage tests
        // ----------------------------------------------------------------
        [Test]
        public void Storage_DefaultFolder_IsNonNullAndWritable()
        {
            var folder = InteropConfig.GetDefaultStorageFolder(
                "MAUI regression: no writable storage folder found");

            Assert.That(folder, Is.Not.Null.And.Not.Empty,
                "Default storage folder must resolve to a non-empty path.");
            Assert.That(Directory.Exists(folder),
                Is.True, $"Storage folder '{folder}' must exist after resolution.");

            var probe = Path.Combine(folder, $"realm-probe-{Path.GetRandomFileName()}");
            try
            {
                File.WriteAllText(probe, "probe");
                Assert.That(File.Exists(probe), Is.True,
                    $"Storage folder '{folder}' must be writable.");
            }
            finally
            {
                if (File.Exists(probe))
                {
                    File.Delete(probe);
                }
            }
        }

        [Test]
        public void Storage_CustomFolder_OverridesDefault()
        {
            var custom = Path.Combine(
                InteropConfig.GetDefaultStorageFolder("MAUI regression: storage unavailable"),
                "custom-storage-test");

            Directory.CreateDirectory(custom);
            try
            {
                InteropConfig.SetDefaultStorageFolder(custom);
                var resolved = InteropConfig.GetDefaultStorageFolder("Should not throw");
                Assert.That(resolved, Is.EqualTo(custom),
                    "SetDefaultStorageFolder must override the auto-resolved folder.");
            }
            finally
            {
                // Reset so subsequent tests use the real default again.
                InteropConfig.SetDefaultStorageFolder(
                    InteropConfig.GetDefaultStorageFolder("MAUI regression: reset failed"));
                Directory.Delete(custom, recursive: true);
            }
        }

        // ----------------------------------------------------------------
        // Native loading test
        // ----------------------------------------------------------------
        [Test]
        public void NativeWrapper_OpenAndWriteRealm_Succeeds()
        {
            var path = Path.Combine(
                InteropConfig.GetDefaultStorageFolder("MAUI regression: storage unavailable"),
                $"maui-native-loading-{Path.GetRandomFileName()}.realm");

            try
            {
                var config = new RealmConfiguration(path);
                using var realm = Realm.GetInstance(config);
                realm.Write(() =>
                {
                    realm.Add(new IntPrimaryKeyWithValueObject { Id = 1, StringValue = "native-load-ok" });
                });

                Assert.That(realm.All<IntPrimaryKeyWithValueObject>().Count(), Is.EqualTo(1),
                    "Native wrapper must be loaded and functional: a Realm write-then-read must succeed.");
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        // ----------------------------------------------------------------
        // Headless-run argument detection tests
        // ----------------------------------------------------------------
        [Test]
        public void HeadlessArgs_WithHeadlessFlag_ReturnsTrue()
        {
            Assert.That(TestHelpers.IsHeadlessRun(new[] { "--headless" }), Is.True,
                "IsHeadlessRun must return true when '--headless' is present.");
            Assert.That(TestHelpers.IsHeadlessRun(new[] { "--labels=All", "--headless", "--result=out.xml" }), Is.True,
                "IsHeadlessRun must return true even when '--headless' is not the only argument.");
        }

        [Test]
        public void HeadlessArgs_WithoutHeadlessFlag_ReturnsFalse()
        {
            Assert.That(TestHelpers.IsHeadlessRun(System.Array.Empty<string>()), Is.False,
                "IsHeadlessRun must return false for an empty args array.");
            Assert.That(TestHelpers.IsHeadlessRun(new[] { "--labels=All", "--result=out.xml" }), Is.False,
                "IsHeadlessRun must return false when '--headless' is absent.");
        }
    }
}
