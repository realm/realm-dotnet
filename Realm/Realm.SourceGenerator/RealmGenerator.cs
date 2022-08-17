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

using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    [Generator]
    public class RealmGenerator : ISourceGenerator
    {
        /* Not explicitly supported:
         * - Inheritance of any kind (classes cannot derive from anything)
         * - Partial classes
         * - Full nullability support
         */

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxContextReceiver scr || scr.RealmClasses == null)
            {
                return;
            }

            var parser = new Parser(context, scr.RealmClasses);
            var parsingResults = parser.Parse();

            if (parsingResults != null)
            {
                var emitter = new Emitter(context, parsingResults);
                emitter.Emit();
            }
        }
    }
}
