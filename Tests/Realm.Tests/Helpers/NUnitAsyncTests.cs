////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Realms.Tests.Helpers
{
    [TestFixture, Preserve(AllMembers = true)]
    [RequiresSynchronizationContext]
    public class NUnitAsyncTests
    {
        [Test]
        public async Task Await_InsideRequiresSynchronizationContext_ContinuesOnSameThread()
        {
            var threadId = Environment.CurrentManagedThreadId;
            await Task.Delay(50);
            Assert.That(threadId, Is.EqualTo(Environment.CurrentManagedThreadId));
            await Task.Delay(50);
            Assert.That(threadId, Is.EqualTo(Environment.CurrentManagedThreadId));
        }

        [Test]
        public async Task Post_WhenYielding_ShouldExecuteRightAway()
        {
            Assert.That(SynchronizationContext.Current, Is.Not.Null);

            var i = 0;
            SynchronizationContext.Current.Post(_ => i = 1, null);
            Assert.That(i, Is.EqualTo(0));
            await Task.Yield();
            Assert.That(i, Is.EqualTo(1));
        }
    }
}
