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

namespace Realms.Native
{
    /// <summary>
    /// .NET booleans have a troubled history when it comes to P/Invoke.
    /// Mono and the CoreCLR treat them as equivalent to the C bool type,
    /// but .NET Framework defaults to the Win32 BOOL type, which is int-sized.
    /// Normally we get around this with [MarshalAs(UnmanagedType.U1)], but that requires runtime marshaling.
    /// This is just a helper type that allows us to use the byte type which matches C bool, but preserve boolean semantics.
    /// </summary>
    internal struct NativeBool
    {
        private byte _value;

        public static implicit operator bool(NativeBool value) => value._value == 1;

        public static implicit operator NativeBool(bool value) => new() { _value = (byte)(value ? 1 : 0) };
    }
}
