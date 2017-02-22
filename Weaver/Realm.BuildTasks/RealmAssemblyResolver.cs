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

using System;
using Mono.Cecil;

namespace RealmBuildTasks
{
    public class RealmAssemblyResolver : IAssemblyResolver
    {
        private readonly AssemblyDefinition _currentAssembly;

        public RealmAssemblyResolver(AssemblyDefinition currentAssembly)
        {
            _currentAssembly = currentAssembly;
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assemblyNameReference)
        {
            return Resolve(assemblyNameReference, new ReaderParameters());
        }

        public AssemblyDefinition Resolve(AssemblyNameReference assemblyNameReference, ReaderParameters parameters)
        {
            try
            {
                return _currentAssembly.MainModule.AssemblyResolver.Resolve(assemblyNameReference, parameters);
            }
            catch
            {
                return _currentAssembly.MainModule.AssemblyResolver.Resolve("mscorlib", parameters);
            }
        }

        public AssemblyDefinition Resolve(string fullName)
        {
            return Resolve(AssemblyNameReference.Parse(fullName));
        }

        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException(nameof(fullName));
            }

            return Resolve(AssemblyNameReference.Parse(fullName), parameters);
        }
    }
}
