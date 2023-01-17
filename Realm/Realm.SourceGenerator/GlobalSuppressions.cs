// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:Keywords should be spaced correctly", Justification = "In C# 9.0 we can use new() to instantiate objects and we don't need a space there", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "This is only used by the source generator and should not be exposed to users.", Scope = "type", Target = "~T:Realms.SourceGenerator.RealmGenerator")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Nicer organization that way.", Scope = "type", Target = "~T:Realms.SourceGenerator.DiagnosticInfo")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Nicer organization that way.", Scope = "type", Target = "~T:Realms.SourceGenerator.DiagnosticLocation")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Nicer organization that way.", Scope = "type", Target = "~T:Realms.SourceGenerator.ClassInfo")]
[assembly: SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "The parameters are used when DEBUG is set", Scope = "member", Target = "~M:Realms.SourceGenerator.DiagnosticsEmitter.SerializeDiagnostics(Microsoft.CodeAnalysis.GeneratorExecutionContext,Realms.SourceGenerator.ClassInfo)")]
