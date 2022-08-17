////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
#if DEBUG
using Newtonsoft.Json;
#endif

namespace Realms.SourceGenerator
{
    internal static class Diagnostics
    {
        #region Errors

        public static Diagnostic UnexpectedError(string className, string message, string stackTrace)
        {
            return CreateDiagnosticError(
                "RLM001",
                "Unexpected error during source generation",
                $"There was an unexpected error during source generation of class {className}",
                Location.None,
                description: $"Exception Message: {message}. \r\nCallstack:\r\n{stackTrace}");
        }

        public static Diagnostic ClassUnclearDefinition(string className, Location location)
        {
            return CreateDiagnosticError(
                "RLM002",
                "Realm classes cannot implement multiple class interfaces",
                $"Class {className} is declared as implementing both IRealmObject and IEmbeddedObject",
                location);
        }

        public static Diagnostic ClassNotPartial(string className, Location location)
        {
            return CreateDiagnosticError(
                "RLM003",
                "Realm classes need to be defined as partial",
                $"Class {className} is a Realm class but it is not declared as partial",
                location);
        }

        public static Diagnostic ClassWithBaseType(string className, Location location)
        {
            return CreateDiagnosticError(
                "RLM004",
                "Realm classes cannot derive from other classes",
                $"{className} derives from another class and this is not yet supported",
                location);
        }

        public static Diagnostic MultiplePrimaryKeys(string className, Location location)
        {
            return CreateDiagnosticError(
                "RLM006",
                "Realm classes cannot have multiple primary keys",
                $"Class {className} has more than one property marked with [PrimaryKey].",
                location);
        }

        public static Diagnostic EmbeddedObjectWithPrimaryKey(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Embedded objects cannot have primary keys",
                $"Class {className} is an EmbeddedObject but has a primary key defined on property {propertyName}.",
                location);
        }

        public static Diagnostic IndexedWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "[Indexed] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {propertyType}.",
                location);
        }

        public static Diagnostic RequiredWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "[Required] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [Required] which is only allowed on strings or byte[] types, not on {propertyType}.",
                location);
        }

        public static Diagnostic RequiredWithNullability(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "[Required] cannot be used together with nullability annotations",
                $"{className}.{propertyName} is marked as [Required], but the type {propertyType} supports nullability annotations. " + $"Please use nullability annotations instead of the attribute.",
                location);
        }

        public static Diagnostic NullabilityNotSupported(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Nullability annotation is not valid for this type",
                $"{className}.{propertyName} has type {propertyType}, that does not support the assigned nullability annotiation.",
                location);
        }

        public static Diagnostic PrimaryKeyWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "[PrimaryKey] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [PrimaryKey] which is only allowed on integral and string types, not on {propertyType}.",
                location);
        }

        public static Diagnostic BacklinkNotQueryable(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "[Backlink] property must be of type IQueryable<Type>",
                $"{className}.{propertyName} has [Backlink] applied, but it's not IQueryable.",
                location);
        }

        public static Diagnostic BacklinkWithSetter(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "[Backlink] property cannot have a setter",
                $"{className}.{propertyName} has a setter but also has [Backlink] applied, which only supports getters.",
                location);
        }

        public static Diagnostic BacklinkWrongRelationship(string className, string propertyName, string elementType, string inversePropertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Wrong definition of inverse relationship",
                $"The property '{elementType}.{inversePropertyName}' does not constitute a link to '{className}' as described by '{className}.{propertyName}'.",
                location);
        }

        public static Diagnostic IQueryableUnsupportedType(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "IQueryable property is not a realm object",
                $"{className}.{propertyName} is of type IQueryable, but the argument is not a realm object.",
                location);
        }

        public static Diagnostic RealmIntegerTypeUnsupported(string className, string propertyName, string internalType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "RealmInteger type is not allowed",
                $"{className}.{propertyName} is a RealmInteger<{internalType}> which is not supported. The type argument can be of type byte, short, int, or long.",
                location);
        }

        public static Diagnostic CollectionRealmInteger(string className, string propertyName, string collectionType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Collections of RealmInteger are not allowed",
                $"{className}.{propertyName} is an {collectionType}<RealmInteger> which is not supported.",
                location);
        }

        public static Diagnostic CollectionUnsupportedType(string className, string propertyName, string collectionType, string elementType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Unsupported element type in collection",
                $"{className}.{propertyName} is an {collectionType} but its generic type is {elementType} which is not supported by Realm.",
                location);
        }

        public static Diagnostic CollectionWithSetter(string className, string propertyName, string collectionType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Collections cannot have setters",
                $"{className}.{propertyName} has a setter but its type is a {collectionType} which only supports getters.",
                location);
        }

        public static Diagnostic DictionaryWithNonStringKeys(string className, string propertyName, string keyType, string valueType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Dictionary can only have strings as keys",
                $"{className}.{propertyName}  is a Dictionary<{keyType}, {valueType}> but only string keys are currently supported by Realm.",
                location);
        }

        public static Diagnostic SetWithEmbedded(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Embedded objects cannot be used in sets",
                $"{className}.{propertyName} is a Set<EmbeddedObject> which is not supported. Embedded objects are always unique which is why List<EmbeddedObject> already has Set semantics.",
                location);
        }

        public static Diagnostic ListWithoutInterface(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "List properties must be declared as IList",
                $"{className}.{propertyName} is declared as List which is not the correct way to declare to-many relationships in Realm. If you want to persist the collection, use the interface IList, otherwise annotate the property with the [Ignored] attribute.",
                location);
        }

        public static Diagnostic DateTimeNotSupported(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "DateTime is not supported",
                $"{className}.{propertyName} is a DateTime which is not supported - use DateTimeOffset instead.",
                location);
        }

        public static Diagnostic TypeNotSupported(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                "REALM001",
                "Type not supported",
                $"{className}.{propertyName} is of type '{propertyType}' which is not yet supported.",
                location);
        }

        #endregion

        #region Warnings

        public static Diagnostic RealmObjectWithoutAutomaticProperty(string className, string propertyName, Location location)
        {
            return CreateDiagnosticWarning(
                "REALM001",
                "RealmObject/EmbeddedObject properties usually indicate a relationship",
                $"{className}.{propertyName} is not an automatic property but its type is a RealmObject/EmbeddedObject which normally indicates a relationship.",
                location);
        }

        public static Diagnostic NotPersistedPropertyWithRealmAttributes(string className, string propertyName, Location location)
        {
            return CreateDiagnosticWarning(
                "REALM001",
                "Not persisted property with Realm attributes",
                $"{className}.{propertyName} has one or more Realm attributes applied, but it's not persisted, so those attributes will be ignored.",
                location);
        }

        #endregion

        private static Diagnostic CreateDiagnostic(string id, string title, string messageFormat, DiagnosticSeverity severity,
            Location location, string category, string description)
        {
            DiagnosticDescriptor descriptor = new(id, title, messageFormat, category, severity, isEnabledByDefault: true, description: description);

            return Diagnostic.Create(descriptor, location);
        }

        private static Diagnostic CreateDiagnosticError(string id, string title, string messageFormat,
            Location location, string category = "RealmClassGeneration", string description = null)
        {
            return CreateDiagnostic(id, title, messageFormat, DiagnosticSeverity.Error, location, category, description);
        }

        private static Diagnostic CreateDiagnosticWarning(string id, string title, string messageFormat,
            Location location, string category = "RealmClassGeneration", string description = null)
        {
            return CreateDiagnostic(id, title, messageFormat, DiagnosticSeverity.Warning, location, category, description);
        }

        public static string GetSerializedDiagnostics(IEnumerable<Diagnostic> diagnostics)
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable("NO_GENERATOR_DIAGNOSTICS") != null)
            {
                return null;
            }

            var diagnosticInfos = diagnostics.Select(Convert);
            return JsonConvert.SerializeObject(diagnosticInfos, Formatting.Indented);
#else
            return null;
#endif
        }

        public static DiagnosticInfo Convert(Diagnostic diag)
        {
            return new DiagnosticInfo
            {
                Id = diag.Id,
                Severity = diag.Severity,
                Message = diag.GetMessage(),
                Location = Convert(diag.Location),
            };
        }

        public static DiagnosticLocation Convert(this Location location)
        {
            // The +1 are necessary because line position start counting at 0
            var mapped = location.GetLineSpan();
            return new DiagnosticLocation
            {
                StartColumn = mapped.StartLinePosition.Character + 1,
                StartLine = mapped.StartLinePosition.Line + 1,
                EndColumn = mapped.EndLinePosition.Character + 1,
                EndLine = mapped.EndLinePosition.Line + 1,
            };
        }
    }

    internal class DiagnosticInfo
    {
        public string Id { get; set; }

        public DiagnosticSeverity Severity { get; set; }

        public string Message { get; set; }

        public DiagnosticLocation Location { get; set; }
    }

    internal class DiagnosticLocation
    {
        public string Path { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }
    }
}
