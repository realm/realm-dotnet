// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "No docs needed for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "No docs needed for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "We underscore private members", Scope = "module")]
[assembly: SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "We're not after cryptography", Scope = "member", Target = "~P:DeployApps.BaasClient._shortDifferentiator")]
[assembly: SuppressMessage("Globalization", "CA1310:Specify StringComparison for correctness", Justification = "We're controlling inputs", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "We underscore private members", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Function body is long", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:Keywords should be spaced correctly", Justification = "In C# 9.0 we can use new() to instantiate objects and we don't need a space there", Scope = "module")]
