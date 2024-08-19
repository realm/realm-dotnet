﻿////////////////////////////////////////////////////////////////////////////
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
using System.Threading.Tasks;
using NUnit.Framework;

namespace Realms.Tests.Database
{
    public class TestHelpersTests
    {
        [Test]
        public void RunAsyncTest_ContinuesOnSameThread()
        {
            TestHelpers.RunAsyncTest(async () =>
            {
                var threadId = Environment.CurrentManagedThreadId;
                await Task.Delay(50);
                Assert.That(threadId, Is.EqualTo(Environment.CurrentManagedThreadId));
                await Task.Delay(50);
                Assert.That(threadId, Is.EqualTo(Environment.CurrentManagedThreadId));
            });
        }

        [Test]
        public void RunAsyncTest_Timeouts()
        {
            Assert.Throws<TimeoutException>(() => TestHelpers.RunAsyncTest(async () => await Task.Delay(1000), timeout: 50));
        }
    }
}
