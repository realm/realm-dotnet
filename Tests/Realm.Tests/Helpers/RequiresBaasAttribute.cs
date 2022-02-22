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
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Realms.Tests.Sync
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    internal class RequiresBaasAttribute : TestActionAttribute, IApplyToTest, IApplyToContext
    {
        private const string ExceptionKey = "Realm_SyncSession_Exception";
        private const string ErrorHandlerKey = "Realm_SyncSession_ErrorHandler";

        public bool EnsureNoSessionErrors { get; set; }

        public override ActionTargets Targets => ActionTargets.Test;

        public void ApplyToTest(Test test)
        {
            if (!SyncTestHelpers.HasBaas)
            {
                test.RunState = RunState.Skipped;
                test.Properties.Set(PropertyNames.SkipReason, "MongoDB Realm is not set up.");
            }
        }

        public void ApplyToContext(TestExecutionContext context)
        {
            if (context.TestCaseTimeout == 0)
            {
                // apply the default timeout for Baas tests if no timeout was defined
                context.TestCaseTimeout = 30000;
            }
        }

        public override void BeforeTest(ITest test)
        {
            Task.Run(() => SyncTestHelpers.CreateBaasAppsAsync()).Wait();

            if (EnsureNoSessionErrors)
            {
                void ErrorHandler(object _, ErrorEventArgs e)
                {
                    test.Properties.Set(ExceptionKey, e.Exception);
                }

                var @delegate = new EventHandler<ErrorEventArgs>(ErrorHandler);
                Realms.Sync.Session.Error += @delegate;
                test.Properties.Set(ErrorHandlerKey, @delegate);
            }
        }

        public override void AfterTest(ITest test)
        {
            if (EnsureNoSessionErrors)
            {
                var @delegate = (EventHandler<ErrorEventArgs>)test.Properties.Get(ErrorHandlerKey);
                Realms.Sync.Session.Error -= @delegate;
                test.Properties[ErrorHandlerKey].Clear();

                if (test.Properties.Get(ExceptionKey) is Exception exception)
                {
                    test.Properties[ExceptionKey].Clear();

                    // throwing in AfterTest will fail the test as if the test method itself threw the exception
                    throw exception;
                }
            }
        }
    }
}
