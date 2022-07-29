// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace RealmWeaver
{
    internal class SourceGeneratorRealmWeaver
    {
        /*
         * The main idea here is that we reduce/simplify the work done by the weaver
         * by using info coming out from the generated code. 
         * 
         * We also expect the weaver to not produce any diagnostic, as it should have been done
         * by the source generator.
         * 1. Find all classes with a specific attribute
         * 2. Find the corresponding accessor interface
         * 3. Find all attributes in the class that have the same name as the interface
         * 4. Change the way the properties are implemented
         */

        private readonly Lazy<MethodReference> _propertyChanged_DoNotNotify_Ctor;

        private readonly ImportedReferences _references;
        private readonly ModuleDefinition _moduleDefinition;
        private readonly ILogger _logger;

        private IEnumerable<TypeDefinition> GetMatchingTypes()
        {
            foreach (var type in _moduleDefinition.GetTypes().Where(t =>
            t.CustomAttributes.Any(a => a.AttributeType.Name == "ToWeaveAttribute")))
            {
                yield return type;
            }
        }

        private WeaveTypeResult WeaveType(TypeDefinition type)
        {
            _logger.Debug("Weaving " + type.Name);

            var interfaceName = $"I{type.Name}Accessor";
            var interfaceType = _moduleDefinition.GetType("Realms.Generated", interfaceName);

            var didSucceed = true;
            var persistedProperties = new List<WeavePropertyResult>();

            // This should be an easy way to get all the properties we're interest into, without doing additional checks
            foreach (var interfaceProperty in interfaceType.Properties)
            {
                var prop = type.Properties.First(p => p.Name == interfaceProperty.Name);

                try
                {
                    var weaveResult = WeaveProperty(prop, type);
                    if (weaveResult.Woven)
                    {
                        persistedProperties.Add(weaveResult);
                    }
                    else
                    {

                        //What do do here?


                    }
                }
                catch (Exception e)
                {
                    var sequencePoint = prop.GetSequencePoint();
                    _logger.Error(
                        $"Unexpected error caught weaving property '{type.Name}.{prop.Name}': {e.Message}.\r\nCallstack:\r\n{e.StackTrace}",
                        sequencePoint);

                    return WeaveTypeResult.Error(type.Name);
                }
            }

            return didSucceed ? WeaveTypeResult.Success(type.Name, persistedProperties) : WeaveTypeResult.Error(type.Name);
        }

        private WeavePropertyResult WeaveProperty(PropertyDefinition prop, TypeDefinition type, TypeDefinition interfaceType)
        {
            var columnName = prop.Name;

            ReplaceGetter(prop, columnName, interfaceType);
            ReplaceSetter(prop, backingField, columnName, setter);

        }

        private void ReplaceGetter(PropertyDefinition prop, string columnName, TypeDefinition interfaceType)
        {
            //// A synthesized property getter looks like this:
            ////   0: ldarg.0
            ////   1: ldfld <backingField>
            ////   2: ret
            //// We want to change it so it looks like this:
            ////   0: ldarg.0
            ////   1: call Realms.RealmObject.get_IsManaged
            ////   2: brfalse.s 7
            ////   3: ldarg.0
            ////   4: ldstr <columnName>
            ////   5: call Realms.RealmObject.GetValue
            ////   6: call op_explicit prop.PropertyType
            ////   7: ret
            ////   8: ldarg.0
            ////   9: ldfld <backingField>
            ////  10: ret
            //// This is roughly equivalent to:
            ////   if (!base.IsManaged) return this.<backingField>;
            ////   return base.GetValue(<columnName>);
            ////
            //// For RealmObject targets, there's no implicit conversion from RealmValue to
            //// prop.PropertyType, so we convert implicitly to RealmObjectBase, then cast.
            //// This is roughly equivalent to:
            ////   if (!base.IsManaged) return this.<backingField>;
            ////   return (TargetType)*(RealmObjectBase)*base.GetValue(<columnName>);
            ///

            //// A synthesized property getter looks like this:
            ////   0: ldarg.0
            ////   1: ldfld <backingField>
            ////   2: ret
            //// We want to change it so it looks like this:
            ////   0: ldarg.0
            ////   1: ldfld _accessor
            ////   2: call property getter on accessor
            ////   3: ret
            //// This is equivalent to:
            ////   get => accessor.Property;

            var start = prop.GetMethod.Body.Instructions.First();
            var il = prop.GetMethod.Body.GetILProcessor();

            //TODO Maybe we need also the declaring type
            var accessorReference = new FieldReference("_accessor", interfaceType);

            //What does HasThis do?
            var propertyGetterOnAccessorReference = new MethodReference($"get_{prop.Name}", prop.PropertyType, interfaceType) { HasThis = true };

            il.InsertBefore(start, il.Create(OpCodes.Ldarg_0)); // this for call
            il.InsertBefore(start, il.Create(OpCodes.Ldfld, accessorReference));
            il.InsertBefore(start, il.Create(OpCodes.Callvirt, propertyGetterOnAccessorReference));
            il.InsertBefore(start, il.Create(OpCodes.Ret));
        }

        private void ReplaceSetter(PropertyDefinition prop, FieldReference backingField, string columnName, MethodReference setValueReference)
        {
            //// A synthesized property setter looks like this:
            ////   0: ldarg.0
            ////   1: ldarg.1
            ////   2: stfld <backingField>
            ////   3: ret
            ////
            //// We want to change it so it looks like this:
            ////   0: ldarg.0
            ////   1: call Realms.RealmObject.get_IsManaged
            ////   2: brfalse.s 8
            ////   3: ldarg.0
            ////   4: ldstr <columnName>
            ////   5: ldarg.1
            ////   6: call Realms.RealmObject.SetValue<T>
            ////   7: ret
            ////   8: ldarg.0
            ////   9: ldarg.1
            ////   10: stfld <backingField>
            ////   11: ret
            ////
            //// This is roughly equivalent to:
            ////   if (!base.IsManaged)
            ////   {
            ////        this.<backingField> = value;
            ////        RaisePropertyChanged(propertyName);
            ////    }
            ////   else base.SetValue<T>(<columnName>, value);

            if (setValueReference == null)
            {
                throw new ArgumentNullException(nameof(setValueReference));
            }

            // Whilst we're only targetting auto-properties here, someone like PropertyChanged.Fody
            // may have already come in and rewritten our IL. Lets clear everything and start from scratch.
            var il = prop.SetMethod.Body.GetILProcessor();
            prop.SetMethod.Body.Instructions.Clear();
            prop.SetMethod.Body.Variables.Clear();

            // While we can tidy up PropertyChanged.Fody IL if we're ran after it, we can't do a heck of a lot
            // if they're the last one in. To combat this, we'll add our own version of [DoNotNotify] which
            // PropertyChanged.Fody will respect.
            prop.CustomAttributes.Add(new CustomAttribute(_propertyChanged_DoNotNotify_Ctor.Value));

            var managedSetStart = il.Create(OpCodes.Ldarg_0);
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Call, _references.RealmObject_get_IsManaged));
            il.Append(il.Create(OpCodes.Brtrue_S, managedSetStart));

            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldarg_1));
            il.Append(il.Create(OpCodes.Stfld, backingField));
            il.Append(il.Create(OpCodes.Ldarg_0));
            il.Append(il.Create(OpCodes.Ldstr, prop.Name));
            il.Append(il.Create(OpCodes.Call, _references.RealmObject_RaisePropertyChanged));
            il.Append(il.Create(OpCodes.Ret));

            il.Append(managedSetStart);
            il.Append(il.Create(OpCodes.Ldstr, columnName));
            il.Append(il.Create(OpCodes.Ldarg_1));

            if (!prop.IsRealmValue())
            {
                var convertType = prop.PropertyType;
                if (prop.ContainsRealmObject(_references) || prop.ContainsEmbeddedObject(_references))
                {
                    convertType = _references.RealmObjectBase;
                }

                var convertMethod = new MethodReference("op_Implicit", _references.RealmValue, _references.RealmValue)
                {
                    Parameters = { new ParameterDefinition(convertType) },
                    HasThis = false
                };

                il.Append(il.Create(OpCodes.Call, convertMethod));
            }

            il.Append(il.Create(OpCodes.Call, setValueReference));
            il.Append(il.Create(OpCodes.Ret));
        }
    }
}
