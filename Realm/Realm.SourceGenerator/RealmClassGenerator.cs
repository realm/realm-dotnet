﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Realms;
using Realms.Schema;

namespace Realm.SourceGenerator
{
    [Generator]
    public class RealmClassGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxContextReceiver scr)
            {
                return;
            }

            //TODO Need to make an error for using both IRealmObject and IEmbeddedObject (or none)

            foreach (var (classSyntax, classSymbol) in scr.RealmClasses)
            {
                try
                {
                    //TODO Need to check if needed at some point
                    var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

                    var classInfo = new ClassInfo();

                    //General info
                    classInfo.Namespace = classSymbol.ContainingNamespace.Name;
                    classInfo.Name = classSymbol.Name;
                    classInfo.MapTo = (string)classSymbol.GetAttributeArgument<MapToAttribute>();
                    classInfo.Accessibility = classSymbol.DeclaredAccessibility;

                    //Properties
                    var propertiesSyntax = classSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                    //var propertiesSymbol = classSymbol.GetMembers()
                    //    .OfType<IPropertySymbol>().Where(p => !p.HasAttribute<IgnoredAttribute>()).ToList(); // TODO ToList is here for debugging purposes

                    FillPropertyInfo(semanticModel, classInfo, propertiesSyntax);

                    classInfo.Diagnostics.ForEach(context.ReportDiagnostic);

                    if (classInfo.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        continue;
                    }

                    var builder = new StringBuilder();

                    //TODO Do we need the copyRight...?
                    builder.Append(@"// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the ""License"")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an ""AS IS"" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////");

                    //Usings
                    builder.AppendLine(@"
using System;");


                    builder.AppendLine(@$"
namespace {classInfo.Namespace}
{{
    {classInfo.Accessibility.ToDisplayString()} partial class {classInfo.Name} : IRealmObject
    {{
");
                    foreach (var prop in classInfo.Properties)
                    {
                        builder.AppendLine($"// {prop.TypeInfo.TypeString} {prop.Name}");
                        builder.AppendLine($"// {prop.TypeInfo.Type}");
                        builder.AppendLine();
                    }

                    builder.AppendLine(@$"
    }}
}}");
                    // We could use this, but we're adding time to compilation
                    // It's not a full format, but it just normalizes whitespace
                    // var formattedSourceText = CSharpSyntaxTree.ParseText(builder.ToString(), encoding: Encoding.UTF8).GetRoot().NormalizeWhitespace().SyntaxTree.GetText();
                    var stringCode = builder.ToString();
                    var sourceText = SourceText.From(stringCode, Encoding.UTF8);
                    context.AddSource($"{classInfo.Name}_generated.cs", sourceText);

                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostics.UnexpectedError(classSymbol.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }
        }

        private void FillPropertyInfo(SemanticModel model, ClassInfo classInfo, IEnumerable<PropertyDeclarationSyntax> propertyDeclarationSyntaxes)
        {
            bool primaryKeySet = false;

            foreach (var propSyntax in propertyDeclarationSyntaxes)
            {
                var propSymbol = model.GetSymbolInfo(propSyntax).Symbol as IPropertySymbol;

                if (propSymbol.HasAttribute<IgnoredAttribute>())
                {
                    continue;
                }

                var info = new PropertyInfo();

                info.Name = propSymbol.Name;
                info.Accessibility = propSymbol.DeclaredAccessibility;
                info.IsIndexed = propSymbol.HasAttribute<IndexedAttribute>();
                info.IsRequired = propSymbol.HasAttribute<RequiredAttribute>();
                info.IsPrimaryKey = propSymbol.HasAttribute<PrimaryKeyAttribute>();
                info.MapTo = (string)propSymbol.GetAttributeArgument<MapToAttribute>();
                info.Backlink = (string)propSymbol.GetAttributeArgument<BacklinkAttribute>();
                info.TypeInfo = GetTypeInfo(classInfo, propSymbol, propSyntax);

                if (info.IsPrimaryKey)
                {
                    if (primaryKeySet)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.MultiplePrimaryKeys("test", Location.None));
                    }

                    primaryKeySet = true;
                }

                classInfo.Properties.Add(info);
            }
        }

        private TypeInfo GetTypeInfo(ClassInfo classInfo, IPropertySymbol propertySymbol, PropertyDeclarationSyntax propertySyntax)
        {
            var propertyLocation = propertySymbol.GetPropertyLocation();
            var typeSymbol = propertySymbol.Type;
            var typeString = typeSymbol.ToReadableName();


            var propertyType = GetPropertyTypeNew(typeSymbol);

            if (propertyType.IsUnsupported())
            {
                classInfo.Diagnostics.Add(Diagnostics.TypeNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
            }

            if (propertyType.IsCollection(out var collectionType))
            {
                PropertyType internalPropertyType = PropertyTypeUtils.Unsupported;
                ITypeSymbol argument = null;
                if (propertyType.IsDictionary())
                {
                    var dictionaryArguments = (typeSymbol as INamedTypeSymbol).TypeArguments;
                    var keyArgument = dictionaryArguments[0];
                    var valueArgument = dictionaryArguments[1];

                    if (keyArgument.SpecialType != SpecialType.System_String)
                    {
                        classInfo.Diagnostics.Add(
                            Diagnostics.DictionaryWithNonStringKeys(classInfo.Name, propertySymbol.Name,
                            keyArgument.ToReadableName(), valueArgument.ToReadableName(), propertyLocation));
                    }

                    internalPropertyType = GetPropertyTypeNew(valueArgument);
                    argument = valueArgument;
                }
                else
                {
                    //List or Set
                    argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();
                    internalPropertyType = GetPropertyTypeNew(argument);

                    //TODO Not sure why this is not there for Dictionaries in PropertyTypeEx
                    if (internalPropertyType.HasFlag(PropertyType.Object))
                    {
                        if (propertyType.IsSet() && argument.IsEmbeddedObject())
                        {
                            classInfo.Diagnostics.Add(Diagnostics.SetWithEmbedded(classInfo.Name, propertySymbol.Name, propertyLocation));
                        }

                        // List/Set<Object> can't contain nulls
                        internalPropertyType &= ~PropertyType.Nullable;
                    }
                }

                if (argument.IsRealmInteger())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionRealmInteger(classInfo.Name, propertySymbol.Name, collectionType, propertyLocation));
                }
                else if (internalPropertyType.IsUnsupportedCollectionType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionUnsupportedType(classInfo.Name, propertySymbol.Name, collectionType, argument.ToReadableName(), propertyLocation));
                }

                if (propertySyntax.AccessorList.Accessors.Any(SyntaxKind.SetAccessorDeclaration))
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionWithSetter(classInfo.Name, propertySymbol.Name, collectionType, propertyLocation));
                }

                propertyType |= internalPropertyType;
            }


            var info = new TypeInfo
            {
                Type = propertyType,
                TypeString = typeString,
            };

            return info;
        }

        private PropertyType GetPropertyTypeNew(ITypeSymbol typeSymbol)
        {
            PropertyType nullabilityModifier = default;
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                // If nullable, we need to get the actual type
                nullabilityModifier = PropertyType.Nullable;
                typeSymbol = (typeSymbol as INamedTypeSymbol).TypeArguments.First();
            }

            switch (typeSymbol)
            {
                case INamedTypeSymbol when typeSymbol.IsIntegerType():
                    return PropertyType.Int | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Boolean:
                    return PropertyType.Bool | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Single:
                    return PropertyType.Float | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Double:
                    return PropertyType.Double | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_String:
                    return PropertyType.NullableString;
                case INamedTypeSymbol when typeSymbol.SpecialType == SpecialType.System_Decimal || typeSymbol.Name == "Decimal128":
                    return PropertyType.Decimal | nullabilityModifier;
                case ITypeSymbol when typeSymbol.ToDisplayString() == "byte[]":
                    return PropertyType.NullableData;
                case INamedTypeSymbol when typeSymbol.Name == "ObjectId":
                    return PropertyType.ObjectId | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.Name == "Guid":
                    return PropertyType.Guid | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.Name == "DateTimeOffset":
                    return PropertyType.Date | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.Name == "RealmValue":
                    return PropertyType.RealmValue | nullabilityModifier;
                case INamedTypeSymbol when typeSymbol.IsRealmObjectBase():
                    return PropertyType.Object | PropertyType.Nullable;
                case INamedTypeSymbol when typeSymbol.Name == "IList":
                    return PropertyType.Array;
                case INamedTypeSymbol when typeSymbol.Name == "ISet":
                    return PropertyType.Set;
                case INamedTypeSymbol when typeSymbol.Name == "IDictionary":
                    return PropertyType.Dictionary;
                default:
                    return PropertyTypeUtils.Unsupported;
            }
        }
    }

    internal static class SymbolUtils
    {
        public static bool HasAttribute(this ISymbol symbol, string attributeName)
        {
            return symbol.GetAttributes().Any(a => a.AttributeClass.Name == attributeName);
        }

        // TODO We could remove this and use directly strings, it would be more efficient probably
        public static bool HasAttribute<T>(this ISymbol symbol) where T : Attribute
        {
            var attributeName = typeof(T).Name;
            return symbol.HasAttribute(attributeName);
        }

        public static object GetAttributeArgument(this ISymbol symbol, string attributeName)
        {
            var attribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == attributeName);
            return attribute?.NamedArguments[0].Value.Value;
        }

        public static object GetAttributeArgument<T>(this ISymbol symbol) where T : Attribute
        {
            var attributeName = typeof(T).Name;
            return symbol.GetAttributeArgument(attributeName);
        }

        public static bool IsIntegerType(this ITypeSymbol symbol)
        {
            var enumIntegers = new List<SpecialType> { SpecialType.System_Byte, SpecialType.System_Char, SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64 };

            return enumIntegers.Contains(symbol.SpecialType) || symbol.IsRealmInteger();
        }

        public static bool IsRealmInteger(this ITypeSymbol symbol)
        {
            return true; //TODO Need to complete
        }

        public static bool IsRealmObjectBase(this ITypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "IRealmObjectBase");
        }

        public static bool IsRealmObject(this ITypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "IRealmObject");
        }

        public static bool IsEmbeddedObject(this ITypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "IEmbeddedObject");
        }

        public static string ToDisplayString(this Accessibility acc)
        {
            return acc switch
            {
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.Public => "public",
                _ => throw new ArgumentException("Unrecognised accessibilty")
            };
        }

        public static string ToReadableName(this ITypeSymbol symbol)
        {
            //Better to have it in one place, in case we want to modify how it looks

            // This has also the complete namespace
            // We can use also ToMinimalDisplayString, but it requires the semantic model;
            return symbol.ToDisplayString();
        }

        public static Location GetPropertyLocation(this IPropertySymbol symbol)
        {
            var syntax = symbol.DeclaringSyntaxReferences.First();  //TODO Need to test if really we have only one reference for properties. What if part of an interface?
            return Location.Create(syntax.SyntaxTree, syntax.Span);
        }


    }

    internal static class PropertyTypeUtils
    {
        public static PropertyType Unsupported = (PropertyType)5000;

        public static bool IsList(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Array);

        public static bool IsSet(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Set);

        public static bool IsDictionary(this PropertyType propertyType) => propertyType.HasFlag(PropertyType.Dictionary);

        public static bool IsCollection(this PropertyType propertyType, out string collectionType)
        {
            if (propertyType.IsList())
            {
                collectionType = "IList";
                return true;
            }

            if (propertyType.IsSet())
            {
                collectionType = "ISet";
                return true;
            }

            if (propertyType.IsDictionary())
            {
                collectionType = "IDictionary";
                return true;
            }

            collectionType = null;
            return false;
        }

        public static bool IsUnsupported(this PropertyType propertyType) => propertyType == Unsupported;

        public static bool IsUnsupportedCollectionType(this PropertyType propertyType)
        {
            return propertyType.IsCollection(out _) || propertyType.IsUnsupported();
        }
    }


    internal class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public IList<(ClassDeclarationSyntax, ITypeSymbol)> RealmClasses { get; } = new List<(ClassDeclarationSyntax, ITypeSymbol)>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax cds)
            {
                return;
            }

            var classSymbol = context.SemanticModel.GetDeclaredSymbol(cds) as ITypeSymbol;

            //This looks for the interfaces of the base class too (recursively)
            if (!classSymbol.IsRealmObjectBase())
            {
                return;
            }

            RealmClasses.Add((cds, classSymbol));


        }
    }
}