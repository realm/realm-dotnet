////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Server;
using Realms.Tests.Database;
using Realms.Tests.Sync;

namespace Realms.Tests.Server
{
    public class NotifierIntegrationTests : ServerTestBase
    {
        [Test]
        public void SmokeTest()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var (realm, userId) = await CreateRandomRealmAsync("modifications");

                var changeDetails = new List<ChangeInfo>();
                var handler = new ProxyingHandler(path => path == $"/{userId}/modifications",
                                                  details =>
                                                  {
                                                      var lastChange = details.Changes.Single();
                                                      Assert.That(lastChange.Key, Is.EqualTo(nameof(IntPropertyObject)));

                                                      changeDetails.Add(new ChangeInfo
                                                      {
                                                          Insertions = lastChange.Value.Insertions.Select(o => (int)o.Int).ToArray(),
                                                          Modifications = lastChange.Value.Modifications.Select(o => ((int)o.CurrentObject.Int, (int)o.PreviousObject.Int, o.ChangedProperties.ToArray())).ToArray(),
                                                          Deletions = lastChange.Value.Deletions.Select(o => (int)o.Int).ToArray()
                                                      });
                                                      return Task.CompletedTask;
                                                  });

                var config = await GetConfiguration(handler);

                using (var notifier = await Notifier.StartAsync(config))
                using (realm)
                {
                    var obj = new IntPropertyObject { Int = 3 };
                    realm.Write(() => realm.Add(obj));

                    var insertChange = await WaitForChangeAsync(changeDetails, 1);
                    Assert.That(insertChange.Modifications.Length, Is.Zero);
                    Assert.That(insertChange.Deletions.Length, Is.Zero);

                    Assert.That(insertChange.Insertions.Length, Is.EqualTo(1));
                    Assert.That(insertChange.Insertions.Single(), Is.EqualTo(obj.Int));

                    realm.Write(() => obj.Int = 4);

                    var modifyChange = await WaitForChangeAsync(changeDetails, 2);
                    Assert.That(modifyChange.Insertions.Length, Is.Zero);
                    Assert.That(modifyChange.Deletions.Length, Is.Zero);

                    Assert.That(modifyChange.Modifications.Length, Is.EqualTo(1));
                    Assert.That(modifyChange.Modifications.Single().PreviousValue, Is.EqualTo(insertChange.Insertions.Single()));
                    Assert.That(modifyChange.Modifications.Single().CurrentValue, Is.EqualTo(obj.Int));
                    Assert.That(modifyChange.Modifications.Single().ModifiedProperties, Is.EquivalentTo(new[] { nameof(IntPropertyObject.Int) }));

                    realm.Write(() => realm.Remove(obj));

                    var deleteChange = await WaitForChangeAsync(changeDetails, 3);
                    Assert.That(deleteChange.Insertions.Length, Is.Zero);
                    Assert.That(deleteChange.Modifications.Length, Is.Zero);

                    Assert.That(deleteChange.Deletions.Length, Is.EqualTo(1));
                    Assert.That(deleteChange.Deletions.Single(), Is.EqualTo(modifyChange.Modifications.Single().CurrentValue));
                }
            }, timeout: 1000000);
        }

        private static Task<ChangeInfo> WaitForChangeAsync(IList<ChangeInfo> changeDetails, int expectedCount)
        {
            return TestHelpers.WaitForConditionAsync(() =>
            {
                if (changeDetails.Count != expectedCount)
                {
                    return null;
                }

                return changeDetails.Last();
            }, c => c != null);
        }

        private class ChangeInfo
        {
            public int[] Insertions { get; set; }

            public (int CurrentValue, int PreviousValue, string[] ModifiedProperties)[] Modifications { get; set; }

            public int[] Deletions { get; set; }
        }
    }
}
