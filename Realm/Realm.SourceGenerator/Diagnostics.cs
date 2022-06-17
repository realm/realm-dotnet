﻿// ////////////////////////////////////////////////////////////////////////////
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
        #region Errors

        public static Diagnostic UnexpectedError(string className, string message, string stackTrace)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Unexpcted error during source generation",
                $"There was an unexpected error during source generation of class {className}",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true,
                description: $"Exception Message: {message}. \r\nCallstack:\r\n{stackTrace}"
                );

            return Diagnostic.Create(descriptor, Location.None);
        }

        public static Diagnostic ObjectWithNoProperties(string className, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Realm objects need to have properties",
                $"Class {className} is a Realm object but has no persisted properties",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic MultiplePrimaryKeys(string className, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Realm classes cannot have multiple primary keys",
                $"Class {className} has more than one property marked with [PrimaryKey].",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic EmbeddedObjectWithPrimaryKey(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Embedded objects cannot have primary keys",
                $"Class {className} is an EmbeddedObject but has a primary key {propertyName} defined.",
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
                $"{className}.{propertyName} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {propertyType}.",
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
                $"{className}.{propertyName} has [Backlink] applied, but it's not IQueryable.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic BacklinkWithSetter(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "[Backlink] property cannot have a setter",
                $"{className}.{propertyName} has a setter but also has [Backlink] applied, which only supports getters.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic BacklinkWrongRelationship(string className, string propertyName, string elementType, string inversePropertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Wrong definition of inverse relationship",
                $"The property '{elementType}.{inversePropertyName}' does not constitute a link to '{className}' as described by '{className}.{propertyName}'.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic CollectionRealmInteger(string className, string propertyName, string collectionType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Collections of RealmInteger are not allowed",
                $"{className}.{propertyName} is an {collectionType}<RealmInteger> which is not supported.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic CollectionUnsupportedType(string className, string propertyName, string collectionType, string elementType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Unsupported element type in collection",
                $"{className}.{propertyName} is an {collectionType} but its generic type is {elementType} which is not supported by Realm.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic CollectionWithSetter(string className, string propertyName, string collectionType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Collections cannot have setters",
                $"{className}.{propertyName} has a setter but its type is a {collectionType} which only supports getters.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic DictionaryWithNonStringKeys(string className, string propertyName, string keyType, string valueType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Dictionary can only have strings as keys",
                $"{className}.{propertyName}  is a Dictionary<{keyType}, {valueType}> but only string keys are currently supported by Realm.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic SetWithEmbedded(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Embedded objects cannot be used in sets",
                $"{className}.{propertyName} is a Set<EmbeddedObject> which is not supported. Embedded objects are always unique which is why List<EmbeddedObject> already has Set semantics.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic ListWithoutInterface(string className, string propertyName, string genericType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "List properties must be declared as IList",
                $"{className}.{propertyName} is declared as List<{genericType}> which is not the correct way to declare to-many relationships in Realm. If you want to persist the collection, use the interface IList<{genericType}>, otherwise annotate the property with the [Ignored] attribute.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);
            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic DateTimeNotSupported(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "DateTime is not supported",
                $"{className}.{propertyName} is a DateTime which is not supported - use DateTimeOffset instead.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic NullableDateTimeNotSupported(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "DateTime? is not supported",
                $"{className}.{propertyName} is a DateTime? which is not supported - use DateTimeOffset? instead.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic TypeNotSupported(string className, string propertyName, string propertyType, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Type not supported",
                $"{className}.{propertyName} is a is a '{propertyType}' which is not yet supported.",
                "RealmClassGeneration",
                DiagnosticSeverity.Error,
                true);

            return Diagnostic.Create(descriptor, location);
        }


        #endregion

        #region Warnings

        public static Diagnostic RealmObjectWithoutAutomaticProperty(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "RealmObject/EmbeddedObject properties usually indicate a relationship",
                $"{className}.{propertyName} is not an automatic property but its type is a RealmObject/EmbeddedObject which normally indicates a relationship.",
                "RealmClassGeneration",
                DiagnosticSeverity.Warning,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        public static Diagnostic NotPersistedPropertyWithRealmAttributes(string className, string propertyName, Location location)
        {
            DiagnosticDescriptor descriptor = new
                ("REALM001",
                "Not persisted property with Realm attributes",
                $"{className}.{propertyName} has one or more Realm attributes applied, but it's not persisted, so those attributes will be ignored.",
                "RealmClassGeneration",
                DiagnosticSeverity.Warning,
                true);

            return Diagnostic.Create(descriptor, location);
        }

        #endregion
    }
}