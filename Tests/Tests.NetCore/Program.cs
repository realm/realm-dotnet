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
using System.Reflection;
using NUnitLite;
using Realms.Sync;
using Tests.Sync;

namespace Tests.NetCore
{
    public class Program
    {
        // Tokens must be passed like Tests.exe --featuretokens developer-token professional-token enterprise-token
        private const string FeatureTokensArg = "--featuretokens";

        public static int Main(string[] args)
        {
            ProcessArgs(args, out var nunitArgs);
            if (!string.IsNullOrEmpty(SyncTestHelpers.DeveloperFeatureToken))
            {
                SyncConfiguration.SetFeatureToken(SyncTestHelpers.DeveloperFeatureToken);
            }

            var autorun = new AutoRun(typeof(Program).GetTypeInfo().Assembly);
            autorun.Execute(nunitArgs);
            return 0;
        }

        private static void ProcessArgs(string[] args, out string[] nunitArgs)
        {
            var nunitList = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == FeatureTokensArg)
                {
                    SyncTestHelpers.DeveloperFeatureToken = args[++i];
                    SyncTestHelpers.ProfessionalFeatureToken = args[++i];
                    SyncTestHelpers.EnterpriseFeatureToken = args[++i];
                }
                else
                {
                    nunitList.Add(args[i]);
                }
            }

            nunitArgs = nunitList.ToArray();
        }
    }
}
