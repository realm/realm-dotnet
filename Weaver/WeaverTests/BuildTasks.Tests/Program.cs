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
using System.IO;
using System.Reflection;
using RealmBuildTasks;

namespace RealmTasks.Tests
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(typeof(Realms.Realm).FullName);

            var current = Assembly.GetExecutingAssembly();

            var output = Path.GetDirectoryName(current.Location);

            var task = new WeaveRealmAssembly
            {
                AssemblyName = "Tests.XamarinIOS",
                OutputDirectory = output,
                IntermediateDirectory = Path.GetFullPath(Path.Combine(output, "..", "..", "Assemblies"))
            };

            task.Execute();
        }
    }
}
