using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Collections.Generic;

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
                //Namespace
                var namespaceSymbol = classSymbol.ContainingNamespace;
                var namespaceName = namespaceSymbol.Name;

                //Class name
                var className = classSymbol.Name;

                //TODO Not necessarily the class is public
                builder.Append(@$"
namespace {namespaceName}
{{
    public partial class {className} : IRealmObject
    {{

    }}
}}");
                //builder.Append("\n\r");
                var sourceText = SourceText.From(builder.ToString(), Encoding.UTF8);
                context.AddSource($"{className}_generated.cs", sourceText);
            }

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
            if (!classSymbol.AllInterfaces.Any(i => i.Name == "IRealmObject"))
            {
                return;
            }

            RealmClasses.Add((cds, classSymbol));


        }
    }
}
