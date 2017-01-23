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
#include <realm/util/uri.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "sync/sync_manager.hpp"
#include "sync/sync_session.hpp"
#include "sync_session_cs.hpp"

using namespace realm;
using namespace realm::binding;

using SharedSyncSession = std::shared_ptr<SyncSession>;

namespace realm {
namespace binding {
    void (*s_refresh_access_token_callback)(std::shared_ptr<SyncUser>*, std::shared_ptr<SyncSession>*, const char* path, size_t path_len);
    void (*s_session_error_callback)(std::shared_ptr<SyncSession>*, int32_t error_code, const char* message, size_t message_len);
    void (*s_progress_callback)(size_t, uint64_t transferred_bytes, uint64_t transferrable_bytes);

    void bind_session(const std::string&, const realm::SyncConfig& config, std::shared_ptr<SyncSession> session)
    {
        s_refresh_access_token_callback(new std::shared_ptr<SyncUser>(config.user), new std::shared_ptr<SyncSession>(session), config.realm_url.c_str(), config.realm_url.size());
    }
    
    void handle_session_error(std::shared_ptr<SyncSession> session, SyncError error)
    {
        s_session_error_callback(new std::shared_ptr<SyncSession>(session), error.error_code.value(), error.message.c_str(), error.message.length());
    }
}
}
extern "C" {
REALM_EXPORT void realm_syncsession_refresh_access_token(SharedSyncSession& session, const uint16_t* token_buf, size_t token_len, const uint16_t* server_path_buf, size_t server_path_len, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        Utf16StringAccessor token(token_buf, token_len);
        Utf16StringAccessor server_path(server_path_buf, server_path_len);

        realm::util::Uri server_url(session->config().realm_url);
        server_url.set_path(server_path);

        session->refresh_access_token(token, server_url.recompose());
    });
}

REALM_EXPORT SharedSyncSession* realm_syncsession_get_from_realm(const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return new SharedSyncSession(SyncManager::shared().get_existing_active_session(realm->config().path));
    });
}

REALM_EXPORT std::shared_ptr<SyncUser>* realm_syncsession_get_user(const SharedSyncSession& session)
{
    return new std::shared_ptr<SyncUser>(session->user());
}

enum class CSharpSessionState : uint8_t {
    Active = 0,
    Inactive,
    Invalid
};

REALM_EXPORT CSharpSessionState realm_syncsession_get_state(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        if (!session) {
            return CSharpSessionState::Invalid;
        }
        switch (session->state()) {
        case SyncSession::PublicState::Inactive:
            return CSharpSessionState::Inactive;
        case SyncSession::PublicState::Error:
            return CSharpSessionState::Invalid;
        default:
            return CSharpSessionState::Active;
        }
    });
}
    
REALM_EXPORT size_t realm_syncsession_get_uri(const SharedSyncSession& session, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        std::string uri(session->full_realm_url().value_or(session->config().realm_url));
        return stringdata_to_csharpstringbuffer(uri, buffer, buffer_length);
    });
}
    
REALM_EXPORT SyncSession* realm_syncsession_get_raw_pointer(const SharedSyncSession& session)
{
    return session.get();
}

REALM_EXPORT void realm_syncsession_destroy(SharedSyncSession* session)
{
    delete session;
}
    
REALM_EXPORT void realm_install_syncsession_callbacks(decltype(s_refresh_access_token_callback) refresh_callback, decltype(s_session_error_callback) session_error_callback, decltype(s_progress_callback) progress_callback)
{
    s_refresh_access_token_callback = refresh_callback;
    s_session_error_callback = session_error_callback;
    s_progress_callback = progress_callback;
}
    
enum class CSharpNotifierType : uint8_t {
    Upload = 0,
    Download
};
    
REALM_EXPORT uint64_t realm_syncsession_register_progress_notifier(const SharedSyncSession& session, size_t managed_state, CSharpNotifierType direction, bool is_streaming, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto notifier_direction = direction == CSharpNotifierType::Upload
                                  ? SyncSession::NotifierType::upload
                                  : SyncSession::NotifierType::download;
        
        return session->register_progress_notifier([managed_state](uint64_t transferred, uint64_t transferrable) {
            s_progress_callback(managed_state, transferred, transferrable);
        }, notifier_direction, is_streaming);
    });
}
    
REALM_EXPORT void realm_syncsession_unregister_progress_notifier(const SharedSyncSession& session, uint64_t token, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        session->unregister_progress_notifier(token);
    });
}
    
}

