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
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Realms.Server;
using Realms.Tests.Sync;

namespace Realms.Tests.Server
{
    public class ShouldHandleTests : ServerTestBase
    {
        [Test]
        public void ShouldHandle_WhenRealmExists_InvokedOnStart()
        {
            SyncTestHelpers.RunRosTestAsync(async () =>
            {
                var paths = new List<string>();
                var handler = new ProxyingHandler(path =>
                {
                    paths.Add(path);
                    return false;
                }, null);

                var (_, userId1) = await CreateRandomRealmAsync("emails");
                var (_, userId2) = await CreateRandomRealmAsync("invoices");
                var (_, userId3) = await CreateRandomRealmAsync("invoices");
                var (_, userId4) = await CreateRandomRealmAsync("deep/folder/hierarchy");

                var config = await GetConfiguration(handler);
                using (var notifier = Notifier.StartAsync(config))
                {
                    var expectedPaths = new[]
                    {
                        $"/{userId1}/emails",
                        $"/{userId2}/invoices",
                        $"/{userId3}/invoices",
                        $"/{userId4}/deep/folder/hierarchy"
                    };

                    // ROS may contain other Realms and it takes some time to go over all.
                    // This will check if we've received ShouldHandle prompt for all expected ones
                    // every 100 ms for 10 seconds.
                    var containsExpected = await TestHelpers.EnsureAsync(() => expectedPaths.All(paths.Contains),
                                                                         retryDelay: 100,
                                                                         attempts: 100); // 10 seconds

                    Assert.True(containsExpected);

                    var (_, userId5) = await CreateRandomRealmAsync("newlyaddedrealm");

                    var containsNewRealm = await TestHelpers.EnsureAsync(() => paths.Contains($"/{userId5}/newlyaddedrealm"),
                                                                         retryDelay: 100,
                                                                         attempts: 10); // 1 second

                    Assert.True(containsNewRealm);
                }
            }, timeout: 30000);
        }
    }
}
