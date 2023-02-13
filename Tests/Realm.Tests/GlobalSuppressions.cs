// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:Partial elements should be documented", Justification = "No docs needed for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "No docs needed for tests.", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1820:Test for empty strings using string length", Justification = "We know what we're doing", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1312:Variable names should begin with lower-case letter", Justification = "Makes it easier to know what these are.", Scope = "member", Target = "~M:Realms.Tests.Database.SimpleLINQtests.SearchComparingChar")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Necessary for parameterized tests.", Scope = "module")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Should still be treated as a private field.", Scope = "module")]
[assembly: SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "One is the base method and the other a property in the db.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:File may only contain a single namespace", Justification = "These are tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:Enumeration items should be documented", Justification = "No docs needed for tests.", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "_ is a valid parameter name", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Design", "CA1054:URI-like parameters should not be strings", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("Globalization", "CA1310: Specify StringComparison for correctness", Justification = "We can't assume users follow best practices.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "This is fine for tests.", Scope = "module")]
[assembly: SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1000:Keywords should be spaced correctly", Justification = "In C# 9.0 we can use new() to instantiate objects and we don't need a space there", Scope = "module")]
[assembly: SuppressMessage("Security", "CA3075:Insecure DTD processing in XML", Justification = "This is an xml file we use internally only.", Scope = "member", Target = "~M:Realms.Tests.ConfigHelpers.#cctor")]
