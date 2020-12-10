////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
using Realms.Helpers;

namespace Realms.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FindAndModifyOptions
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        private string projection_buf;
        private IntPtr projection_len;

        [MarshalAs(UnmanagedType.LPWStr)]
        private string sort_buf;
        private IntPtr sort_len;

        [MarshalAs(UnmanagedType.U1)]
        private bool upsert;

        [MarshalAs(UnmanagedType.U1)]
        private bool return_new_document;

        private Int64 limit;

        public static FindAndModifyOptions Find(object projection, object sort, Int64? limit = null)
        {
            var result = new FindAndModifyOptions(projection, sort);
            if (limit.HasValue)
            {
                result.limit = limit.Value;
            }

            return result;
        }

        public static FindAndModifyOptions FindAndModify(object projection, object sort, bool upsert = false, bool returnNewDocument = false)
        {
            var result = new FindAndModifyOptions(projection, sort);
            result.upsert = upsert;
            result.return_new_document = returnNewDocument;
            return result;
        }

        private FindAndModifyOptions(object projection, object sort)
        {
            projection_buf = projection?.ToNativeJson();
            projection_len = projection_buf.IntPtrLength();

            sort_buf = sort?.ToNativeJson();
            sort_len = sort_buf.IntPtrLength();

            upsert = false;
            return_new_document = false;
            limit = 0;
        }
    }
}
