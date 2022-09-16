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

using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    [Generator]
    public class RealmGenerator : ISourceGenerator
    {
        /* Not explicitly supported:
         * - Inheritance of any kind (classes cannot derive from anything)
         * - Full nullability support
         */

        /* Notes:
         * - If a realm class is defined inside other classes, all the enclosing classes must be partial
         * - "OnManaged" is a partial method now
         * - "OnPropertyChanged" is a partial method now
         * 
         */

        /*
         * TODOS:
         * - Avoid generating some methods if already present (Equals, ToString, GetHashCode, PropertyChanged)
         * - Investigate how does it work with classes defined inside other classes (private, public, ...), especially if they
         * contain properties of other classes (in the same enclosing class or not)
         */

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

            var parser = new Parser(context);
            var parsingResults = parser.Parse(scr.RealmClasses);

            var diagnosticsEmitter = new DiagnosticsEmitter(context);
            diagnosticsEmitter.Emit(parsingResults);

            var codeEmitter = new CodeEmitter(context);
            codeEmitter.Emit(parsingResults);
        }
    }
}
