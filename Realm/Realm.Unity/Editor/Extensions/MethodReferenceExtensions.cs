////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System.ComponentModel;
using Mono.Cecil;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class MethodReferenceExtensions
{
    public static MethodReference MakeHostInstanceGeneric(this MethodReference @this, params TypeReference[] genericArguments)
    {
        var genericDeclaringType = new GenericInstanceType(@this.DeclaringType);
        foreach (var genericArgument in genericArguments)
        {
            genericDeclaringType.GenericArguments.Add(genericArgument);
        }

        var reference = new MethodReference(@this.Name, @this.ReturnType, genericDeclaringType)
        {
            HasThis = @this.HasThis,
            ExplicitThis = @this.ExplicitThis,
            CallingConvention = @this.CallingConvention
        };

        foreach (var parameter in @this.Parameters)
        {
            reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        }

        foreach (var genericParam in @this.GenericParameters)
        {
            reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
        }

        return reference;
    }

    public static bool ConstructsType(this MethodReference @this, TypeReference type)
    {
        return @this.DeclaringType.IsSameAs(type) && @this.Name == ".ctor";
    }
}
