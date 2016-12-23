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
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Internal;

namespace IntegrationTests
{
    #if !WINDOWS
    internal class TestRunner
    {
        internal static void Run(string title, Stream outputStream)
        {
            var suite = new TestSuite(title);
            var builder = new NUnitLiteTestAssemblyBuilder();
            suite.Add(builder.Build(System.Reflection.Assembly.GetExecutingAssembly(), new Dictionary<string, object>()));

            var testExecutionContext = TestExecutionContext.CurrentContext;
            testExecutionContext.WorkDirectory = Environment.CurrentDirectory;

            #if __IOS__
            var workItem = suite.CreateWorkItem(TestFilter.Empty, new FinallyDelegate());
            #else
            var workItem = suite.CreateWorkItem(TestFilter.Empty);
            #endif
            workItem.Execute(testExecutionContext);

            var testWriter = new NUnitLite.Runner.NUnit2XmlOutputWriter(DateTime.Now);
            using (var writer = new StreamWriter(outputStream))
            {
                testWriter.WriteResultFile(workItem.Result, writer);
            }
        }
    }
    #endif
}