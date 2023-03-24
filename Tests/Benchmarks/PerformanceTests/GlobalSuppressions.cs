// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "No need to document test projects.", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is fine for test", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "No docs for tests", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1837:Use 'Environment.ProcessId'", Justification = "Not supported for netstandard2.0", Scope = "member", Target = "~M:PerformanceTests.BenchmarkBase.#cctor")]
