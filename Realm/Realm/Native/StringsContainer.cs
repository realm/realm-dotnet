////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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

namespace Realms.Native
{
    // This is a wrapper struct around MarshaledVector since P/Invoke doesn't like it
    // when the MarshaledVector is returned as the top-level return value from a native
    // function. This only manifests in .NET Framework and is not an issue with Mono/.NET.
    // The native return value is MarshaledVector without the wrapper because they are binary
    // compatible.
    [StructLayout(LayoutKind.Sequential)]
    internal struct StringsContainer
    {
        public MarshaledVector<StringValue> Strings;
    }
}
