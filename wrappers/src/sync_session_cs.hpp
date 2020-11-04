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

#ifndef SYNC_SESSION_CS_HPP
#define SYNC_SESSION_CS_HPP

#include "realm_export_decls.hpp"
#include <realm/sync/config.hpp>

namespace realm {
namespace binding {
    extern void (*s_progress_callback)(void*, uint64_t transferred_bytes, uint64_t transferrable_bytes);

    void handle_session_error(std::shared_ptr<SyncSession> session, SyncError error);
}
}

#endif /* defined(SYNC_SESSION_CS_HPP) */
