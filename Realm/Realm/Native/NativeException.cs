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
        public IntPtr detail;
        public IntPtr detailLength;

        internal Exception Convert(Func<RealmExceptionCodes, Exception> overrider = null)
        {
            try
            {
                var overridden = overrider?.Invoke(type);
                if (overridden != null)
                {
                    return overridden;
                }

                var message = (messageLength != IntPtr.Zero) ?
                    Encoding.UTF8.GetString(messageBytes, (int)messageLength)
                    : "No further information available";

                var innerException = GetInnerException();
                if (innerException != null)
                {
                    return new AggregateException(message, innerException);
                }

                var detailMessage = detail != IntPtr.Zero ? Encoding.UTF8.GetString((byte*)detail, (int)detailLength) : null;
                return RealmException.Create(type, message, detailMessage);
            }
            finally
            {
                NativeCommon.delete_pointer(messageBytes);
                if (type != RealmExceptionCodes.RealmDotNetExceptionDuringCallback && detail != IntPtr.Zero)
                {
                    NativeCommon.delete_pointer((byte*)detail);
                }
            }
        }

        private Exception GetInnerException()
        {
            if (type == RealmExceptionCodes.RealmDotNetExceptionDuringCallback)
            {
                var handle = GCHandle.FromIntPtr(detail);
                var result = (Exception)handle.Target;
                handle.Free();
                return result;
            }

            return null;
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
