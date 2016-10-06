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
using System.Collections.Generic;
using System.Reflection;
using Realms.Weaving;

namespace Realms.Dynamic
{
    internal class DynamicRealmObjectHelper : IRealmObjectHelper
    {
        internal static readonly DynamicRealmObjectHelper Instance = new DynamicRealmObjectHelper();

        public void CopyToRealm(RealmObject instance)
        {
            foreach (var property in instance.ObjectSchema)
            {
                var field = property.PropertyInfo.DeclaringType.GetField(
                           property.PropertyInfo.GetCustomAttribute<WovenPropertyAttribute>().BackingFieldName,
                           BindingFlags.Instance | BindingFlags.NonPublic
                        );
                var value = field?.GetValue(this);
                if (value != null)
                {
                    var listValue = value as IEnumerable<RealmObject>;
                    if (listValue != null)  // assume it is IList NOT a RealmList so need to wipe afer copy
                    {
                        // cope with ReplaceListGetter creating a getter which assumes 
                        // a backing field for a managed IList is already a RealmList, so null it first
                        field.SetValue(this, null);  // now getter will create a RealmList below
                        var realmList = (ICopyValuesFrom)property.PropertyInfo.GetValue(this, null);
                        realmList.CopyValuesFrom(listValue);
                    }
                    else
                    {
                        property.PropertyInfo.SetValue(this, value, null);
                    }
                }  // only null if blank relationship or string so leave as default
            }
        }

        public RealmObject CreateInstance()
        {
            return new DynamicRealmObject();
        }
    }
}

