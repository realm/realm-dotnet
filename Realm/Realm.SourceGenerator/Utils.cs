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

namespace Realms.SourceGenerator
{
    internal static class Utils
    {
        private static readonly HashSet<SpecialType> _validRealmIntegerArgumentTypes = new()
        {
            SpecialType.System_Byte,
            SpecialType.System_Int16,
            SpecialType.System_Int32,
            SpecialType.System_Int64
        };

        private static readonly HashSet<SpecialType> _validIntegerTypes = new()
        {
            SpecialType.System_Char,
            SpecialType.System_Byte,
            SpecialType.System_Int16,
            SpecialType.System_Int32,
            SpecialType.System_Int64
        };

        public static bool HasAttribute(this ISymbol symbol, string attributeName)
        {
            return symbol.GetAttributes().Any(a => a.AttributeClass?.Name == attributeName);
        }

        public static object? GetAttributeArgument(this ISymbol symbol, string attributeName, int index = 0)
        {
            var arguments = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == attributeName)?.ConstructorArguments;
            if (arguments != null && arguments.Value.Length > index)
            {
                return arguments.Value[index].Value;
            }

            return null;
        }

        public static IndexType? GetIndexType(this ISymbol symbol)
        {
            var attribute = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "IndexedAttribute");
            if (attribute == null)
            {
                return null;
            }

            if (attribute.ConstructorArguments.Length == 0)
            {
                return IndexType.General;
            }

            return (IndexType)(int)attribute.ConstructorArguments[0].Value!;
        }

        public static bool IsValidIntegerType(this ITypeSymbol symbol)
        {
            return _validIntegerTypes.Contains(symbol.SpecialType);
        }

        public static bool IsValidRealmIntegerType(this ITypeSymbol symbol)
        {
            return _validRealmIntegerArgumentTypes.Contains(symbol.SpecialType);
        }

        public static bool IsRealmInteger(this ITypeSymbol symbol)
        {
            var namedSymbol = symbol as INamedTypeSymbol;

            return namedSymbol is { IsGenericType: true }
                && namedSymbol.ConstructUnboundGenericType().ToDisplayString() == "Realms.RealmInteger<>";
        }

        public static IEnumerable<ObjectType> ImplementingObjectTypes(this ITypeSymbol symbol)
        {
            foreach (var i in symbol.Interfaces)
            {
                if (IsIRealmObjectInterface(i))
                {
                    yield return ObjectType.RealmObject;
                }
                else if (IsIEmbeddedObjectInterface(i))
                {
                    yield return ObjectType.EmbeddedObject;
                }
                else if (IsIAsymmetricObjectInterface(i))
                {
                    yield return ObjectType.AsymmetricObject;
                }
                else if (IsIMappedObjectInterface(i))
                {
                    yield return ObjectType.MappedObject;
                }
            }
        }

        public static bool IsAnyRealmObjectType(this ITypeSymbol symbol) => symbol.ImplementingObjectTypes().Any();

        public static bool IsRealmObject(this ITypeSymbol symbol) => symbol.Interfaces.Any(IsIRealmObjectInterface);

        public static bool IsEmbeddedObject(this ITypeSymbol symbol) => symbol.Interfaces.Any(IsIEmbeddedObjectInterface);

        public static bool IsAsymmetricObject(this ITypeSymbol symbol) => symbol.Interfaces.Any(IsIAsymmetricObjectInterface);

        private static bool IsIRealmObjectInterface(this INamedTypeSymbol interfaceSymbol) => interfaceSymbol.Name == "IRealmObject";

        private static bool IsIEmbeddedObjectInterface(this INamedTypeSymbol interfaceSymbol) => interfaceSymbol.Name == "IEmbeddedObject";

        private static bool IsIAsymmetricObjectInterface(this INamedTypeSymbol interfaceSymbol) => interfaceSymbol.Name == "IAsymmetricObject";

        private static bool IsIMappedObjectInterface(this INamedTypeSymbol interfaceSymbol) => interfaceSymbol.Name == "IMappedObject";

        public static INamedTypeSymbol AsNamed(this ITypeSymbol symbol)
        {
            if (symbol is INamedTypeSymbol namedSymbol)
            {
                return namedSymbol;
            }

            throw new Exception($"symbol is not INamedTypeSymbol. Actual type: {symbol.GetType().Name}");
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

        // Heavily inspired from https://github.com/dotnet/roslyn-analyzers/blob/main/src/Utilities/Compiler/Extensions/INamedTypeSymbolExtensions.cs
        public static bool OverridesEquals(this ITypeSymbol symbol)
        {
            return symbol.GetMembers(WellKnownMemberNames.ObjectEquals).OfType<IMethodSymbol>().Any(m => m.IsObjectEqualsOverride());
        }

        public static bool OverridesGetHashCode(this ITypeSymbol symbol)
        {
            return symbol.GetMembers(WellKnownMemberNames.ObjectGetHashCode).OfType<IMethodSymbol>().Any(m => m.IsGetHashCodeOverride());
        }

        public static bool OverridesToString(this ITypeSymbol symbol)
        {
            return symbol.GetMembers(WellKnownMemberNames.ObjectToString).OfType<IMethodSymbol>().Any(m => m.IsToStringOverride());
        }

        public static bool HasPropertyChangedEvent(this ITypeSymbol symbol)
        {
            return symbol.GetMembers("PropertyChanged").OfType<IEventSymbol>().Any();
        }

        private static bool IsObjectEqualsOverride(this IMethodSymbol method)
        {
            return method is { IsOverride: true, ReturnType.SpecialType: SpecialType.System_Boolean, Parameters.Length: 1 } &&
                method.Parameters[0].Type.SpecialType == SpecialType.System_Object &&
                IsObjectMethodOverride(method);
        }

        private static bool IsGetHashCodeOverride(this IMethodSymbol method)
        {
            return method is { IsOverride: true, ReturnType.SpecialType: SpecialType.System_Int32, Parameters.IsEmpty: true } &&
                IsObjectMethodOverride(method);
        }

        private static bool IsToStringOverride(this IMethodSymbol method)
        {
            return method is { IsOverride: true, ReturnType.SpecialType: SpecialType.System_String, Parameters.IsEmpty: true } &&
                IsObjectMethodOverride(method);
        }

        private static bool IsObjectMethodOverride(IMethodSymbol method)
        {
            var overriddenMethod = method.OverriddenMethod;
            while (overriddenMethod != null)
            {
                if (overriddenMethod.ContainingType.SpecialType == SpecialType.System_Object)
                {
                    return true;
                }

                overriddenMethod = overriddenMethod.OverriddenMethod;
            }

            return false;
        }

        public static bool IsAutomaticProperty(this PropertyDeclarationSyntax propertySyntax)
        {
            // This means the property has explicit getter and/or setter
            if (propertySyntax.AccessorList != null)
            {
                // Body is "classic" curly brace body
                // ExpressionBody is =>
                return !propertySyntax.AccessorList.Accessors.Any(a => a.Body != null || a.ExpressionBody != null);
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

        public static string ToCodeString(this IndexType? index)
        {
            return $"IndexType.{index ?? IndexType.None}";
        }

        #region Formatting

        public static string Indent(this string str, int indents = 1, bool trimNewLines = false)
        {
            var indentString = new string(' ', indents * 4);

            var sb = new StringBuilder();
            var lines = str.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    sb.Append(indentString);
                }

                sb.AppendLine(line);
            }

            sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            var result = sb.ToString();
            if (trimNewLines)
            {
                result = result.TrimEnd();
            }

            return result;
        }

        public static string Indent(this StringBuilder sb, int indents = 1, bool trimNewLines = false) => sb.ToString().Indent(indents, trimNewLines);

        #endregion
    }
}
