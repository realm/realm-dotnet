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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Realms.Tests
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal class RequiresSynchronizationContextAttribute : TestActionAttribute
    {
        private const string SynchronizationContextKey = "Realm_SynchronizationContext";

        private static readonly Type NUnitSynchronizationContext =
            typeof(NUnitAttribute).Assembly.GetType("NUnit.Framework.Internal.SingleThreadedTestSynchronizationContext");

        public override ActionTargets Targets => ActionTargets.Test;

        public override void BeforeTest(ITest test)
        {
            if (IsAsyncTest(test))
            {
                /*
                 * NUnit's support for async tests allows it to "await" them in a SynchronizationContext-friendly way.
                 * The SynchronizationContext implementations they support are the Windows Forms, WPF, and their own
                 * "SingleThreadedTestSynchronizationContext". It's enough to install their SynchronizationContext
                 * before the test runs for the test executor to pick it up and await the returned Task in it.
                 * This is different from Nito's AsyncContext where we explicitly had to "enter" the context,
                 * not just installing it.
                 * Because of this, posting work on the SynchronizationContext only executes it in async tests
                 * where NUnit awaits the Task returned by the test method. Synchronous "void" tests won't await
                 * and run the posted work so we don't install a SynchronizationContext for them.
                */

                if (SynchronizationContext.Current is SynchronizationContext current)
                {
                    test.Properties.Set(SynchronizationContextKey, current);
                }

                var syncContext = (SynchronizationContext)Activator.CreateInstance(NUnitSynchronizationContext, TimeSpan.FromSeconds(10));
                SynchronizationContext.SetSynchronizationContext(syncContext);
            }
        }

        public override void AfterTest(ITest test)
        {
            if (test.Properties.Get(SynchronizationContextKey) is SynchronizationContext current)
            {
                SynchronizationContext.SetSynchronizationContext(current);
                test.Properties[SynchronizationContextKey].Clear();
            }
        }

        private static bool IsAsyncTest(ITest test) => test.Method.ReturnType.Type.GetMethod(nameof(Task.GetAwaiter)) != null;
    }
}
