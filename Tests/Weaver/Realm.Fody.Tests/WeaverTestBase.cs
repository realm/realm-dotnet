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

using System.Collections.Generic;
using System.Linq;
using Fody;

namespace RealmWeaver
{
    extern alias realm;

    public abstract class WeaverTestBase
    {
        protected readonly List<string> _warnings = new List<string>();
        protected readonly List<string> _errors = new List<string>();
        protected readonly List<string> _messages = new List<string>();

        protected TestResult WeaveRealm(string assemblyPath)
        {
            var weaver = new realm::ModuleWeaver();

            var result = weaver.ExecuteTestRun(assemblyPath, ignoreCodes: new[] { "80131869" }, runPeVerify: false);
            _warnings.AddRange(result.Warnings.Select(m => m.Text));
            _errors.AddRange(result.Errors.Select(m => m.Text));
            _messages.AddRange(result.Messages.Where(m => m.MessageImportance?.Equals(MessageImportance.Normal) == true).Select(m => m.Text));
            return result;
        }
    }
}