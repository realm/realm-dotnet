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
using System.Reflection;
using RealmBuildTasks;

namespace RealmTasks.Tests
{
    internal class MainClass
    {
        public static void Main(string[] args)
        {
            var current = Assembly.GetExecutingAssembly();

            var output = CopyTestProject(Path.GetDirectoryName(current.Location));

            var task = new WeaveRealmAssembly
            {
                AssemblyName = "Tests.XamarinIOS",
                OutputDirectory = output,
                IntermediateDirectory = output,
                LogDebug = Console.WriteLine
            };

            var success = task.Execute();
            Console.WriteLine($"Task executed. Result: {success}");
        }

        private static string CopyTestProject(string currentFolder)
        {
            var pathSegments = new List<string>
            {
                currentFolder
            };

            for (var i = 0; i < 5; i++)
            {
                pathSegments.Add("..");
            }

            pathSegments.AddRange(new[]
            {
                "Tests",
                "Tests.XamarinIOS",
                "bin",
                "iPhoneSimulator",
                "Debug"
            });

            var iosTestOutput = Path.GetFullPath(Path.Combine(pathSegments.ToArray()));
            var outputCopy = Path.Combine(currentFolder, "temp");

            if (Directory.Exists(outputCopy))
            {
                Directory.Delete(outputCopy, recursive: true);
            }

            Directory.CreateDirectory(outputCopy);

            foreach (var file in Directory.EnumerateFiles(iosTestOutput))
            {
                File.Copy(file, file.Replace(iosTestOutput, outputCopy));
            }

            return outputCopy;
        }
    }
}
