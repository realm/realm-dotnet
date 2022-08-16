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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realms.SourceGenerator
{
    internal static class Utils
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

        public static object GetAttributeArgument(this ISymbol symbol, string attributeName)
        {
            var attribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass.Name == attributeName);
            return attribute?.ConstructorArguments[0].Value;
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

        public static bool IsAutomaticProperty(this PropertyDeclarationSyntax propertySyntax)
        {
            // This means the property has explicit getter and/or setter
            if (propertySyntax.AccessorList != null)
            {
                // Body is "classic" curly brace body
                // ExpressionBody is =>
                return !propertySyntax.AccessorList.Accessors.Any(a => a.Body != null | a.ExpressionBody != null);
            }

            // This means the body is => (propertySyntax.ExpressionBody != null)
            return false;
        }

        public static bool HasSetter(this PropertyDeclarationSyntax propertySyntax)
        {
            return propertySyntax.AccessorList?.Accessors.Any(SyntaxKind.SetAccessorDeclaration) == true;
        }

        public static Location GetIdentifierLocation(this ClassDeclarationSyntax cds)
        {
            // If we return the location on the ClassDeclarationSyntax, then the whole class will be selected.
            // "Identifier" points only to the class name
            return cds.Identifier.GetLocation();
        }

        public static string ToCodeString(this bool boolean)
        {
            return boolean.ToString().ToLower();
        }
    }
}
