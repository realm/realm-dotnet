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
using System.Security;

namespace Realms.DataBinding
{
    internal class WovenPropertyInfo : PropertyInfo
    {
        private readonly PropertyInfo _pi;
        private readonly Lazy<MethodInfo?> _publicGetter;
        private readonly Lazy<MethodInfo?> _nonPublicGetter;
        private readonly Lazy<MethodInfo?> _publicSetter;
        private readonly Lazy<MethodInfo?> _nonPublicSetter;

        public override PropertyAttributes Attributes => _pi.Attributes;

        public override bool CanRead => _pi.CanRead;

        public override bool CanWrite => _pi.CanWrite;

        public override Type? DeclaringType => _pi.DeclaringType;

        public override string Name => _pi.Name;

        public override Type PropertyType => _pi.PropertyType;

        public override Type? ReflectedType => _pi.ReflectedType;

        public WovenPropertyInfo(PropertyInfo pi)
        {
            if (pi == null)
            {
                throw new ArgumentNullException(nameof(pi));
            }

            _pi = pi;
            _publicGetter = GetGetterLazy(nonPublic: false);
            _nonPublicGetter = GetGetterLazy(nonPublic: true);
            _publicSetter = GetSetterLazy(nonPublic: false);
            _nonPublicSetter = GetSetterLazy(nonPublic: true);
        }

        public override MethodInfo[] GetAccessors(bool nonPublic) => _pi.GetAccessors(nonPublic);

        public override object[] GetCustomAttributes(bool inherit) => _pi.GetCustomAttributes(inherit);

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _pi.GetCustomAttributes(attributeType, inherit);

        public override MethodInfo? GetGetMethod(bool nonPublic)
        {
            return nonPublic ? _nonPublicGetter.Value : _publicGetter.Value;
        }

        public override ParameterInfo[] GetIndexParameters() => _pi.GetIndexParameters();

        public override MethodInfo? GetSetMethod(bool nonPublic)
        {
            return nonPublic ? _nonPublicSetter.Value : _publicSetter.Value;
        }

        // From https://github.com/mono/mono/blob/master/mcs/class/corlib/System.Reflection/MonoProperty.cs#L408
        public override object? GetValue(object? obj, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
        {
            var method = GetGetMethod(true);
            if (method == null)
            {
                throw new ArgumentException("Get Method not found for '" + Name + "'");
            }

            try
            {
                if (index == null || index.Length == 0)
                {
                    return method.Invoke(obj, invokeAttr, binder, null, culture);
                }

                return method.Invoke(obj, invokeAttr, binder, index, culture);
            }
            catch (SecurityException se)
            {
                throw new TargetInvocationException(se);
            }
        }

        public override bool IsDefined(Type attributeType, bool inherit) => _pi.IsDefined(attributeType, inherit);

        // From https://github.com/mono/mono/blob/master/mcs/class/corlib/System.Reflection/MonoProperty.cs#L429
        public override void SetValue(object? obj, object? value, BindingFlags invokeAttr, Binder? binder, object?[]? index, CultureInfo? culture)
        {
            var method = GetSetMethod(true);
            if (method == null)
            {
                throw new ArgumentException("Set Method not found for '" + Name + "'");
            }

            object?[] parms;
            var ilen = index?.Length ?? 0;
            if (ilen == 0)
            {
                parms = new[] { value };
            }
            else
            {
                parms = new object?[ilen + 1];
                index!.CopyTo(parms, 0);
                parms[ilen] = value;
            }

            method.Invoke(obj, invokeAttr, binder, parms, culture);
        }

        private Lazy<MethodInfo?> GetGetterLazy(bool nonPublic)
        {
            return new Lazy<MethodInfo?>(() =>
            {
                var mi = _pi.GetGetMethod(nonPublic);
                return mi == null ? null : new WovenGetterMethodInfo(mi);
            });
        }

        private Lazy<MethodInfo?> GetSetterLazy(bool nonPublic)
        {
            return new Lazy<MethodInfo?>(() =>
            {
                var mi = _pi.GetSetMethod(nonPublic);
                return mi == null ? null : new WovenSetterMethodInfo(mi, _pi.GetMethod!);
            });
        }
    }
}
