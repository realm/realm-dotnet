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
#include "debug.hpp"
#include "realm_export_decls.hpp"

#include <realm/object-store/impl/realm_coordinator.hpp>

#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_user.hpp>
#include <realm/object-store/sync/app.hpp>

namespace realm {

using DebugLoggerT = void(*)(void* utf8Str, size_t strLen);
static DebugLoggerT debug_log_function = nullptr;

void debug_log(const std::string message)
{
  // second check against -1 based on suspicions from stack traces of this as sentinel value
  if (debug_log_function != nullptr && debug_log_function != reinterpret_cast<DebugLoggerT>(-1))
    debug_log_function((void*)message.data(), message.size());
}

}

extern "C" {
    REALM_EXPORT void set_debug_logger(realm::DebugLoggerT debug_logger)
    {
      realm::debug_log_function = debug_logger;
    }

    REALM_EXPORT void realm_reset_for_testing()
    {
        realm::_impl::RealmCoordinator::clear_all_caches();
    }
}
