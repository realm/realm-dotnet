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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realms.SourceGenerator
{
    internal class Parser
    {
        private GeneratorExecutionContext _context;

        public Parser(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public ParsingResults Parse(IEnumerable<RealmClassDefinition> realmClasses)
        {
            var result = new ParsingResults();

            foreach (var (classSymbol, classDeclarations) in realmClasses)
            {
                var classInfo = new ClassInfo();

                // We tie the diagnostics to the first class declaration only.
                var firstClassDeclarationSyntax = classDeclarations.First();

                try
                {
                    if (classSymbol.HasAttribute("IgnoredAttribute"))
                    {
                        continue;
                    }

                    if (!firstClassDeclarationSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ClassNotPartial(classSymbol.Name, firstClassDeclarationSyntax.GetIdentifierLocation()));
                    }

                    if (classSymbol.BaseType.SpecialType != SpecialType.System_Object)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ClassWithBaseType(classSymbol.Name, firstClassDeclarationSyntax.GetIdentifierLocation()));
                    }

                    var isEmbedded = classSymbol.IsEmbeddedObject();

                    if (isEmbedded && classSymbol.IsRealmObject())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ClassUnclearDefinition(classSymbol.Name, firstClassDeclarationSyntax.GetIdentifierLocation()));
                    }

                    // General info
                    classInfo.Namespace = classSymbol.ContainingNamespace.ToDisplayString();
                    classInfo.Name = classSymbol.Name;
                    classInfo.MapTo = (string)classSymbol.GetAttributeArgument("MapToAttribute");
                    classInfo.Accessibility = classSymbol.DeclaredAccessibility;
                    classInfo.TypeSymbol = classSymbol;
                    classInfo.IsEmbedded = isEmbedded;

                    // Properties
                    foreach (var classDeclarationSyntax in classDeclarations)
                    {
                        var semanticModel = _context.Compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);
                        var propertiesSyntax = classDeclarationSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                        classInfo.Properties.AddRange(GetProperties(classInfo, propertiesSyntax, semanticModel));
                    }

                    if (classInfo.Properties.Count(p => p.IsPrimaryKey) > 1)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.MultiplePrimaryKeys(classInfo.Name, firstClassDeclarationSyntax.GetIdentifierLocation()));
                    }

                    result.ClassInfo.Add(classInfo);
                }
                catch (Exception ex)
                {
                    classInfo.Diagnostics.Add(Diagnostics.UnexpectedError(classSymbol.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }

            return result;
        }

        private static IEnumerable<PropertyInfo> GetProperties(ClassInfo classInfo, IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, SemanticModel model)
        {
            foreach (var propSyntax in propertyDeclarationSyntaxes)
            {
                var propSymbol = model.GetDeclaredSymbol(propSyntax);

                if (propSymbol.HasAttribute("IgnoredAttribute"))
                {
                    continue;
                }

                var info = new PropertyInfo
                {
                    Name = propSymbol.Name,
                    Accessibility = propSymbol.DeclaredAccessibility,
                    IsIndexed = propSymbol.HasAttribute("IndexedAttribute"),
                    IsRequired = propSymbol.HasAttribute("RequiredAttribute"),
                    IsPrimaryKey = propSymbol.HasAttribute("PrimaryKeyAttribute"),
                    MapTo = (string)propSymbol.GetAttributeArgument("MapToAttribute"),
                    Backlink = (string)propSymbol.GetAttributeArgument("BacklinkAttribute"),
                    Initializer = propSyntax.Initializer?.ToString(),
                    TypeInfo = GetPropertyTypeInfo(classInfo, propSymbol, propSyntax)
                };

                if (info.TypeInfo.IsUnsupported)
                {
                    continue;
                }

                if (!propSyntax.IsAutomaticProperty() && info.TypeInfo.SimpleType == SimpleTypeEnum.Object)
                {
                    classInfo.Diagnostics.Add(Diagnostics.RealmObjectWithoutAutomaticProperty(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    continue;
                }

                if (!propSyntax.HasSetter() && !info.TypeInfo.IsCollection && !info.TypeInfo.IsIQueryable)
                {
                    continue;
                }

                if (info.Backlink != null)
                {
                    if (!info.TypeInfo.IsIQueryable)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.BacklinkNotQueryable(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    }

                    var backlinkType = info.TypeInfo.InternalType.TypeSymbol;
                    var inversePropertyName = info.Backlink;
                    var inverseProperty = backlinkType.GetMembers(inversePropertyName).FirstOrDefault() as IPropertySymbol;
                    var inversePropertyTypeInfo = inverseProperty == null ? null : GetSingleLevelPropertyTypeInfo(inverseProperty.Type);

                    var isSameType = SymbolEqualityComparer.Default.Equals(inversePropertyTypeInfo?.TypeSymbol, classInfo.TypeSymbol);
                    var isCollectionOfSameType = inversePropertyTypeInfo?.IsListOrSet == true
                        && SymbolEqualityComparer.Default.Equals(inversePropertyTypeInfo?.InternalType.TypeSymbol, classInfo.TypeSymbol);

                    if (inversePropertyTypeInfo == null || (!isSameType && !isCollectionOfSameType))
                    {
                        classInfo.Diagnostics.Add(Diagnostics.BacklinkWrongRelationship(classInfo.Name, info.Name, backlinkType.Name, inversePropertyName, propSyntax.GetLocation()));
                    }
                }

                if (info.IsPrimaryKey)
                {
                    if (classInfo.IsEmbedded)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.EmbeddedObjectWithPrimaryKey(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    }

                    if (!info.TypeInfo.IsSupportedPrimaryKeyType())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.PrimaryKeyWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                }

                if (info.IsIndexed && !info.TypeInfo.IsSupportedIndexType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.IndexedWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                }

                if (info.IsRequired)
                {
                    if (info.TypeInfo.NullableAnnotation != NullableAnnotation.None)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.RequiredWithNullability(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                    else if (!info.TypeInfo.IsSupportedRequiredType())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.RequiredWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                }

                yield return info;
            }
        }

        private static PropertyTypeInfo GetPropertyTypeInfo(ClassInfo classInfo, IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertySyntax)
        {
            var propertyLocation = propertySyntax.GetLocation();
            var typeSymbol = propertySymbol.Type;
            var typeString = propertySyntax.Type.ToString();

            var propertyType = GetSingleLevelPropertyTypeInfo(typeSymbol);

            if (propertyType.IsUnsupported)
            {
                if (propertySymbol is INamedTypeSymbol namedSymbol && namedSymbol.SpecialType == SpecialType.System_DateTime)
                {
                    classInfo.Diagnostics.Add(Diagnostics.DateTimeNotSupported(classInfo.Name, propertySymbol.Name, propertyLocation));
                }
                else if (propertySymbol.Type.Name == "List")
                {
                    classInfo.Diagnostics.Add(Diagnostics.ListWithoutInterface(classInfo.Name, propertySymbol.Name, propertyLocation));
                }
                else
                {
                    classInfo.Diagnostics.Add(Diagnostics.TypeNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
                }

                return propertyType;  // We are sure we can't produce more diagnostics
            }

            if (!propertyType.HasCorrectNullabilityAnnotation())
            {
                classInfo.Diagnostics.Add(Diagnostics.NullabilityNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
            }

            if (propertyType.IsRealmInteger)
            {
                var argument = propertyType.TypeSymbol.AsNamed().TypeArguments.Single();

                if (!argument.IsValidRealmIntgerType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.RealmIntegerTypeUnsupported(classInfo.Name, propertySymbol.Name,
                        argument.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            else if (propertyType.IsIQueryable)
            {
                // TODO: these checks are a bit too restrictive as they operate on IQueryable that may not have
                // been annotated with [Backlink] - e.g. IQueryable<string> Foo { get; set; }
                // We should make sure we only return unsupported if the property is actually a backlink.
                var argument = typeSymbol.AsNamed().TypeArguments.Single();

                var internalType = GetSingleLevelPropertyTypeInfo(argument);

                if (internalType.SimpleType != SimpleTypeEnum.Object)
                {
                    classInfo.Diagnostics.Add(Diagnostics.IQueryableUnsupportedType(classInfo.Name, propertySymbol.Name, propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                if (propertySyntax.HasSetter())
                {
                    classInfo.Diagnostics.Add(Diagnostics.BacklinkWithSetter(classInfo.Name, propertySymbol.Name, propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            else if (propertyType.IsCollection)
            {
                PropertyTypeInfo internalPropertyType;
                ITypeSymbol argument;
                var isUnsupported = false;

                if (propertyType.IsDictionary)
                {
                    var dictionaryArguments = typeSymbol.AsNamed().TypeArguments;
                    var keyArgument = dictionaryArguments[0];
                    var valueArgument = dictionaryArguments[1];

                    if (keyArgument.SpecialType != SpecialType.System_String)
                    {
                        classInfo.Diagnostics.Add(
                            Diagnostics.DictionaryWithNonStringKeys(classInfo.Name, propertySymbol.Name,
                            keyArgument.ToReadableName(), valueArgument.ToReadableName(), propertyLocation));
                        isUnsupported = true;
                    }

                    internalPropertyType = GetSingleLevelPropertyTypeInfo(valueArgument);
                    propertyType.InternalType = internalPropertyType;
                    argument = valueArgument;
                }
                else
                {
                    // List or Set
                    argument = typeSymbol.AsNamed().TypeArguments.Single();
                    internalPropertyType = GetSingleLevelPropertyTypeInfo(argument);

                    if (propertyType.IsSet && internalPropertyType.SimpleType == SimpleTypeEnum.Object && argument.IsEmbeddedObject())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.SetWithEmbedded(classInfo.Name, propertySymbol.Name, propertyLocation));
                        isUnsupported = true;
                    }

                    propertyType.InternalType = internalPropertyType;
                }

                var collectionTypeString = propertyType.CollectionType.ToString();
                if (argument.IsRealmInteger())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionRealmInteger(classInfo.Name, propertySymbol.Name, collectionTypeString, propertyLocation));
                    isUnsupported = true;
                }
                else if (internalPropertyType.IsUnsupported)
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionUnsupportedType(classInfo.Name, propertySymbol.Name, collectionTypeString, argument.ToReadableName(), propertyLocation));
                    isUnsupported = true;
                }

                if (propertySyntax.HasSetter())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionWithSetter(classInfo.Name, propertySymbol.Name, collectionTypeString, propertyLocation));
                    isUnsupported = true;
                }

                if (isUnsupported)
                {
                    return PropertyTypeInfo.Unsupported;
                }
            }

            return propertyType;
        }

        private static PropertyTypeInfo GetSingleLevelPropertyTypeInfo(ITypeSymbol typeSymbol)
        {
            var completeTypeSymbol = typeSymbol;
            var nullableAnnotation = typeSymbol.NullableAnnotation;
            if (nullableAnnotation == NullableAnnotation.Annotated)
            {
                if (typeSymbol.Name == "Nullable")
                {
                    typeSymbol = (typeSymbol as INamedTypeSymbol).TypeArguments.First();
                }
                else
                {
                    // This happens only when nullability annotations are enabled
                    typeSymbol = typeSymbol.OriginalDefinition;
                }
            }

            PropertyTypeInfo propInfo = typeSymbol switch
            {
                INamedTypeSymbol when typeSymbol.IsRealmInteger() => PropertyTypeInfo.RealmInteger,
                INamedTypeSymbol when typeSymbol.IsValidIntegerType() => PropertyTypeInfo.Int,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Boolean => PropertyTypeInfo.Bool,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Single => PropertyTypeInfo.Float,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Double => PropertyTypeInfo.Double,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_String => PropertyTypeInfo.String,
                INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Decimal || typeSymbol.Name == "Decimal128" => PropertyTypeInfo.Decimal,
                ITypeSymbol when typeSymbol.ToDisplayString() == "byte[]" => PropertyTypeInfo.Data,
                INamedTypeSymbol when typeSymbol.Name == "ObjectId" => PropertyTypeInfo.ObjectId,
                INamedTypeSymbol when typeSymbol.Name == "Guid" => PropertyTypeInfo.Guid,
                INamedTypeSymbol when typeSymbol.Name == "DateTimeOffset" => PropertyTypeInfo.Date,
                INamedTypeSymbol when typeSymbol.Name == "RealmValue" => PropertyTypeInfo.RealmValue,
                INamedTypeSymbol when typeSymbol.IsRealmObjectBase() => PropertyTypeInfo.Object,
                INamedTypeSymbol when typeSymbol.Name == "IList" => PropertyTypeInfo.List,
                INamedTypeSymbol when typeSymbol.Name == "ISet" => PropertyTypeInfo.Set,
                INamedTypeSymbol when typeSymbol.Name == "IDictionary" => PropertyTypeInfo.Dictionary,
                INamedTypeSymbol when typeSymbol.Name == "IQueryable" => PropertyTypeInfo.IQueryable,
                _ => PropertyTypeInfo.Unsupported
            };

            propInfo.TypeSymbol = typeSymbol;
            propInfo.CompleteTypeSymbol = completeTypeSymbol;
            propInfo.NullableAnnotation = nullableAnnotation;
            propInfo.Namespace = typeSymbol.ContainingNamespace?.ToString();

            return propInfo;
        }
    }

    internal record ParsingResults
    {
        public List<ClassInfo> ClassInfo { get; } = new();
    }
}
