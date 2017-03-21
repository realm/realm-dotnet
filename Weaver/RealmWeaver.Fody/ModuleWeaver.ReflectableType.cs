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

using Mono.Cecil;
using Mono.Cecil.Cil;

public partial class ModuleWeaver
{
    private void WeaveReflectableType(TypeDefinition type)
    {
        if (_references.TypeInfoHelper_GetInfo == null)
        {
            return;
        }

        type.Interfaces.Add(_references.System_Reflection_IReflectableType);

        var getTypeInfo = new MethodDefinition("GetTypeInfo", DefaultMethodAttributes, _references.System_Reflection_TypeInfo);
        {
            var il = getTypeInfo.Body.GetILProcessor();
            var fromType = new GenericInstanceMethod(_references.TypeInfoHelper_GetInfo) { GenericArguments = { type } };
            il.Emit(OpCodes.Call, fromType);
            il.Emit(OpCodes.Ret);
        }

        type.Methods.Add(getTypeInfo);
    }
}
