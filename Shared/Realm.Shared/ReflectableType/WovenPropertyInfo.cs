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

namespace Realms
{
    internal class WovenPropertyInfo : PropertyInfo
    {
        private readonly PropertyInfo _pi;

        public override PropertyAttributes Attributes => _pi.Attributes;

        public override bool CanRead => _pi.CanRead;

        public override bool CanWrite => _pi.CanWrite;

        public override Type DeclaringType => _pi.DeclaringType;

        public override string Name => _pi.Name;

        public override Type PropertyType => _pi.PropertyType;

        public override Type ReflectedType => _pi.ReflectedType;

        public WovenPropertyInfo(PropertyInfo pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException(nameof(pi));
            }

            _pi = pi;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic) => _pi.GetAccessors(nonPublic);

        public override object[] GetCustomAttributes(bool inherit) => _pi.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _pi.GetCustomAttributes(attributeType, inherit);

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            var mi = _pi.GetGetMethod(nonPublic);
            return new WovenGetterMethodInfo(mi);
        }

        public override ParameterInfo[] GetIndexParameters() => _pi.GetIndexParameters();

        public override MethodInfo GetSetMethod(bool nonPublic) => _pi.GetSetMethod(nonPublic);

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => _pi.GetValue(obj, invokeAttr, binder, index, culture);

        public override bool IsDefined(Type attributeType, bool inherit) => _pi.IsDefined(attributeType, inherit);

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) => _pi.SetValue(obj, value, invokeAttr, binder, index, culture);
    }
}
