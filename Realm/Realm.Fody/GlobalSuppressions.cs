// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "No docs needed for weaver.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "No docs needed for weaver.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "We use local var T for generic arguments.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1509:Opening braces should not be preceded by blank line", Justification = "We're opening scopes for defining references.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Nicer organization that way.", Scope = "type", Target = "~T:RealmWeaver.WeaveModuleResult")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Nicer organization that way.", Scope = "type", Target = "~T:RealmWeaver.WeaveTypeResult")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Nicer organization that way", Scope = "type", Target = "~T:RealmWeaver.WeavePropertyResult")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "It doesn't really conflict with anything.", Scope = "member", Target = "~M:RealmWeaver.ILogger.Error(System.String,Mono.Cecil.Cil.SequencePoint)")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:Keywords should be spaced correctly", Justification = "In C# 9.0 we can use new() to instantiate objects and we don't need a space there", Scope = "module")]
