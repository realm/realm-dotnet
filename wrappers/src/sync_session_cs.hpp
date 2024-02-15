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

#include "marshalling.hpp"
#include <realm/sync/config.hpp>

namespace realm::binding {

using SharedSyncSession = std::shared_ptr<SyncSession>;
using SessionErrorCallbackT = void(SharedSyncSession* session, realm_sync_error error, void* managed_sync_config);
using ProgressCallbackT = void(void* state, uint64_t transferred_bytes, uint64_t transferrable_bytes, double progress_estimate);
using NotifyBeforeClientResetCallbackT = void*(SharedRealm& before_frozen, void* managed_sync_config);
using NotifyAfterClientResetCallbackT = void*(SharedRealm& before_frozen, SharedRealm& after, void* managed_sync_config, bool did_recover);

extern std::function<SessionErrorCallbackT> s_session_error_callback;
extern std::function<ProgressCallbackT> s_progress_callback;
extern std::function<NotifyBeforeClientResetCallbackT> s_notify_before_callback;
extern std::function<NotifyAfterClientResetCallbackT> s_notify_after_callback;

} // namespace realm::binding

