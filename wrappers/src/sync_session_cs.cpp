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
#include <realm/sync/protocol.hpp>
#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_session.hpp>
#include "sync_session_cs.hpp"

using namespace realm;
using namespace realm::binding;

using SharedSyncSession = std::shared_ptr<SyncSession>;

namespace realm {
namespace binding {
    void (*s_session_error_callback)(std::shared_ptr<SyncSession>*, int32_t error_code, const char* message, size_t message_len, std::pair<char*, char*>* user_info_pairs, int user_info_pairs_len, bool is_client_reset);
    void (*s_progress_callback)(void*, uint64_t transferred_bytes, uint64_t transferrable_bytes);
    void (*s_wait_callback)(void* task_completion_source, int32_t error_code, const char* message, size_t message_len);

    void handle_session_error(std::shared_ptr<SyncSession> session, SyncError error)
    {
        std::vector<std::pair<char*, char*>> user_info_pairs;

        for (const auto& p : error.user_info) {
            user_info_pairs.push_back(std::make_pair(const_cast<char*>(p.first.c_str()), const_cast<char*>(p.second.c_str())));
        }

        s_session_error_callback(new std::shared_ptr<SyncSession>(session), error.error_code.value(), error.message.c_str(), error.message.length(), user_info_pairs.data(), user_info_pairs.size(), error.is_client_reset_requested());
    }
}
}
extern "C" {

REALM_EXPORT std::shared_ptr<SyncUser>* realm_syncsession_get_user(const SharedSyncSession& session)
{
    if (session->user() == nullptr) {
        return nullptr;
    }

    return new std::shared_ptr<SyncUser>(session->user());
}

enum class CSharpSessionState : uint8_t {
    Active = 0,
    Inactive
};

REALM_EXPORT CSharpSessionState realm_syncsession_get_state(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        switch (session->state()) {
        case SyncSession::PublicState::Inactive:
            return CSharpSessionState::Inactive;
        default:
            return CSharpSessionState::Active;
        }
    });
}

REALM_EXPORT size_t realm_syncsession_get_path(const SharedSyncSession& session, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return stringdata_to_csharpstringbuffer(session->path(), buffer, buffer_length);
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

REALM_EXPORT void realm_syncsession_install_callbacks(decltype(s_session_error_callback) session_error_callback, decltype(s_progress_callback) progress_callback, decltype(s_wait_callback) wait_callback)
{
    s_session_error_callback = session_error_callback;
    s_progress_callback = progress_callback;
    s_wait_callback = wait_callback;
}

enum class CSharpNotifierType : uint8_t {
    Upload = 0,
    Download = 1
};

REALM_EXPORT uint64_t realm_syncsession_register_progress_notifier(const SharedSyncSession& session, void* managed_state, CSharpNotifierType direction, bool is_streaming, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto notifier_direction = direction == CSharpNotifierType::Upload
                                  ? SyncSession::NotifierType::upload
                                  : SyncSession::NotifierType::download;

        return session->register_progress_notifier([managed_state](uint64_t transferred, uint64_t transferable) {
            s_progress_callback(managed_state, transferred, transferable);
        }, notifier_direction, is_streaming);
    });
}

REALM_EXPORT void realm_syncsession_unregister_progress_notifier(const SharedSyncSession& session, uint64_t token, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        session->unregister_progress_notifier(token);
    });
}

REALM_EXPORT void realm_syncsession_wait(const SharedSyncSession& session, void* task_completion_source, CSharpNotifierType direction, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        auto waiter = [task_completion_source](std::error_code error) {
            s_wait_callback(task_completion_source, error.value(), error.message().c_str(), error.message().length());
        };

        if (direction == CSharpNotifierType::Upload) {
            session->wait_for_upload_completion(waiter);
        } else {
            session->wait_for_download_completion(waiter);
        }
    });
}

REALM_EXPORT void realm_syncsession_report_error_for_testing(const SharedSyncSession& session, int err, const uint16_t* message_buf, size_t message_len, bool is_fatal)
{
    Utf16StringAccessor message(message_buf, message_len);
    std::error_code error_code(err, realm::sync::protocol_error_category());
    SyncSession::OnlyForTesting::handle_error(*session, SyncError{error_code, std::move(message), is_fatal});
}

REALM_EXPORT void realm_syncsession_stop(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        session->log_out();
    });
}

REALM_EXPORT void realm_syncsession_start(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        session->revive_if_needed();
    });
}

}

