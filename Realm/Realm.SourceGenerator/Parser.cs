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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Realms.SourceGenerator
{
    internal class Parser
    {
        private List<RealmClassDefinition> realmClasses;
        private GeneratorExecutionContext context;

        public Parser(GeneratorExecutionContext context, List<RealmClassDefinition> realmClasses)
        {
            this.context = context;
            this.realmClasses = realmClasses;
        }

        public ParsingResults Parse()
        {
            var result = new ParsingResults();

            foreach (var (classSyntax, classSymbol) in realmClasses)
            {
                try
                {
                    if (classSymbol.HasAttribute("IgnoredAttribute"))
                    {
                        continue;
                    }

                    var classInfo = new ClassInfo();

                    if (!classSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
                    {
                        context.ReportDiagnostic(Diagnostics.ClassNotPartial(classSymbol.Name, classSyntax.GetIdentifierLocation()));
                    }

                    if (classSymbol.BaseType.SpecialType != SpecialType.System_Object)
                    {
                        context.ReportDiagnostic(Diagnostics.ClassWithBaseType(classSymbol.Name, classSyntax.GetIdentifierLocation()));
                    }

                    var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

                    var isEmbedded = classSymbol.IsEmbeddedObject();

                    if (isEmbedded && classSymbol.IsRealmObject())
                    {
                        context.ReportDiagnostic(Diagnostics.ClassUnclearDefinition(classSymbol.Name, classSyntax.GetIdentifierLocation()));
                    }

                    // General info
                    classInfo.Namespace = classSymbol.ContainingNamespace.ToDisplayString();
                    classInfo.Name = classSymbol.Name;
                    classInfo.MapTo = (string)classSymbol.GetAttributeArgument("MapToAttribute");
                    classInfo.Accessibility = classSymbol.DeclaredAccessibility;
                    classInfo.TypeSymbol = classSymbol;
                    classInfo.IsEmbedded = isEmbedded;

                    // Properties
                    var propertiesSyntax = classSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                    FillPropertyInfo(classInfo, propertiesSyntax, semanticModel);

                    if (classInfo.Properties.Count(p => p.IsPrimaryKey) > 1)
                    {
                        context.ReportDiagnostic(Diagnostics.MultiplePrimaryKeys(classInfo.Name, classSyntax.GetIdentifierLocation()));
                    }

                    SerializeDiagnostics(context, classInfo);

                    classInfo.Diagnostics.ForEach(context.ReportDiagnostic);

                    if (classInfo.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        continue;
                    }

                    result.ClassInfo.Add(classInfo);
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostics.UnexpectedError(classSymbol.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }

            return result;
        }

        private void FillPropertyInfo(ClassInfo classInfo, IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes, SemanticModel model)
        {
            foreach (var propSyntax in propertyDeclarationSyntaxes)
            {
                var propSymbol = model.GetDeclaredSymbol(propSyntax);

                if (propSymbol.HasAttribute("IgnoredAttribute"))
                {
                    continue;
                }

                var info = new PropertyInfo();

                info.Name = propSymbol.Name;
                info.Accessibility = propSymbol.DeclaredAccessibility;
                info.IsIndexed = propSymbol.HasAttribute("IndexedAttribute");
                info.IsRequired = propSymbol.HasAttribute("RequiredAttribute");
                info.IsPrimaryKey = propSymbol.HasAttribute("PrimaryKeyAttribute");
                info.MapTo = (string)propSymbol.GetAttributeArgument("MapToAttribute");
                info.Backlink = (string)propSymbol.GetAttributeArgument("BacklinkAttribute");
                info.TypeInfo = GetPropertyTypeInfo(classInfo, propSymbol, propSyntax);

                if (info.TypeInfo.IsUnsupported)
                {
                    continue;
                }

                if (!propSyntax.IsAutomaticProperty() && info.TypeInfo.SimpleType == SimpleTypeEnum.Object)
                {
                    context.ReportDiagnostic(Diagnostics.RealmObjectWithoutAutomaticProperty(classInfo.Name, info.Name, propSyntax.GetLocation()));
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
                        context.ReportDiagnostic(Diagnostics.BacklinkNotQueryable(classInfo.Name, info.Name, propSyntax.GetLocation()));
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
                        context.ReportDiagnostic(Diagnostics.BacklinkWrongRelationship(classInfo.Name, info.Name, backlinkType.Name, inversePropertyName, propSyntax.GetLocation()));
                    }
                }

                if (info.IsPrimaryKey)
                {
                    if (classInfo.IsEmbedded)
                    {
                        context.ReportDiagnostic(Diagnostics.EmbeddedObjectWithPrimaryKey(classInfo.Name, info.Name, propSyntax.GetLocation()));
                    }

                    if (!info.TypeInfo.IsSupportedPrimaryKeyType())
                    {
                        context.ReportDiagnostic(Diagnostics.PrimaryKeyWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                }

                if (info.IsIndexed && !info.TypeInfo.IsSupportedIndexType())
                {
                    context.ReportDiagnostic(Diagnostics.IndexedWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                }

                if (info.IsRequired)
                {
                    if (info.TypeInfo.NullableAnnotation != NullableAnnotation.None)
                    {
                        context.ReportDiagnostic(Diagnostics.RequiredWithNullability(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                    else if (!info.TypeInfo.IsSupportedRequiredType())
                    {
                        context.ReportDiagnostic(Diagnostics.RequiredWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
                    }
                }

                classInfo.Properties.Add(info);
            }
        }

        private PropertyTypeInfo GetPropertyTypeInfo(ClassInfo classInfo, IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertySyntax)
        {
            var propertyLocation = propertySyntax.GetLocation();
            var typeSymbol = propertySymbol.Type;
            var typeString = propertySyntax.Type.ToString();

            var propertyType = GetSingleLevelPropertyTypeInfo(typeSymbol);

            if (propertyType.IsUnsupported)
            {
                if (propertySymbol is INamedTypeSymbol namedSymbol && namedSymbol.SpecialType == SpecialType.System_DateTime)
                {
                    context.ReportDiagnostic(Diagnostics.DateTimeNotSupported(classInfo.Name, propertySymbol.Name, propertyLocation));
                }
                else if (propertySymbol.Type.Name == "List")
                {
                    context.ReportDiagnostic(Diagnostics.ListWithoutInterface(classInfo.Name, propertySymbol.Name, propertyLocation));
                }
                else
                {
                    context.ReportDiagnostic(Diagnostics.TypeNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
                }

                return propertyType;  // We are sure we can't produce more diagnostics
            }

            if (!propertyType.SupportsNullability())
            {
                context.ReportDiagnostic(Diagnostics.NullabilityNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
            }

            if (propertyType.IsRealmInteger)
            {
                var argument = (propertyType.TypeSymbol as INamedTypeSymbol).TypeArguments.Single();

                if (!argument.IsValidRealmIntgerType())
                {
                    context.ReportDiagnostic(Diagnostics.RealmIntegerTypeUnsupported(classInfo.Name, propertySymbol.Name,
                        argument.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            else if (propertyType.IsIQueryable)
            {
                var argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();

                var internalType = GetSingleLevelPropertyTypeInfo(argument);

                if (internalType.SimpleType != SimpleTypeEnum.Object)
                {
                    context.ReportDiagnostic(Diagnostics.IQueryableUnsupportedType(classInfo.Name, propertySymbol.Name, propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                if (propertySyntax.HasSetter())
                {
                    context.ReportDiagnostic(Diagnostics.BacklinkWithSetter(classInfo.Name, propertySymbol.Name, propertyLocation));
                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            else if (propertyType.IsCollection)
            {
                PropertyTypeInfo internalPropertyType;
                ITypeSymbol argument;
                bool isUnsupported = false;

                var collectionTypeString = propertyType.CollectionType.ToString();

                if (propertyType.IsDictionary)
                {
                    var dictionaryArguments = (typeSymbol as INamedTypeSymbol).TypeArguments;
                    var keyArgument = dictionaryArguments[0];
                    var valueArgument = dictionaryArguments[1];

                    if (keyArgument.SpecialType != SpecialType.System_String)
                    {
                        context.ReportDiagnostic(
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
                    argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();
                    internalPropertyType = GetSingleLevelPropertyTypeInfo(argument);

                    if (propertyType.IsSet && internalPropertyType.SimpleType == SimpleTypeEnum.Object && argument.IsEmbeddedObject())
                    {
                        context.ReportDiagnostic(Diagnostics.SetWithEmbedded(classInfo.Name, propertySymbol.Name, propertyLocation));
                        isUnsupported = true;
                    }

                    propertyType.InternalType = internalPropertyType;
                }

                if (argument.IsRealmInteger())
                {
                    context.ReportDiagnostic(Diagnostics.CollectionRealmInteger(classInfo.Name, propertySymbol.Name, collectionTypeString, propertyLocation));
                    isUnsupported = true;
                }
                else if (internalPropertyType.IsUnsupported)
                {
                    context.ReportDiagnostic(Diagnostics.CollectionUnsupportedType(classInfo.Name, propertySymbol.Name, collectionTypeString, argument.ToReadableName(), propertyLocation));
                    isUnsupported = true;
                }

                if (propertySyntax.HasSetter())
                {
                    context.ReportDiagnostic(Diagnostics.CollectionWithSetter(classInfo.Name, propertySymbol.Name, collectionTypeString, propertyLocation));
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

            return propInfo;
        }

        private static void SerializeDiagnostics(GeneratorExecutionContext context, ClassInfo classInfo)
        {
            if (!classInfo.Diagnostics.Any())
            {
                return;
            }

            var serializedJson = Diagnostics.GetSerializedDiagnostics(classInfo.Diagnostics);
            if (!string.IsNullOrEmpty(serializedJson))
            {
                // Discussion about emitting non-source files: https://github.com/dotnet/roslyn/issues/57608
                context.AddSource($"{classInfo.Name}.diagnostics", serializedJson);
            }
        }
    }

    internal record ParsingResults
    {
        public List<ClassInfo> ClassInfo { get; } = new();
    }
}
