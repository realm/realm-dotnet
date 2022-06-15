using System;
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

            var builder = new StringBuilder();

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

            foreach (var (classSyntax, classSymbol) in scr.RealmClasses)
            {
                //TODO Need to check if needed at some point
                var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

                //Namespace
                var namespaceSymbol = classSymbol.ContainingNamespace;
                var namespaceName = namespaceSymbol.Name;

                //Class name
                var className = classSymbol.Name;

                //var properties = classSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>();
                var propertiesSymbol = classSymbol.GetMembers()
                    .OfType<IPropertySymbol>().Where( p => !p.HasAttribute<IgnoredAttribute>()).ToList(); // TODO ToList is here for debugging purposes

                var extractedInfo = ExtractPropertyInfo(context, propertiesSymbol);

                //TODO Not necessarily the class is public, need to get correct visibility
                builder.AppendLine(@$"
namespace {namespaceName}
{{
    public partial class {className} : IRealmObject
    {{
");
                foreach (var prop in extractedInfo)
                {
                    builder.AppendLine($"// {prop.TypeInfo.TypeString} {prop.Name}");
                    builder.AppendLine($"// {prop.TypeInfo.Type}");
                    builder.AppendLine();
                }


                builder.AppendLine(@$"
    }}
}}");
                // We could use this, but we're adding time to compilation
                // var formattedSourceText = CSharpSyntaxTree.ParseText(builder.ToString(), encoding: Encoding.UTF8).GetRoot().NormalizeWhitespace().SyntaxTree.GetText();
                var stringCode = builder.ToString();
                var sourceText = SourceText.From(stringCode, Encoding.UTF8);
                context.AddSource($"{className}_generated.cs", sourceText);
            }

        }

        private IEnumerable<PropertyInfo> ExtractPropertyInfo(GeneratorExecutionContext context, IEnumerable<IPropertySymbol> propList)
        {
            var infoList = new List<PropertyInfo>();

            bool primaryKeySet = false;

            foreach (var propSymbol in propList)
            {
                var info = new PropertyInfo();

                info.Name = propSymbol.Name;
                info.IsIndexed = propSymbol.HasAttribute<IndexedAttribute>();
                info.IsRequired = propSymbol.HasAttribute<RequiredAttribute>();
                info.IsPrimaryKey = propSymbol.HasAttribute<PrimaryKeyAttribute>();
                info.MapTo = (string)propSymbol.GetAttributeArgument<MapToAttribute>();
                info.Backlink = (string)propSymbol.GetAttributeArgument<BacklinkAttribute>();
                info.TypeInfo = ExtractTypeInfo(propSymbol.Type);

                if (info.IsPrimaryKey)
                {
                    if (primaryKeySet)
                    {
                        context.ReportDiagnostic(Diagnostics.MultiplePrimaryKeys("test", Location.None));
                    }

                    primaryKeySet = true;
                }

                infoList.Add(info);
            }



            //TODO This is where I need to check it's a valid property
            // For instance: primary keys / indexed can only be of certain types

            return infoList;
        }

        private TypeInfo ExtractTypeInfo(ITypeSymbol typeSymbol)
        {
            var propertyType = ExtractPropertyType(typeSymbol, out var objectType);
            var typeString = typeSymbol.ToDisplayString(); // This has also the complete namespace
            // We can use also ToMinimalDisplayString, but it requires the semantic model too;

            var info = new TypeInfo
            {
                Type = propertyType,
                TypeString = typeString,
            };

            return info;
        }

        private PropertyType ExtractPropertyType(ITypeSymbol typeSymbol, out ITypeSymbol objectTypeSymbol)
        {
            objectTypeSymbol = null;
            PropertyType nullabilityModifier = default;
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                // If nullable, we need to get the actual type
                nullabilityModifier = PropertyType.Nullable;
                typeSymbol = (typeSymbol as INamedTypeSymbol).TypeArguments.First();
            }

            switch (typeSymbol)
            {
                case INamedTypeSymbol when typeSymbol.IsRealmInteger():
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
                case INamedTypeSymbol when typeSymbol.IsRealmObject():
                    objectTypeSymbol = typeSymbol;
                    return PropertyType.Object | PropertyType.Nullable;
                case INamedTypeSymbol when typeSymbol.Name == "IList":
                    var listArgument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();
                    var listResult = PropertyType.Array | ExtractPropertyType(listArgument, out objectTypeSymbol);

                    if (listResult.HasFlag(PropertyType.Object))
                    {
                        // List<Object> can't contain nulls
                        listResult &= ~PropertyType.Nullable;
                    }

                    return listResult;
                case INamedTypeSymbol when typeSymbol.Name == "ISet":
                    var setArgument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();
                    var setResult = PropertyType.Set | ExtractPropertyType(setArgument, out objectTypeSymbol);

                    if (setResult.HasFlag(PropertyType.Object))
                    {
                        // Set<Object> can't contain nulls
                        setResult &= ~PropertyType.Nullable;
                    }

                    return setResult;
                case INamedTypeSymbol when typeSymbol.Name == "IDictionary":
                    var dictionaryArguments = (typeSymbol as INamedTypeSymbol).TypeArguments;
                    var keyArgument = dictionaryArguments[0];
                    var valueArgument = dictionaryArguments[1];

                    if (keyArgument.SpecialType != SpecialType.System_String)
                    {
                        throw new Exception(); //TODO This needs to be different...
                    }

                    var dictionaryResult = PropertyType.Dictionary | ExtractPropertyType(valueArgument, out objectTypeSymbol);

                    if (dictionaryResult.HasFlag(PropertyType.Object))
                    {
                        // Set<Object> can't contain nulls
                        dictionaryResult &= ~PropertyType.Nullable;
                    }

                    return dictionaryResult;
                default:
                    break;
            }

            return PropertyType.Float;
        }
    }

    internal record PropertyInfo
    {
        public bool IsIndexed { get; set; }

        public bool IsRequired { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsNullable { get; set; }

        public string MapTo { get; set; }

        public string Backlink { get; set; }

        public TypeInfo TypeInfo { get; set; }

        public string Name { get; set; }
    }

    internal record TypeInfo
    {
        public PropertyType Type { get; set; }

        public string TypeString { get; set; }

    }

    internal static class Utils
    {
        public static bool HasAttribute(this ISymbol symbol, string attributeName)
        {
            return symbol.GetAttributes().Any(a => a.AttributeClass.Name == attributeName);
        }

        // TODO We could remove this and use directly strings, it would be more efficient probably
        public static bool HasAttribute<T>(this ISymbol symbol) where T: Attribute
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

        public static bool IsRealmInteger(this ITypeSymbol symbol)
        {
            //TODO Need to consider case of RealmIntegers and can save list of special type somewhere
            var enumIntegers = new List<SpecialType> { SpecialType.System_Byte, SpecialType.System_Char, SpecialType.System_Int16, SpecialType.System_Int32, SpecialType.System_Int64 };

            return enumIntegers.Contains(symbol.SpecialType);
        }

        public static bool IsRealmObject(this ITypeSymbol symbol)
        {
            return symbol.AllInterfaces.Any(i => i.Name == "IRealmObjectBase");
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
            if (!classSymbol.IsRealmObject())
            {
                return;
            }

            RealmClasses.Add((cds, classSymbol));


        }
    }
}
