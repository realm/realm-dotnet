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
        public RealmExceptionCodes code;
        public RealmExceptionCategories categories;
        public byte* messageBytes;
        public IntPtr messageLength;
        public IntPtr managedException;

        internal Exception Convert()
        {
            try
            {
                var message = (messageLength != IntPtr.Zero) ?
                    Encoding.UTF8.GetString(messageBytes, (int)messageLength)
                    : "No further information available";

                var innerException = GetInnerException();
                if (innerException != null)
                {
                    return new AggregateException(message, innerException);
                }

                return RealmException.Create(code, message, categories);
            }
            finally
            {
                NativeCommon.delete_pointer(messageBytes);
            }
        }

        private Exception? GetInnerException()
        {
            if (managedException != IntPtr.Zero)
            {
                var handle = GCHandle.FromIntPtr(managedException);
                var result = (Exception?)handle.Target;
                handle.Free();
                return result;
            }

            return null;
        }

        internal void ThrowIfNecessary()
        {
            if (code == RealmExceptionCodes.RLM_ERR_NONE)
            {
                return;
            }

            throw Convert();
        }
    }
}
