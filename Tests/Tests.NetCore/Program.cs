////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;

namespace Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var cmd = new CommandLineApplication(throwOnUnexpectedArg: false);
            var regex = cmd.Option("--where", "Specify a RegEx to filter test cases", CommandOptionType.SingleValue);
            var force = cmd.Option("-f | --force", "Specify whether the test should be run even if it's marked [Explicit]", CommandOptionType.NoValue);
            cmd.HelpOption("-? | -h | --help");

            cmd.OnExecute(() =>
            {
                var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
                var assembly = typeof(Program).GetTypeInfo().Assembly;
                runner.Load(assembly, new Dictionary<string, object>());

                var filter = new CustomTestFilter(regex.Value(), force.HasValue());
                cmd.Out.WriteLine($"Testing started on {assembly.Location}.");
                cmd.Out.WriteLine($"{runner.CountTestCases(filter)} tests will be run.");

                var result = runner.Run(new CustomTestListener(cmd), filter);

                var total = result.FailCount + result.PassCount + result.InconclusiveCount;
                var message = $"Tests run: {total} Passed: {result.PassCount} Failed: {result.FailCount} Inconclusive: {result.InconclusiveCount}";
                cmd.Out.WriteLine(message);

                return 0;
            });

            cmd.Execute(args);
        }
    }

    internal class CustomTestListener : ITestListener
    {
        private readonly CommandLineApplication _cmd;

        public CustomTestListener(CommandLineApplication cmd)
        {
            _cmd = cmd;
        }

        public void TestFinished(ITestResult result)
        {
            if (!result.Test.IsSuite)
            {
                var className = result.Test.ClassName?.Split('.').LastOrDefault();
                var status = result.ResultState.Status.ToString().ToUpper();
                var message = $"\t[{status}] {className}.{result.Test.Name}";
                _cmd.Out.WriteLine(message);
            }
        }

        public void TestOutput(TestOutput output)
        {
        }

        public void TestStarted(ITest test)
        {
        }
    }

    internal class CustomTestFilter : TestFilter
    {
        private readonly Regex _testRegex;
        private readonly bool _force;

        public CustomTestFilter(string regex, bool force)
        {
            if (!string.IsNullOrEmpty(regex))
            {
                _testRegex = new Regex(regex);
            }

            _force = force;
        }

        public override TNode AddToXml(TNode parentNode, bool recursive)
        {
            return parentNode.AddElement("filter");
        }

        public override bool Match(ITest test)
        {
            if (_testRegex?.IsMatch(test.FullName) != false)
            {
                // We don't want to run explicit tests
                if (!_force)
                {
                    var parent = test;
                    while (parent != null)
                    {
                        if (parent.RunState != RunState.Runnable)
                        {
                            return false;
                        }

                        parent = parent.Parent;
                    }
                }

                return true;
            }

            return false;
        }
    }
}