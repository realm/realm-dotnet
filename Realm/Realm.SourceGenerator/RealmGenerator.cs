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

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Realms.SourceGenerator
{
    [Generator]
    public class RealmGenerator : ISourceGenerator
    {
        private Analytics _analytics = new();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver(_analytics));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not SyntaxContextReceiver scr)
            {
                return;
            }

            var parser = new Parser(context, _analytics);
            var parsingResults = parser.Parse(scr.RealmClasses);

            var submitAnalytics = Task.Run(() =>
            {
                try
                {
                    // TODO andrea: the lifetime of context worries me a little as the generator
                    // may be kept alive by this task if we ever decide to wait on this task.
                    // At the same time, the only reason why I'm taking the context is to be able
                    // to print the metrics as standard diagnostics to the user's compilation output stream.
                    // We may look at other ways, like writing to file if necessary.
                    _analytics.SubmitAnalytics(context);
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(Diagnostics.AnalyticsDebugInfo(e.Message));
                }
            });

            var diagnosticsEmitter = new DiagnosticsEmitter(context);
            diagnosticsEmitter.Emit(parsingResults);

            var codeEmitter = new CodeEmitter(context);
            codeEmitter.Emit(parsingResults);

            // TODO andrea: investigate if we should wait or not for the analytics task to end
            // for now I'm locking the whole compilation
            submitAnalytics.Wait();
        }
    }
}
