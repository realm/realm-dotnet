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

using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    internal static class Diagnostics
    {
        private enum Id
        {
            UnexpectedError = 1,
            ClassUnclearDefinition = 2,
            ClassNotPartial = 3,
            ClassWithBaseType = 4,
            MultiplePrimaryKeys = 5,
            EmbeddedObjectWithPrimaryKey = 6,
            IndexedWrongType = 7,
            RequiredWrongType = 8,
            RequiredWithNullability = 9,
            NullabilityNotSupported = 10,
            PrimaryKeyWrongType = 11,
            BacklinkNotQueryable = 12,
            BacklinkWithSetter = 13,
            BacklinkWrongRelationship = 14,
            IQueryableUnsupportedType = 15,
            RealmIntegerTypeUnsupported = 16,
            CollectionRealmInteger = 17,
            CollectionUnsupportedType = 18,
            CollectionWithSetter = 19,
            DictionaryWithNonStringKeys = 20,
            SetWithEmbedded = 21,
            ListWithoutInterface = 22,
            DateTimeNotSupported = 23,
            TypeNotSupported = 24,
            RealmObjectWithoutAutomaticProperty = 25,
            ParentOfNestedClassIsNotPartial = 27,
            IndexedPrimaryKey = 28,
        }

        #region Errors

        public static Diagnostic UnexpectedError(string className, string message, string stackTrace)
        {
            return CreateDiagnosticError(
                Id.UnexpectedError,
                "Unexpected error during source generation",
                $"There was an unexpected error during source generation of class {className}",
                Location.None,
                description: $"Exception Message: {message}. \r\nCallstack:\r\n{stackTrace}");
        }

        public static Diagnostic ClassUnclearDefinition(string className, Location location)
        {
            return CreateDiagnosticError(
                Id.ClassUnclearDefinition,
                "Realm classes cannot implement multiple class interfaces",
                $"Class {className} is declared as implementing multiple class interfaces.A class can implement only one interface between IRealmObject, IEmbeddedObject, IAsymmetricObject.",
                location);
        }

        public static Diagnostic ClassNotPartial(string className, Location location)
        {
            return CreateDiagnosticError(
                Id.ClassNotPartial,
                "Realm classes need to be defined as partial",
                $"Class {className} is a Realm class but it is not declared as partial",
                location);
        }

        public static Diagnostic ClassWithBaseType(string className, Location location)
        {
            return CreateDiagnosticError(
                Id.ClassWithBaseType,
                "Realm classes cannot derive from other classes",
                $"{className} derives from another class and this is not yet supported",
                location);
        }

        public static Diagnostic MultiplePrimaryKeys(string className, Location location)
        {
            return CreateDiagnosticError(
                Id.MultiplePrimaryKeys,
                "Realm classes cannot have multiple primary keys",
                $"Class {className} has more than one property marked with [PrimaryKey].",
                location);
        }

        public static Diagnostic EmbeddedObjectWithPrimaryKey(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.EmbeddedObjectWithPrimaryKey,
                "Embedded objects cannot have primary keys",
                $"Class {className} is an EmbeddedObject but has a primary key defined on property {propertyName}.",
                location);
        }

        public static Diagnostic IndexedWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.IndexedWrongType,
                "[Indexed] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [Indexed] which is only allowed on integral types as well as string, bool and DateTimeOffset, not on {propertyType}.",
                location);
        }

        public static Diagnostic FullTextIndexedWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.IndexedWrongType,
                "[Indexed(IndexType.FullText)] is only allowed on string properties",
                $"{className}.{propertyName} is marked as [Indexed(IndexType.FullText)] which is only allowed on string properties, not on {propertyType}.",
                location);
        }

        public static Diagnostic IndexedModeNone(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.IndexedWrongType,
                "[Indexed(IndexType.None)] is not allowed",
                $"{className}.{propertyName} is annotated as [Indexed(IndexType.None)] which is not allowed. If you don't wish to index the property, removed the [Indexed] attribute.",
                location);
        }

        public static Diagnostic IndexPrimaryKey(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.IndexedPrimaryKey,
                "[Indexed] is not allowed in combination with [PrimaryKey]",
                $"{className}.{propertyName} is marked has both [Indexed] and [PrimaryKey] attributes which is not allowed. PrimaryKey properties are indexed by default so the [Indexed] attribute is redundant.",
                location);
        }

        public static Diagnostic RequiredWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.RequiredWrongType,
                "[Required] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [Required] which is only allowed on strings or byte[] types, not on {propertyType}.",
                location);
        }

        public static Diagnostic RequiredWithNullability(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.RequiredWithNullability,
                "[Required] cannot be used together with nullability annotations",
                $"{className}.{propertyName} is marked as [Required], but the type {propertyType} supports nullability annotations. " + $"Please use nullability annotations instead of the attribute.",
                location);
        }

        public static Diagnostic NullabilityNotSupported(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.NullabilityNotSupported,
                "Nullability annotation is not valid for this type",
                $"{className}.{propertyName} has type {propertyType}, that does not support the assigned nullability annotation.",
                location);
        }

        public static Diagnostic PrimaryKeyWrongType(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.PrimaryKeyWrongType,
                "[PrimaryKey] is only allowed on specific types",
                $"{className}.{propertyName} is marked as [PrimaryKey] which is only allowed on byte, char, short, int, long, string, ObjectId, and Guid, not on {propertyType}.",
                location);
        }

        public static Diagnostic BacklinkNotQueryable(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.BacklinkNotQueryable,
                "[Backlink] property must be of type IQueryable<Type>",
                $"{className}.{propertyName} has [Backlink] applied, but it's not IQueryable.",
                location);
        }

        public static Diagnostic BacklinkWithSetter(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.BacklinkWithSetter,
                "[Backlink] property cannot have a setter",
                $"{className}.{propertyName} has a setter but also has [Backlink] applied, which only supports getters.",
                location);
        }

        public static Diagnostic BacklinkWrongRelationship(string className, string propertyName, string elementType, string inversePropertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.BacklinkWrongRelationship,
                "Wrong definition of inverse relationship",
                $"The property '{elementType}.{inversePropertyName}' does not constitute a link to '{className}' as described by '{className}.{propertyName}'.",
                location);
        }

        public static Diagnostic IQueryableUnsupportedType(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.IQueryableUnsupportedType,
                "IQueryable property is not a realm object",
                $"{className}.{propertyName} is of type IQueryable, but the argument is not a realm object.",
                location);
        }

        public static Diagnostic RealmIntegerTypeUnsupported(string className, string propertyName, string internalType, Location location)
        {
            return CreateDiagnosticError(
                Id.RealmIntegerTypeUnsupported,
                "RealmInteger type is not allowed",
                $"{className}.{propertyName} is a RealmInteger<{internalType}> which is not supported. The type argument can be of type byte, short, int, or long.",
                location);
        }

        public static Diagnostic CollectionRealmInteger(string className, string propertyName, string collectionType, Location location)
        {
            return CreateDiagnosticError(
                Id.CollectionRealmInteger,
                "Collections of RealmInteger are not allowed",
                $"{className}.{propertyName} is an {collectionType}<RealmInteger> which is not supported.",
                location);
        }

        public static Diagnostic CollectionUnsupportedType(string className, string propertyName, string collectionType, string elementType, Location location)
        {
            return CreateDiagnosticError(
                Id.CollectionUnsupportedType,
                "Unsupported element type in collection",
                $"{className}.{propertyName} is an {collectionType} but its generic type is {elementType} which is not supported by Realm.",
                location);
        }

        public static Diagnostic CollectionWithSetter(string className, string propertyName, string collectionType, Location location)
        {
            return CreateDiagnosticError(
                Id.CollectionWithSetter,
                "Collections cannot have setters",
                $"{className}.{propertyName} has a setter but its type is a {collectionType} which only supports getters.",
                location);
        }

        public static Diagnostic DictionaryWithNonStringKeys(string className, string propertyName, string keyType, string valueType, Location location)
        {
            return CreateDiagnosticError(
                Id.DictionaryWithNonStringKeys,
                "Dictionary can only have strings as keys",
                $"{className}.{propertyName}  is a Dictionary<{keyType}, {valueType}> but only string keys are currently supported by Realm.",
                location);
        }

        public static Diagnostic SetWithEmbedded(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.SetWithEmbedded,
                "Embedded objects cannot be used in sets",
                $"{className}.{propertyName} is a Set<EmbeddedObject> which is not supported. Embedded objects are always unique which is why List<EmbeddedObject> already has Set semantics.",
                location);
        }

        public static Diagnostic ListWithoutInterface(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.ListWithoutInterface,
                "List properties must be declared as IList",
                $"{className}.{propertyName} is declared as List which is not the correct way to declare to-many relationships in Realm. If you want to persist the collection, use the interface IList, otherwise annotate the property with the [Ignored] attribute.",
                location);
        }

        public static Diagnostic DateTimeNotSupported(string className, string propertyName, Location location)
        {
            return CreateDiagnosticError(
                Id.DateTimeNotSupported,
                "DateTime is not supported",
                $"{className}.{propertyName} is a DateTime which is not supported - use DateTimeOffset instead.",
                location);
        }

        public static Diagnostic TypeNotSupported(string className, string propertyName, string propertyType, Location location)
        {
            return CreateDiagnosticError(
                Id.TypeNotSupported,
                "Type not supported",
                $"{className}.{propertyName} is of type '{propertyType}' which is not yet supported. If that is supposed to be a model class, make sure it implements IRealmObject/IEmbeddedObject/IAsymmetricObject.",
                location);
        }

        public static Diagnostic ParentOfNestedClassIsNotPartial(string className, string parentClassName, Location location)
        {
            return CreateDiagnosticError(
                Id.ParentOfNestedClassIsNotPartial,
                "Containing class of nested Realm class is not declared as partial",
                $"Class {parentClassName} contains nested Realm class {className} and needs to be declared as partial.",
                location);
        }

        #endregion

        #region Warnings

        public static Diagnostic RealmObjectWithoutAutomaticProperty(string className, string propertyName, Location location)
        {
            return CreateDiagnosticWarning(
                Id.RealmObjectWithoutAutomaticProperty,
                "RealmObject/EmbeddedObject properties usually indicate a relationship",
                $"{className}.{propertyName} is not an automatic property but its type is a RealmObject/EmbeddedObject which normally indicates a relationship.",
                location);
        }

        #endregion

        private static Diagnostic CreateDiagnostic(Id id, string title, string messageFormat, DiagnosticSeverity severity,
            Location location, string category, string? description)
        {
            var reportedId = $"RLM{(int)id:000}";
            DiagnosticDescriptor descriptor = new(reportedId, title, messageFormat, category, severity, isEnabledByDefault: true, description: description);

            return Diagnostic.Create(descriptor, location);
        }

        private static Diagnostic CreateDiagnosticError(Id id, string title, string messageFormat,
            Location location, string category = "RealmClassGeneration", string? description = null)
            => CreateDiagnostic(id, title, messageFormat, DiagnosticSeverity.Error, location, category, description);

        private static Diagnostic CreateDiagnosticWarning(Id id, string title, string messageFormat,
            Location location, string category = "RealmClassGeneration", string? description = null)
            => CreateDiagnostic(id, title, messageFormat, DiagnosticSeverity.Warning, location, category, description);
    }
}
