﻿////////////////////////////////////////////////////////////////////////////
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;
using Realms.Exceptions;

namespace Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class InMemoryTests : RealmTest
    {
        private InMemoryConfiguration _config;

        protected override void CustomSetUp()
        {
            base.CustomSetUp();

            _config = new InMemoryConfiguration(Guid.NewGuid().ToString());
        }

        [Test]
        public void InMemoryRealm_WhenDeleted_RemovesAuxiliaryFiles()
        {
            using (var realm = Realm.GetInstance(_config))
            {
                realm.Write(() => realm.Add(new IntPropertyObject
                {
                    Int = 42
                }));

                Assert.That(File.Exists(_config.DatabasePath));
                Assert.That(realm.All<IntPropertyObject>().Single().Int, Is.EqualTo(42));
            }

            Assert.That(File.Exists(_config.DatabasePath), Is.False);

            using (var realm = Realm.GetInstance(_config))
            {
                Assert.That(File.Exists(_config.DatabasePath));
                Assert.That(realm.All<IntPropertyObject>(), Is.Empty);
            }
        }

        [Test]
        public void InMemoryRealm_ReceivesNotifications()
        {
            AsyncContext.Run(async () =>
            {
                var tcs = new TaskCompletionSource<ChangeSet>();

                var realm = Realm.GetInstance(_config);

                try
                {
                    var query = realm.All<IntPropertyObject>();
                    query.SubscribeForNotifications((sender, changes, error) =>
                    {
                        if (changes != null)
                        {
                            tcs.TrySetResult(changes);
                        }
                        else if (error != null)
                        {
                            tcs.TrySetException(error);
                        }
                    });

                    await Task.Run(() =>
                    {
                        using (var otherRealm = Realm.GetInstance(_config))
                        {
                            otherRealm.Write(() => otherRealm.Add(new IntPropertyObject
                            {
                                Int = 42
                            }));
                        }
                    });

                    var backgroundChanges = await tcs.Task;

                    Assert.That(backgroundChanges.InsertedIndices, Is.Not.Empty);
                    Assert.That(backgroundChanges.DeletedIndices, Is.Empty);
                    Assert.That(backgroundChanges.ModifiedIndices, Is.Empty);
                    Assert.That(backgroundChanges.InsertedIndices[0], Is.EqualTo(0));
                }
                finally
                {
                    realm.Dispose();
                }
            });
        }

        [Test]
        public void InMemoryRealm_WhenMultipleInstancesOpen_DoesntDeleteData()
        {
            var first = Realm.GetInstance(_config);
            var second = Realm.GetInstance(_config);

            first.Write(() => first.Add(new IntPropertyObject
            {
                Int = 42
            }));

            Assert.That(File.Exists(_config.DatabasePath));
            Assert.That(second.All<IntPropertyObject>().Single().Int, Is.EqualTo(42));

            first.Dispose();

            Assert.That(File.Exists(_config.DatabasePath));
            Assert.That(second.All<IntPropertyObject>().Single().Int, Is.EqualTo(42));

            second.Dispose();

            Assert.That(File.Exists(_config.DatabasePath), Is.False);
        }

        [Test]
        public void InMemoryRealm_WhenGarbageCollected_DeletesData()
        {
            AsyncContext.Run(async () =>
            {
                WeakReference realmReference = null;
                new Action(() =>
                {
                    var realm = Realm.GetInstance(_config);
                    realm.Write(() => realm.Add(new IntPropertyObject
                    {
                        Int = 42
                    }));
                    realmReference = new WeakReference(realm);
                })();

                while (realmReference.IsAlive)
                {
                    await Task.Yield();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                Assert.That(realmReference.IsAlive, Is.False);
                Assert.That(File.Exists(_config.DatabasePath), Is.False);

                using (var realm = Realm.GetInstance(_config))
                {
                    Assert.That(realm.All<IntPropertyObject>(), Is.Empty);
                }
            });
        }

        [Test]
        public void InMemoryRealm_WhenEncrypted_RequiresEncryptionKey()
        {
            var encryptedConfig = new InMemoryConfiguration(_config.Identifier)
            {
                EncryptionKey = TestHelpers.GetEncryptionKey(23)
            };

            using (var realm = Realm.GetInstance(encryptedConfig))
            {
                realm.Write(() => realm.Add(new IntPropertyObject
                {
                    Int = 42
                }));

                Assert.That(() => Realm.GetInstance(_config), Throws.TypeOf<RealmMismatchedConfigException>());
            }

            Assert.That(() => Realm.GetInstance(_config), Throws.Nothing);
        }
    }
}