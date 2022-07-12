using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realm.SourceGenerator
{
    internal class SyntaxContextReceiver : ISyntaxContextReceiver
    {
        public IList<(ClassDeclarationSyntax, ITypeSymbol)> RealmClasses { get; } = new List<(ClassDeclarationSyntax, ITypeSymbol)>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is not ClassDeclarationSyntax cds)// || cds.Identifier.ToString() != "TestClass")
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
