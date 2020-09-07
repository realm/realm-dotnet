// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "No docs needed for weaver.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "No docs needed for weaver.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "We use local var T for generic arguments.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1509:Opening braces should not be preceded by blank line", Justification = "We're opening scopes for defining references.", Scope = "module")]
