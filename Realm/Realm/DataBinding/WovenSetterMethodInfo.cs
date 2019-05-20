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
        private readonly MethodInfo _mi;

        public override MethodAttributes Attributes => _mi.Attributes;

        public override Type DeclaringType => _mi.DeclaringType;

        public override RuntimeMethodHandle MethodHandle => _mi.MethodHandle;

        public override string Name => _mi.Name;

        public override Type ReflectedType => _mi.ReflectedType;

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => _mi.ReturnTypeCustomAttributes;

        public override Type ReturnType => _mi.ReturnType;

        public WovenSetterMethodInfo(MethodInfo mi)
        {
            Argument.NotNull(mi, nameof(mi));
            _mi = mi;
        }

        public override MethodInfo GetBaseDefinition() => _mi.GetBaseDefinition();

        public override object[] GetCustomAttributes(bool inherit) => _mi.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _mi.GetCustomAttributes(attributeType, inherit);

        public override MethodImplAttributes GetMethodImplementationFlags() => _mi.GetMethodImplementationFlags();

        public override ParameterInfo[] GetParameters() => _mi.GetParameters();

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            var managingRealm = (obj as RealmObject)?.Realm;
            Transaction writeTransaction = null;
            if (managingRealm != null && !managingRealm.IsInTransaction)
            {
                writeTransaction = managingRealm.BeginWrite();
            }

            var result = _mi.Invoke(obj, invokeAttr, binder, parameters, culture);

            if (writeTransaction != null)
            {
                writeTransaction.Commit();
                writeTransaction.Dispose();
            }

            return result;
        }

        public override bool IsDefined(Type attributeType, bool inherit) => _mi.IsDefined(attributeType, inherit);
    }
}