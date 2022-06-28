using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

            foreach (var (classSyntax, classSymbol) in scr.RealmClasses)
            {
                try
                {
                    var semanticModel = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

                    var isEmbedded = classSymbol.IsEmbeddedObject();

                    if (isEmbedded && classSymbol.IsRealmObject())
                    {
                        context.ReportDiagnostic(Diagnostics.ClassUnclearDefinition(classSymbol.Name, classSyntax.GetLocation()));
                        continue;
                    }

                    var classInfo = new ClassInfo();

                    //General info
                    classInfo.Namespace = classSymbol.ContainingNamespace.Name;
                    classInfo.Name = classSymbol.Name;
                    classInfo.MapTo = (string)classSymbol.GetAttributeArgument("MapToAttribute");
                    classInfo.Accessibility = classSymbol.DeclaredAccessibility;
                    classInfo.IsEmbedded = isEmbedded;

                    //Properties
                    var propertiesSyntax = classSyntax.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                    FillPropertyInfo(semanticModel, classInfo, propertiesSyntax);

                    var props = string.Join(Environment.NewLine, classInfo.Properties.Select(t => t.Name + " " + t.TypeInfo.ToString()));  //TODO For testing

                    if (!classInfo.Properties.Any())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.ObjectWithNoProperties(classInfo.Name, classSyntax.GetLocation()));
                    }

                    if (classInfo.Properties.Count(p => p.IsPrimaryKey) > 1)
                    {
                        classInfo.Diagnostics.Add(Diagnostics.MultiplePrimaryKeys(classInfo.Name, classSyntax.GetLocation()));
                    }

                    classInfo.Diagnostics.ForEach(context.ReportDiagnostic);

                    // In general we are collecting diagnostics as we go, we don't stop the properties extraction if we find an error
                    // I think it makes sense to give all possible diagnostics error on the first run (no need to correct and run again)

                    if (classInfo.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
                    {
                        continue;
                    }

                    //Code generation
                    var builder = new StringBuilder();

                    //TODO Do we need the copyright...?
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
                    // Add properties

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

                if (classInfo.IsEmbedded && info.IsPrimaryKey)
                {
                    classInfo.Diagnostics.Add(Diagnostics.EmbeddedObjectWithPrimaryKey(classInfo.Name, info.Name, propSyntax.GetLocation()));
                }

                if (info.IsIndexed && info.TypeInfo.IsSupportedIndexType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.IndexedWrongType(classInfo.Name, info.Name, info.TypeInfo.TypeString, propSyntax.GetLocation()));
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
                classInfo.Diagnostics.Add(Diagnostics.TypeNotSupported(classInfo.Name, propertySymbol.Name, typeString, propertyLocation));
                return propertyType;  //We are sure we can't produce more diagnostics
            }

            if (propertyType.IsRealmInteger)
            {
                var argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();

                if (!argument.IsValidRealmIntgerType())
                {
                    classInfo.Diagnostics.Add(Diagnostics.RealmIntegerTypeUnsupported(classInfo.Name, propertySymbol.Name, argument.ToReadableName(), propertyLocation));

                    return PropertyTypeInfo.Unsupported;
                }

                propertyType.InternalType = GetSingleLevelPropertyTypeInfo(argument);
            }
            if (propertyType.IsCollection)
            {
                PropertyTypeInfo internalPropertyType;
                ITypeSymbol argument;

                var collectionType = propertyType.CollectionType.ToString(); //TODO Need to change

                if (propertyType.IsDictionary)
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

                    internalPropertyType = GetSingleLevelPropertyTypeInfo(valueArgument);
                    propertyType.InternalType = internalPropertyType;
                    argument = valueArgument;
                }
                else
                {
                    //List or Set
                    argument = (typeSymbol as INamedTypeSymbol).TypeArguments.Single();
                    internalPropertyType = GetSingleLevelPropertyTypeInfo(argument);

                    if (propertyType.IsSet && internalPropertyType.ScalarType == ScalarTypeEnum.Object && argument.IsEmbeddedObject())
                    {
                        classInfo.Diagnostics.Add(Diagnostics.SetWithEmbedded(classInfo.Name, propertySymbol.Name, propertyLocation));
                    }

                    propertyType.InternalType = internalPropertyType;
                }

                if (argument.IsRealmInteger())
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionRealmInteger(classInfo.Name, propertySymbol.Name, collectionType, propertyLocation));
                }
                else if (internalPropertyType.IsUnsupported)
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionUnsupportedType(classInfo.Name, propertySymbol.Name, collectionType, argument.ToReadableName(), propertyLocation));
                }

                if (propertySyntax.AccessorList.Accessors.Any(SyntaxKind.SetAccessorDeclaration))
                {
                    classInfo.Diagnostics.Add(Diagnostics.CollectionWithSetter(classInfo.Name, propertySymbol.Name, collectionType, propertyLocation));
                }
            }

            return propertyType;
        }

        private PropertyTypeInfo GetSingleLevelPropertyTypeInfo(ITypeSymbol typeSymbol)
        {
            bool isNullable = false;
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                // If nullable, we need to get the actual type
                isNullable = true;
                typeSymbol = (typeSymbol as INamedTypeSymbol).TypeArguments.First();
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
                _ => PropertyTypeInfo.Unsupported
            };

            if (!propInfo.IsUnsupported)
            {
                propInfo.TypeSymbol = typeSymbol;
                propInfo.IsNullable = isNullable;
            }

            return propInfo;
        }
    }

    internal static class SymbolUtils
    {
        private static List<SpecialType> _validRealmIntegerArgumentTypes = new()
        {
            SpecialType.System_Byte,
            SpecialType.System_Int16,
            SpecialType.System_Int32,
            SpecialType.System_Int64
        };

        private static List<SpecialType> _validIntegerTypes = new()
        {
            SpecialType.System_Char,
            SpecialType.System_Byte,
            SpecialType.System_Int16,
            SpecialType.System_Int32,
            SpecialType.System_Int64
        };

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

        public static bool IsValidIntegerType(this ITypeSymbol symbol)
        {
            return _validIntegerTypes.Contains(symbol.SpecialType);
        }

        public static bool IsValidRealmIntgerType(this ITypeSymbol symbol)
        {
            return _validRealmIntegerArgumentTypes.Contains(symbol.SpecialType);
        }

        public static bool IsRealmInteger(this ITypeSymbol symbol)
        {
            var namedSymbol = symbol as INamedTypeSymbol;

            return namedSymbol != null && namedSymbol.IsGenericType == true
                && namedSymbol.ConstructUnboundGenericType().ToDisplayString() == "Realms.RealmInteger<>";
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

        public static string ToFullyQualifiedName(this ITypeSymbol symbol)
        {
            var symbolDisplayFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            return symbol.ToDisplayString(symbolDisplayFormat);
        }

        public static Location GetLocation(this SyntaxNode node)
        {
            return Location.Create(node.SyntaxTree, node.Span);
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
            if (classSymbol.IsRealmObject() || classSymbol.IsEmbeddedObject())
            {
                RealmClasses.Add((cds, classSymbol));
            }
        }
    }
}
