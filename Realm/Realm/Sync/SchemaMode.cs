////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

namespace Realms.Sync
{
    /// <summary>
    /// To see details of each mode check its mirroring definition in <see href="https://github.com/realm/realm-core/blob/0976706c26ce24866e6be6c165b6c6192fb663ed/src/realm/object-store/shared_realm.hpp#L61"> core</see>.
    /// </summary>
    internal enum SchemaMode : byte
    {
        // N.B. the values must match their representation in core!
        AdditiveDiscovered = 4,
        AdditiveExplicit
    }
}