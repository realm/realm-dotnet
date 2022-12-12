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

using System;
using System.Globalization;
using System.Reflection;
using Realms.Helpers;

namespace Realms.DataBinding
{
    internal class WovenSetterMethodInfo : MethodInfo
    {
        private readonly MethodInfo _getterMi;

        private readonly MethodInfo _setterMi;

        public override MethodAttributes Attributes => _setterMi.Attributes;

        public override Type DeclaringType => _setterMi.DeclaringType;

        public override RuntimeMethodHandle MethodHandle => _setterMi.MethodHandle;

        public override string Name => _setterMi.Name;

        public override Type ReflectedType => _setterMi.ReflectedType;

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => _setterMi.ReturnTypeCustomAttributes;

        public override Type ReturnType => _setterMi.ReturnType;

        public WovenSetterMethodInfo(MethodInfo setterMi, MethodInfo getterMi)
        {
            Argument.NotNull(setterMi, nameof(setterMi));
            Argument.NotNull(getterMi, nameof(getterMi));

            _setterMi = setterMi;
            _getterMi = getterMi;
        }

        public override MethodInfo GetBaseDefinition() => _setterMi.GetBaseDefinition();

        public override object[] GetCustomAttributes(bool inherit) => _setterMi.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _setterMi.GetCustomAttributes(attributeType, inherit);

        public override MethodImplAttributes GetMethodImplementationFlags() => _setterMi.GetMethodImplementationFlags();

        public override ParameterInfo[] GetParameters() => _setterMi.GetParameters();

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            var realmObject = obj as IRealmObjectBase;

            if (realmObject?.IsManaged == true)
            {
                if (_getterMi.Invoke(realmObject, null).Equals(parameters[0]))
                {
                    return null;
                }

                var managingRealm = (obj as IRealmObjectBase)?.Realm;

                // If managingRealm is not null and not currently in transaction, wrap setting the property in a realm.Write(...)
                if (managingRealm?.IsInTransaction == false)
                {
                    return managingRealm.Write(() =>
                    {
                        return _setterMi.Invoke(obj, invokeAttr, binder, parameters, culture);
                    });
                }
            }

            return _setterMi.Invoke(obj, invokeAttr, binder, parameters, culture);
        }

        public override bool IsDefined(Type attributeType, bool inherit) => _setterMi.IsDefined(attributeType, inherit);
    }
}
