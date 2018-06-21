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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Fody;

namespace RealmWeaver
{
    extern alias realm;

    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1304:NonPrivateReadonlyFieldsMustBeginWithUpperCaseLetter")]
    public abstract class WeaverTestBase
    {
        protected readonly List<string> _warnings = new List<string>();
        protected readonly List<string> _errors = new List<string>();
        protected readonly AssemblyType _assemblyType;

        protected WeaverTestBase(AssemblyType assemblyType)
        {
            _assemblyType = assemblyType;
        }

        protected string WeaveRealm(string assemblyPath)
        {
            var weaver = new realm::ModuleWeaver();
            var targetPath = $"{Path.GetDirectoryName(assemblyPath)}/{Path.GetFileNameWithoutExtension(assemblyPath)}_realm.dll";
            var result = weaver.ExecuteTestRun(assemblyPath, runPeVerify: false, 
                afterExecuteCallback: module =>
                {
                    var parameters = new Mono.Cecil.WriterParameters { WriteSymbols = true };
                    module.Write(targetPath, parameters);
                });
            _warnings.AddRange(result.Warnings.Select(m => m.Text));
            _errors.AddRange(result.Errors.Select(m => m.Text));
            return targetPath;
        }

        public enum AssemblyType 
        {
            NonPCL,
            PCL
        }
    }
}