////////////////////////////////////////////////////////////////////////////
//
// Copyright 2024 Realm Inc.
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace Realms.SourceGenerator;

internal abstract class ClassCodeBuilderBase
{
    private readonly string[] _defaultNamespaces =
    {
        "MongoDB.Bson.Serialization",
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "System.Runtime.CompilerServices",
        "System.Runtime.Serialization",
        "System.Xml.Serialization",
        "System.Reflection",
        "System.ComponentModel",
        "Realms",
        "Realms.Weaving",
        "Realms.Schema",
    };

    protected readonly ClassInfo _classInfo;
    protected readonly Lazy<string> _ignoreFieldAttribute;

    protected string _baseInterface => $"I{_classInfo.ObjectType}";

    protected ClassCodeBuilderBase(ClassInfo classInfo, GeneratorConfig generatorConfig)
    {
        _classInfo = classInfo;

        _ignoreFieldAttribute = new(() =>
        {
            var result = "[IgnoreDataMember, XmlIgnore]";
            var customAttribute = generatorConfig.CustomIgnoreAttribute;
            if (!string.IsNullOrEmpty(customAttribute))
            {
                result += customAttribute;
            }

            return result;
        });
    }

    public static ClassCodeBuilderBase CreateBuilder(ClassInfo classInfo, GeneratorConfig generatorConfig) => classInfo.ObjectType switch
    {
        ObjectType.MappedObject => new MappedObjectCodeBuilder(classInfo, generatorConfig),
        ObjectType.AsymmetricObject or ObjectType.EmbeddedObject or ObjectType.RealmObject => new RealmObjectCodeBuilder(classInfo, generatorConfig),
        _ => throw new NotSupportedException($"Unexpected ObjectType: {classInfo.ObjectType}")
    };

    public string GenerateSource()
    {
        var usings = GetUsings();

        var classString = GeneratePartialClass();

        foreach (var enclosingClass in _classInfo.EnclosingClasses)
        {
            classString = $@"{SyntaxFacts.GetText(enclosingClass.Accessibility)} partial class {enclosingClass.Name}
{{
{classString.Indent()}
}}";
        }

        if (!_classInfo.NamespaceInfo.IsGlobal)
        {
            classString = $@"namespace {_classInfo.NamespaceInfo.OriginalName}
{{
{classString.Indent()}
}}";
        }

        return $@"// <auto-generated />
#nullable enable

{usings}

{classString}
";
    }

    private string GetUsings()
    {
        var namespaces = new HashSet<string>(_defaultNamespaces);
        namespaces.UnionWith(_classInfo.Usings);

        if (!_classInfo.NamespaceInfo.IsGlobal)
        {
            namespaces.Add(_classInfo.NamespaceInfo.OriginalName);
        }

        return string.Join(Environment.NewLine, namespaces.Where(n => !string.IsNullOrWhiteSpace(n)).OrderBy(s => s).Select(s => $"using {s};"));
    }

    protected abstract string GeneratePartialClass();
}
