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

using System.Linq;
using System.Reflection;
using NUnitLite;

namespace Realms.Tests
{
    public sealed class Program
    {
        public static int Main(string[] args)
        {
            var autorun = new AutoRun(typeof(Program).GetTypeInfo().Assembly);
            var arguments = Sync.SyncTestHelpers.ExtractBaasSettings(args);

            autorun.Execute(arguments);

            var resultPath = args.FirstOrDefault(a => a.StartsWith("--result="))?.Replace("--result=", string.Empty);
            if (!string.IsNullOrEmpty(resultPath))
            {
                TestHelpers.TransformTestResults(resultPath);
            }

            return 0;
        }
    }
}