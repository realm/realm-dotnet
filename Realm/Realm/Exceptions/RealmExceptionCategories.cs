﻿////////////////////////////////////////////////////////////////////////////
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

using System;

namespace Realms.Exceptions
{
    [Flags]
    internal enum RealmExceptionCategories
    {
        RLM_ERR_CAT_LOGIC = 0x0002,
        RLM_ERR_CAT_RUNTIME = 0x0004,
        RLM_ERR_CAT_INVALID_ARG = 0x0008,
        RLM_ERR_CAT_FILE_ACCESS = 0x0010,
        RLM_ERR_CAT_SYSTEM_ERROR = 0x0020,
        RLM_ERR_CAT_APP_ERROR = 0x0040,
        RLM_ERR_CAT_CLIENT_ERROR = 0x0080,
        RLM_ERR_CAT_JSON_ERROR = 0x0100,
        RLM_ERR_CAT_SERVICE_ERROR = 0x0200,
        RLM_ERR_CAT_HTTP_ERROR = 0x0400,
        RLM_ERR_CAT_CUSTOM_ERROR = 0x0800,
        RLM_ERR_CAT_WEBSOCKET_ERROR = 0x1000,
    }
}
