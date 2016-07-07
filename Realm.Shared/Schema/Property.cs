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

using System.Diagnostics;
using System.Reflection;

namespace Realms.Schema
{
    [DebuggerDisplay("Name = {Name}, Type = {Type}")]
    public struct Property
    {
        public string Name { get; set; }

        public PropertyType Type { get; set; }

        public string ObjectType { get; set; }

        public bool IsNullable { get; set; }

        public bool IsObjectId { get; set; }

        public bool IsIndexed { get; set; }

        internal PropertyInfo PropertyInfo;
    }
}

