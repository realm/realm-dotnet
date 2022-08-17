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
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Realms.SourceGenerator
{
    internal class Emitter
    {
        private GeneratorExecutionContext context;
        private ParsingResults parsingResults;

        public Emitter(GeneratorExecutionContext context, ParsingResults parsingResults)
        {
            this.context = context;
            this.parsingResults = parsingResults;
        }

        public void Emit()
        {
            foreach (var classInfo in parsingResults.ClassInfo)
            {
                try
                {
                    var generator = new ClassCodeBuilder(classInfo);
                    var generatedSource = generator.GenerateSource();

                    // This helps with normalizing whitespace, but it could be expensive. Also, it's kinda aggressive (the schema definition gets squished for example)
                    // var formattedFile = CSharpSyntaxTree.ParseText(SourceText.From(generatedSource, Encoding.UTF8)).GetRoot().NormalizeWhitespace().SyntaxTree.GetText();
                    var formattedFile = SourceText.From(generatedSource, Encoding.UTF8);

                    context.AddSource($"{classInfo.Name}_generated.cs", formattedFile);
                }
                catch (Exception ex)
                {
                    context.ReportDiagnostic(Diagnostics.UnexpectedError(classInfo.Name, ex.Message, ex.StackTrace));
                    throw;
                }
            }

        }
    }
}
