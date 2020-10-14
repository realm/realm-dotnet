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
using System.Text;

namespace Realms.Sync.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct AppError
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool is_null;

        private byte* message_buf;
        private IntPtr message_len;

        private byte* error_category_buf;
        private IntPtr error_category_len;

        private byte* logs_link_buf;
        private IntPtr logs_link_len;

        public int http_status_code;

        public string Message => message_buf == null ? null : Encoding.UTF8.GetString(message_buf, (int)message_len);

        public string ErrorCategory => error_category_buf == null ? null : Encoding.UTF8.GetString(error_category_buf, (int)error_category_len);

        public string LogsLink => logs_link_buf == null ? null : Encoding.UTF8.GetString(logs_link_buf, (int)logs_link_len);
    }
}
