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

using System.Collections.Generic;
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
        [Ignore("??")]
        public void SmokeTest()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var (realm, userId) = await CreateRandomRealmAsync("modifications");

                var changeDetails = new List<IChangeDetails>();
                var handler = new ProxyingHandler(path => path == $"/{userId}/modifications",
                                                  details =>
                                                  {
                                                      changeDetails.Add(details);
                                                      return Task.CompletedTask;
                                                  });

                var config = await GetConfiguration(handler);

                using (var notifier = Notifier.StartAsync(config))
                using (realm)
                {
                    var obj = new IntPropertyObject { Int = 3 };
                    realm.Write(() => realm.Add(obj));

                    var containsInsertion = await EnsureChangesAsync<IntPropertyObject>(changeDetails, 1, change =>
                    {
                        return change.Insertions.Length == 1 &&
                               change.Deletions.Length == 0 &&
                               change.Modifications.Length == 0 &&
                               change.Insertions[0].CurrentIndex == 0 &&
                               change.Insertions[0].PreviousIndex == -1;
                    });

                    Assert.True(containsInsertion);

                    realm.Write(() => obj.Int = 4);

                    var containsModification = await EnsureChangesAsync<IntPropertyObject>(changeDetails, 2, change =>
                    {
                        return change.Insertions.Length == 0 &&
                               change.Deletions.Length == 0 &&
                               change.Modifications.Length == 1 &&
                               change.Modifications[0].CurrentIndex == 0 &&
                               change.Modifications[0].PreviousIndex == 0;
                    });

                    Assert.True(containsModification);

                    realm.Write(() => realm.Remove(obj));

                    var containsDeletion = await EnsureChangesAsync<IntPropertyObject>(changeDetails, 3, change =>
                    {
                        return change.Insertions.Length == 0 &&
                               change.Deletions.Length == 1 &&
                               change.Modifications.Length == 0 &&
                               change.Deletions[0].CurrentIndex == -1 &&
                               change.Deletions[0].PreviousIndex == 0;
                    });

                    Assert.True(containsDeletion);
                }
            }, timeout: 1000000);
        }
    }
}
