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

#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "sync/sync_manager.hpp"
#include "sync/sync_user.hpp"
#include "sync/sync_session.hpp"

using namespace realm;
using namespace realm::binding;

using SharedSyncUser = std::shared_ptr<SyncUser>;
using SharedSyncSession = std::shared_ptr<SyncSession>;

extern "C" {

REALM_EXPORT SharedSyncUser* realm_get_current_sync_user(NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> SharedSyncUser* {
        auto ptr = SyncManager::shared().get_current_user();
        if (ptr == nullptr) {
            return nullptr;
        }

        return new SharedSyncUser(std::move(ptr));
    });
}

REALM_EXPORT size_t realm_get_logged_in_users(SharedSyncUser** buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> size_t {
        auto users = SyncManager::shared().all_users();
        if (users.size() > buffer_length) {
            return users.size();
        }

        if (users.size() <= 0) {
            return 0;
        }

        for (size_t i = 0; i < users.size(); i++) {
            buffer[i] = new SharedSyncUser(users.at(i));
        }

        return users.size();
    });
}

REALM_EXPORT void realm_syncuser_log_out(SharedSyncUser& user, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        user->log_out();
    });
}

REALM_EXPORT size_t realm_syncuser_get_identity(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        std::string identity(user->identity());
        return stringdata_to_csharpstringbuffer(identity, buffer, buffer_length);
    });
}

REALM_EXPORT size_t realm_syncuser_get_refresh_token(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        std::string refresh_token(user->refresh_token());
        return stringdata_to_csharpstringbuffer(refresh_token, buffer, buffer_length);
    });
}

REALM_EXPORT SyncUser::State realm_syncuser_get_state(SharedSyncUser& user, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return user->state();
    });
}

REALM_EXPORT void realm_syncuser_destroy(SharedSyncUser* user)
{
    delete user;
}

}
