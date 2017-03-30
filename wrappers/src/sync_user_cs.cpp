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

REALM_EXPORT SharedSyncUser* realm_get_sync_user(const uint16_t* identity_buf, size_t identity_len,
                                                 const uint16_t* refresh_token_buf, size_t refresh_token_len,
                                                 const uint16_t* auth_server_url_buf, size_t auth_server_url_len,
                                                 bool is_admin, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        Utf16StringAccessor identity(identity_buf, identity_len);
        Utf16StringAccessor refresh_token(refresh_token_buf, refresh_token_len);
        
        util::Optional<std::string> auth_server_url;
        if (auth_server_url_buf) {
            auth_server_url.emplace(Utf16StringAccessor(auth_server_url_buf, auth_server_url_len));
        }
        
        SyncUser::TokenType token_type = is_admin ? SyncUser::TokenType::Admin : SyncUser::TokenType::Normal;
        return new SharedSyncUser(SyncManager::shared().get_user(identity, refresh_token, auth_server_url, token_type));
    });
}
    
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
        auto users = SyncManager::shared().all_logged_in_users();
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
    
REALM_EXPORT SharedSyncUser* realm_get_logged_in_user(const uint16_t* identity_buf, size_t identity_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> SharedSyncUser* {
        Utf16StringAccessor identity(identity_buf, identity_len);
        auto ptr = SyncManager::shared().get_existing_logged_in_user(identity);
        if (ptr == nullptr) {
            return nullptr;
        }
        
        return new SharedSyncUser(std::move(ptr));
    });
}
    
REALM_EXPORT SharedSyncSession* realm_syncuser_get_session(SharedSyncUser& user, const uint16_t* path_buf, size_t path_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        Utf16StringAccessor path(path_buf, path_len);
        return new SharedSyncSession(user->session_for_on_disk_path(path));
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

REALM_EXPORT size_t realm_syncuser_get_server_url(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        const std::string& server_url(user->server_url());
        return stringdata_to_csharpstringbuffer(server_url, buffer, buffer_length);
    });
}
    
REALM_EXPORT SyncUser::State realm_syncuser_get_state(SharedSyncUser& user, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return user->state();
    });
}
    
REALM_EXPORT bool realm_syncuser_get_is_admin(SharedSyncUser& user)
{
    return user->is_admin();
}
    
REALM_EXPORT void realm_syncuser_set_is_admin(SharedSyncUser& user, bool is_admin, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        user->set_is_admin(is_admin);
    });
}
    
REALM_EXPORT void realm_syncuser_destroy(SharedSyncUser* user)
{
    delete user;
}

}
