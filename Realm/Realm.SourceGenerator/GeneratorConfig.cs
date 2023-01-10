////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

using Microsoft.CodeAnalysis.Diagnostics;

namespace Realms.SourceGenerator
{
    internal class GeneratorConfig
    {
        public bool IgnoreObjectsNullability { get; private set; }

        public static GeneratorConfig ParseConfig(AnalyzerConfigOptions analyzerConfigOptions)
        {
            analyzerConfigOptions.TryGetValue("realm.ignore_objects_nullability", out var ignoreObjectsNullabilityString);
            var ignoreObjectsNullability =
                (string.IsNullOrEmpty(ignoreObjectsNullabilityString) || ignoreObjectsNullabilityString == "false") ? false : true;

            return new GeneratorConfig
            {
                IgnoreObjectsNullability = ignoreObjectsNullability
            };
        }
    }
}
