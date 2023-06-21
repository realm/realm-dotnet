﻿////////////////////////////////////////////////////////////////////////////
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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realms.SourceGenerator
{
    internal class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        private readonly Dictionary<ITypeSymbol, RealmClassDefinition> _realmClassesDict = new(SymbolEqualityComparer.Default);

        public IReadOnlyCollection<RealmClassDefinition> RealmClasses => _realmClassesDict.Values;

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classSyntax)
            {
                return;
            }

            var classSymbol = (ITypeSymbol)context.SemanticModel.GetDeclaredSymbol(classSyntax)!;

            if (_realmClassesDict.TryGetValue(classSymbol, out var rcDefinition))
            {
                rcDefinition.ClassDeclarations.Add(classSyntax);
                return;
            }

            if (classSymbol.IsAnyRealmObjectType())
            {
                var realmClassDefinition = new RealmClassDefinition(classSymbol, new List<ClassDeclarationSyntax> { classSyntax });
                _realmClassesDict.Add(classSymbol, realmClassDefinition);
            }
        }
    }

    internal readonly struct RealmClassDefinition
    {
        public ITypeSymbol ClassSymbol { get; }

        public List<ClassDeclarationSyntax> ClassDeclarations { get; }

        public RealmClassDefinition(ITypeSymbol classSymbol, List<ClassDeclarationSyntax> classDeclarations)
        {
            ClassSymbol = classSymbol;
            ClassDeclarations = classDeclarations;
        }
    }
}
