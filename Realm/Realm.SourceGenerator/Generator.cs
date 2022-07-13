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

        private string _helperClassName;
        private string _accessorInterfaceName;
        private string _managedAccessorClassName;
        private string _unmanagedAccessorClassName;

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
// ////////////////////////////////////////////////////////////////////////////";

        public Generator(ClassInfo classInfo)
        {
            _classInfo = classInfo;

            _helperClassName = $"{_classInfo.Name}ObjectHelper";
            _accessorInterfaceName = $"I{_classInfo.Name}Accessor";
            _managedAccessorClassName = $"{_classInfo.Name}ManagedAccessor";
            _unmanagedAccessorClassName = $"{_classInfo.Name}UnmanagedAccessor";
        }

        private const string sourceString = @"
{0}

{1}

namespace {2}
{{
    {3}
}}

namespace Realm.Generated
{{
    {4}

    {5}

    {6}

    {7}
}}
";

        public string GenerateSource()
        {
            var copyright = string.Format(_copyrightString, DateTime.Now.Year);

            var usings = @$"using System.ComponentModel;
using Realms;
using Realms.Weaving;";

            var partialClassString = GeneratePartialClass();
            var interfaceString = GenerateInterface();
            var managedAccessorString = GenerateManagedAccessor();
            var unmanagedAccessorString = GeneratedUnmanagedAccessor();
            var objectHelperString = GenerateClassObjectHelper();

            return string.Format(sourceString, copyright, usings, _classInfo.Namespace,
                partialClassString, objectHelperString, interfaceString,
                managedAccessorString, unmanagedAccessorString);
        }

        private const string _accessorInterfaceString = @"[EditorBrowsable(EditorBrowsableState.Never)]
    internal interface {0} : IRealmAccessor
    {{
        {1}
    }}";

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
            }

            return string.Format(_accessorInterfaceString, _accessorInterfaceName, propertiesBuilder.ToString());
        }

        private string GeneratePartialClass()
        {
            return "";
        }

        private const string _objectHelperString = @"[EditorBrowsable(EditorBrowsableState.Never)]
    internal class {0} : IRealmObjectHelper
    {{
        public void CopyToRealm(IRealmObjectBase instance, bool update, bool skipDefaults)
        {{
            throw new InvalidOperationException(""This method should not be called for source generated classes."");
        }}

        public ManagedAccessor CreateAccessor() => new {1}();

        public IRealmObjectBase CreateInstance()
        {{
            return new {2}();
        }}

        public bool TryGetPrimaryKeyValue(IRealmObjectBase instance, out object value)
        {{
            {3}
        }}
    }}";

        private string GenerateClassObjectHelper()
        {
            var primaryKeyProperty = _classInfo.PrimaryKey;
            string tryGetPrimaryKeyBody;

            if (primaryKeyProperty != null)
            {
                var valueString = $"value = (({_accessorInterfaceName})instance.Accessor).{primaryKeyProperty.Name};";
                tryGetPrimaryKeyBody = $@"{valueString}
            return true;";
            }
            else
            {
                tryGetPrimaryKeyBody = @"value = null;
            return false;";
            }

            return string.Format(_objectHelperString, _helperClassName, _managedAccessorClassName, _classInfo.Name, tryGetPrimaryKeyBody);
        }

        private const string _unmanagedAccesorString = @"internal class {0} : UnmanagedAccessor, {1}
    {{
        {2}

        public {0}(Type objectType) : base(objectType)
        {{
        }}

        public override RealmValue GetValue(string propertyName)
        {{
            {3}
        }}

        public override void SetValue(string propertyName, RealmValue val)
        {{
            {4}
        }}

        public override void SetValueUnique(string propertyName, RealmValue val)
        {{
            {5}
        }}

        public override IList<T> GetListValue<T>(string propertyName)
        {{
            {6}
        }}

        public override ISet<T> GetSetValue<T>(string propertyName)
        {{
            {7}
        }}

        public override IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName)
        {{
            {8}
    }}";

        private string GeneratedUnmanagedAccessor()
        {
            var propertiesString = new StringBuilder();
            var getValueLines = new StringBuilder();
            var setValueLines = new StringBuilder();
            var setValueUniqueLines = new StringBuilder();
            var getListValueLines = new StringBuilder();
            var getSetValueLines = new StringBuilder();
            var getDictionaryValueLines = new StringBuilder();

            for (var i = 0; i < _classInfo.Properties.Count; i++)
            {
                var isNotFirst = i != 0;
                var property = _classInfo.Properties[i];
                var name = property.Name;
                var backingFieldName = "_" + char.ToLowerInvariant(name[0]) + name.Substring(1);
                var type = property.TypeInfo.TypeString;
                var stringName = property.MapTo ?? name;

                if (property.TypeInfo.IsCollection)
                {
                    //GetListValue

                    //GetSetValue

                    //GetDictionaryValue
                }
                else
                {
                    // Properties
                    var backingFieldString = $"private {type} {backingFieldName};";

                    var propertyString = @$"
        public string {name}
        {{get => {backingFieldName};
            set
            {{
                stringValue = value;
                RaisePropertyChanged(""{stringName}"");
            }}
        }}";
                    //TODO Need to distinguish between non-collection and collection, because we don't know what's the first
                    if(isNotFirst)
                    {
                        propertiesString.AppendLine();
                    }

                    propertiesString.AppendLine(backingFieldString);
                    propertiesString.Append(propertyString);

                    //GetValue
                    getValueLines.Append(@$"""{stringName}"" => {backingFieldName},");

                    //SetValue/SetValueUnique
                    setValueLines.Append($@"case ""{stringName}"":");

                    if (property.IsPrimaryKey)
                    {
                        setValueLines.AppendLine($@"throw new InvalidOperationException(""Cannot set the value of a primary key property with SetValue.You need to use SetValueUnique"");");

                        setValueUniqueLines.AppendLine($@"if (propertyName != ""{stringName}"")
                        {{
                            throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");
                        }}

                        {name} = ({type})val;
                        ");
                    }
                    else
                    {
                        setValueLines.AppendLine($@"{name} = ({type})val;
                        return;");
                    }
                }

            }

            //Properties

            var propertyBody = propertiesString.ToString();

            //GetValue

            var getValueBody = $@"return propertyName switch
            {{
                {getValueLines}
                => throw new MissingMemberException($""The object does not have a gettable Realm property with name {{propertyName}}""),
            }};";

            //SetValue

            var setValueBody = $@"switch (propertyName)
            {{
                {setValueLines}
                default:
                        throw new MissingMemberException($""The object does not have a settable Realm property with name {{propertyName}}"");
            }}";

            //SetValueUnique

            if (setValueUniqueLines.Length == 0)
            {
                setValueUniqueLines.Append(@"throw new InvalidOperationException(""Cannot set the value of an non primary key property with SetValueUnique"");");
            }

            var setValueUniqueBody = setValueUniqueLines.ToString();

            //GetListValue

            if (getListValueLines.Length == 0)
            {
                getListValueLines.Append(@"throw new MissingMemberException($""The object does not have a Realm list property with name { propertyName}"");");
            }

            var getListValueBody = getListValueLines.ToString();


            //GetSetValue

            if (getSetValueLines.Length == 0)
            {
                getSetValueLines.Append(@"throw new MissingMemberException($""The object does not have a Realm set property with name { propertyName}"");");
            }

            var getSetValueBody = getSetValueLines.ToString();


            //GetDictionaryValue

            if (getDictionaryValueLines.Length == 0)
            {
                getDictionaryValueLines.Append(@"throw new MissingMemberException($""The object does not have a Realm dictionary property with name { propertyName}"");");
            }

            var getDictionaryBody = getDictionaryValueLines.ToString();

            return string.Format(_unmanagedAccesorString, _unmanagedAccessorClassName, _accessorInterfaceName,
                propertyBody, getValueBody, setValueBody, setValueUniqueBody,
                getListValueBody, getSetValueBody, getDictionaryBody);

        }

        private const string _managedAccessorString = @"[EditorBrowsable(EditorBrowsableState.Never)]
    internal class {0} : ManagedAccessor, {1}
    {{
        {2}
    }}";

        private string GenerateManagedAccessor()
        {
            var propertiesBuilder = new StringBuilder();

            for (var i = 0; i < _classInfo.Properties.Count; i++)
            {
                var isNotFirst = i != 0;
                var property = _classInfo.Properties[i];
                var type = property.TypeInfo.TypeString;
                var name = property.Name;
                var stringName = property.MapTo ?? name;

                // TODO Where does IQueryable go?
                if (property.TypeInfo.IsCollection)
                {
                    //TODO
                }
                else
                {
                    var getterString = $@"get => ({type})GetValue(""{stringName}"");";

                    var setterMethod = property.IsPrimaryKey ? "SetValueUnique" : "SetValue";
                    var setterString = $@"set => {setterMethod}(""{stringName}"", value);";

                    var propertyString = @$"public {type} {name}
        {{
            {getterString}
            {setterString}
        }}";
                    if (isNotFirst)
                    {
                        propertiesBuilder.AppendLine();
                    }

                    propertiesBuilder.Append(propertyString);
                }
            }

            return string.Format(_managedAccessorString,  _managedAccessorClassName, _accessorInterfaceName, propertiesBuilder.ToString());
        }
    }
}
