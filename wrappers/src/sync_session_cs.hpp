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

using ProgressCallbackT = void(void* state, uint64_t transferred_bytes, uint64_t transferrable_bytes);
using NotifyBeforeClientResetCallbackT = bool(SharedRealm& before_frozen, void* managed_sync_config);
using NotifyAfterClientResetCallbackT = bool(SharedRealm& before_frozen, SharedRealm& after, void* managed_sync_config, bool did_recover);

namespace realm {
namespace binding {
    extern std::function<ProgressCallbackT> s_progress_callback;
    extern std::function<NotifyBeforeClientResetCallbackT> s_notify_before_callback;
    extern std::function<NotifyAfterClientResetCallbackT> s_notify_after_callback;
}
}

#endif /* defined(SYNC_SESSION_CS_HPP) */
