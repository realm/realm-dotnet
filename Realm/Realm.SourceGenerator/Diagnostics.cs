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

        public static Diagnostic MultiplePrimaryKeys(string className, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Realm classes cannot have multiple primary keys",
                $"Class {className} has multiple primary keys.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic DictionaryWithNonStringKeys(string className, string propertyName, string keyType, string valueType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Dictionaries can only have strings as keys",
                $"{className}.{propertyName} is a Dictionary <{keyType}, {valueType}> but only string keys are currently supported by Realm.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic IndexedWrongType(string className, string propertyName, string propertyType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "[Indexed] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {2}.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic RequiredWrongType(string className, string propertyName, string propertyType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "[Required] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [Required] which is only allowed on strings or nullable scalar types, not on {propertyType}.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic BacklinkNotQueryable(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "[Backlink] property must be of type IQueryable",
                $"{className}.{propertyName} has [Backlink] applied, but it's not IQueryable",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }
    }
}
