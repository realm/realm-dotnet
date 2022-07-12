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
using System.Text;

namespace Realm.SourceGenerator
{
    internal class Generator
    {
        private ClassInfo _classInfo;

        private string helperClassName;
        private string accessorInterfaceName;
        private string managedAccessorClassName;
        private string unmanagedAccessorClassName;

        private readonly string _copyrightString = @"// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright {0} Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the ""License"")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an ""AS IS"" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////
";

        public Generator(ClassInfo classInfo)
        {
            _classInfo = classInfo;

            helperClassName = $"{_classInfo.Name}ObjectHelper";
            accessorInterfaceName = $"I{_classInfo.Name}Accessor";
            managedAccessorClassName = $"{_classInfo.Name}ManagedAccessor";
            unmanagedAccessorClassName = $"{_classInfo.Name}UnmanagedAccessor";
        }

        public string GenerateSource()
        {


            return null;
        }

        private const string _accessorInterfaceString = @"
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface {0} : IRealmAccessor
    {
        {1}
    }";

        private string GenerateInterface()
        {
            var propertiesBuilder = new StringBuilder();

            foreach (var property in _classInfo.Properties)
            {
                var type = property.TypeInfo.TypeString;
                var name = property.Name;
                var hasSetter = !property.TypeInfo.IsCollection && !property.TypeInfo.IsIQueryable;
                var setterString = hasSetter ? " set; " : "";

                var propertyString = $"{type} {name} {{ get;{setterString}}}";
                propertiesBuilder.AppendLine(propertyString);
                propertiesBuilder.AppendLine();
            }

            return string.Format(_accessorInterfaceString, accessorInterfaceName, propertiesBuilder.ToString());
        }

        private string GeneratePartialClass()
        {
            return null;
        }

        private const string _objectHelperString = @"
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class {0} : IRealmObjectHelper
    {
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {
            throw new InvalidOperationException(""This method should not be called for source generated classes."");
        }

        public ManagedAccessor CreateAccessor() => new {1}();

        public IRealmObjectBase CreateInstance()
        {
            return new {2}();
        }

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {
            {3}
        }
    }";

        private string GenerateClassObjectHelper()
        {
            var primaryKeyProperty = _classInfo.PrimaryKey;
            string tryGetPrimaryKeyBody;

            if (primaryKeyProperty != null)
            {
                var valueString = $"value = (({accessorInterfaceName})instance.Accessor).{primaryKeyProperty.Name};";
                tryGetPrimaryKeyBody = $@"{valueString}
                return true;";
            }
            else
            {
                tryGetPrimaryKeyBody = "return false";
            }

            return string.Format(_objectHelperString, helperClassName, managedAccessorClassName, _classInfo.Name, tryGetPrimaryKeyBody);
        }

        private string GeneratedUnmanagedAccessor()
        {
            return null;
        }

        private const string _managedAccessorString = @"
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class {0} : ManagedAccessor, {1}
    {
        {1}
    }
";

        private string GenerateManagedAccessor()
        {
            var propertiesBuilder = new StringBuilder();

            foreach (var property in _classInfo.Properties)
            {
                var type = property.TypeInfo.TypeString;
                var name = property.Name;
                var stringName = property.MapTo ?? name;

                // TODO Where does IQueryable go?
                if (property.TypeInfo.IsCollection)
                {

                }
                else
                {
                    var getterString = $@"get => ({type})GetValue(""{stringName}"");";

                    var setterMethod = property.IsPrimaryKey ? "SetValueUnique" : "SetValue";
                    var setterString = $@"set => {setterMethod}(""{stringName}"", value)";

                    var propertyString = @$"
        public {type} {name}
        {{
            {getterString}
            {setterString}
        }}
";
                    propertiesBuilder.Append(propertyString);
                }
            }

            return string.Format(_accessorInterfaceString, accessorInterfaceName, propertiesBuilder.ToString());
        }
    }
}
