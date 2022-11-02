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
    // TODO andrea: check everything for exceptions
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
                    var payload = _analytics.SubmitAnalytics();

                    // TODO andrea: read this from env vars
#if REALM_PRINT_ANALYTICS
                    // TODO andrea: likely log this in diagnostics
                    context.ReportDiagnostic(Diagnostics.Info($@"
----------------------------------
Analytics payload
{payload}
----------------------------------"));
#endif
                }
                catch (Exception e)
                {
                    // TODO andrea: likely log this in diagnostics
                    // something like
                    //context.ReportDiagnostic(Diagnostics.Info("Error submitting analytics: " + e.Message));
                }
            });

            var diagnosticsEmitter = new DiagnosticsEmitter(context);
            diagnosticsEmitter.Emit(parsingResults);

            var codeEmitter = new CodeEmitter(context);
            codeEmitter.Emit(parsingResults);

            // TODO andrea: wait or not for the analytics task, to be seen
        }
    }
}
