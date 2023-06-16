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

using System.Runtime.InteropServices;
using Realms.Schema;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SchemaProperty
    {
        public StringValue name;

        public StringValue object_type;

        public StringValue link_origin_property_name;

        public PropertyType type;

        private byte is_primary_byte;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Native struct field")]
        public bool is_primary
        {
            get => is_primary_byte == 1;
            set => is_primary_byte = (byte)(value ? 1 : 0);
        }

        public IndexType index;
    }
}
