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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Realms.Exceptions;

namespace Realms
{
    [StructLayout(LayoutKind.Sequential)]
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:AccessibleFieldsMustBeginWithUpperCaseLetter")]
    internal unsafe struct NativeException
    {
        public RealmExceptionCodes type;
        public char* messageBytes;
        public IntPtr messageLength;

        internal Exception Convert(Func<RealmExceptionCodes, Exception> overrider = null)
        {
            var message = (messageLength != IntPtr.Zero) ?
                new string(messageBytes, 0, (int)messageLength)
                : "No further information available";
            NativeCommon.delete_pointer(messageBytes);

            return overrider?.Invoke(type) ?? RealmException.Create(type, message);
        }

        internal void ThrowIfNecessary(Func<RealmExceptionCodes, Exception> overrider = null)
        {
            if (type == RealmExceptionCodes.NoError)
            {
                return;
            }

            throw Convert(overrider);
        }
    }
}