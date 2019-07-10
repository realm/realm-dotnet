////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

#pragma once

#include "realm_export_decls.hpp"

namespace realm {
    class SyncUser;

    struct SyncConfiguration
    {
        std::shared_ptr<SyncUser>* user;

        uint16_t* url;
        size_t url_len;

        bool client_validate_ssl;

        uint16_t* trusted_ca_path;
        size_t trusted_ca_path_len;

        bool is_partial;
        uint16_t* partial_sync_identifier;
        size_t partial_sync_identifier_len;
    };
    
    namespace binding {
        REALM_EXPORT bool has_feature(StringData feature);
    }
}
