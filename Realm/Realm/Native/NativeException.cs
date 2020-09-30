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
using System.Runtime.InteropServices;
using System.Text;
using Realms.Exceptions;

namespace Realms
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct NativeException
    {
        public RealmExceptionCodes type;
        public byte* messageBytes;
        public IntPtr messageLength;
        public byte* detailBytes;
        public IntPtr detailLength;

        internal Exception Convert(Func<RealmExceptionCodes, Exception> overrider = null)
        {
            var message = (messageLength != IntPtr.Zero) ?
                            Encoding.UTF8.GetString(messageBytes, (int)messageLength)
                            : "No further information available";
            NativeCommon.delete_pointer(messageBytes);

            var detail = (detailLength != IntPtr.Zero) ? Encoding.UTF8.GetString(detailBytes, (int)detailLength) : null;
            NativeCommon.delete_pointer(detailBytes);

            return overrider?.Invoke(type) ?? RealmException.Create(type, message, detail);
        }

        internal void ThrowIfNecessary(Func<RealmExceptionCodes, Exception> overrider = null)
        {
            if (type == RealmExceptionCodes.NoError)
            {
                return;
            }

            throw Convert(overrider);
        }

        internal void ThrowIfNecessary(GCHandle handleToFree)
        {
            if (type == RealmExceptionCodes.NoError)
            {
                return;
            }

            handleToFree.Free();
            throw Convert();
        }
    }
}