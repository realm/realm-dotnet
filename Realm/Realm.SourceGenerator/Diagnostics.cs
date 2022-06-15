// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using Microsoft.CodeAnalysis;

namespace Realm.SourceGenerator
{
    internal static class Diagnostics
    {
        private static readonly DiagnosticDescriptor _multiplePrimaryKeysDescription = new 
            ("REALM001", 
            "Realm classes cannot have multiple primary keys", 
            "Class {0} has multiple primary keys", 
            "RealmClassGeneration",
            DiagnosticSeverity.Error, 
            true);

        public static Diagnostic MultiplePrimaryKeys(string className, Location location)
        {
            return Diagnostic.Create(_multiplePrimaryKeysDescription, location, className);
        }


    }
}
