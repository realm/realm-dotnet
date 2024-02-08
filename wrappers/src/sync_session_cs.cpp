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
#include <realm/sync/client_base.hpp>

namespace realm::binding {
enum class NotifiableProperty : uint8_t {
    ConnectionState = 0,
};

using WaitCallbackT = void(void* task_completion_source, int32_t error_code, realm_value_t message);
using PropertyChangedCallbackT = void(void* managed_session_handle, NotifiableProperty property);

std::function<SessionErrorCallbackT> s_session_error_callback;
std::function<ProgressCallbackT> s_progress_callback;
std::function<WaitCallbackT> s_wait_callback;
std::function<PropertyChangedCallbackT> s_property_changed_callback;
std::function<NotifyBeforeClientResetCallbackT> s_notify_before_callback;
std::function<NotifyAfterClientResetCallbackT> s_notify_after_callback;

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
        case SyncSession::State::Inactive:
        case SyncSession::State::Dying:
        case SyncSession::State::Paused:
            return CSharpSessionState::Inactive;
        default:
            return CSharpSessionState::Active;
        }
    });
}

REALM_EXPORT SyncSession::ConnectionState realm_syncsession_get_connection_state(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return session->connection_state();
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

REALM_EXPORT void realm_syncsession_install_callbacks(SessionErrorCallbackT* session_error_callback, ProgressCallbackT* progress_callback, WaitCallbackT* wait_callback, PropertyChangedCallbackT* property_changed_callback, NotifyBeforeClientResetCallbackT notify_before, NotifyAfterClientResetCallbackT notify_after)
{
    s_session_error_callback = wrap_managed_callback(session_error_callback);
    s_progress_callback = wrap_managed_callback(progress_callback);
    s_wait_callback = wrap_managed_callback(wait_callback);
    s_property_changed_callback = wrap_managed_callback(property_changed_callback);
    s_notify_before_callback = wrap_managed_callback(notify_before);
    s_notify_after_callback = wrap_managed_callback(notify_after);

    realm::binding::s_can_call_managed = true;
}

enum class CSharpNotifierType : uint8_t {
    Upload = 0,
    Download = 1
};

REALM_EXPORT uint64_t realm_syncsession_register_progress_notifier(const SharedSyncSession& session, void* managed_state, CSharpNotifierType direction, bool is_streaming, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto notifier_direction = direction == CSharpNotifierType::Upload
                                  ? SyncSession::ProgressDirection::upload
                                  : SyncSession::ProgressDirection::download;

        return session->register_progress_notifier([managed_state](uint64_t transferred, uint64_t transferable, double progress_estimate) {
            s_progress_callback(managed_state, progress_estimate);
        }, notifier_direction, is_streaming);
    });
}

REALM_EXPORT void realm_syncsession_unregister_progress_notifier(const SharedSyncSession& session, uint64_t token, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        session->unregister_progress_notifier(token);
    });
}

typedef struct PropertyChangedNotificationToken {
    uint64_t connection_state;
} PropertyChangedNotificationToken;

REALM_EXPORT PropertyChangedNotificationToken realm_syncsession_register_property_changed_callback(const SharedSyncSession& session, void* managed_session_handle, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto connection_state_token = session->register_connection_change_callback([managed_session_handle](realm::SyncSession::ConnectionState old_state, realm::SyncSession::ConnectionState new_state) {
            s_property_changed_callback(managed_session_handle, NotifiableProperty::ConnectionState);
        });

        PropertyChangedNotificationToken notification_token { connection_state_token };
        return notification_token;
    });
}

REALM_EXPORT void realm_syncsession_unregister_property_changed_callback(const SharedSyncSession& session, PropertyChangedNotificationToken tokens, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        session->unregister_connection_change_callback(tokens.connection_state);
    });
}

REALM_EXPORT void realm_syncsession_wait(const SharedSyncSession& session, void* task_completion_source, CSharpNotifierType direction, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        auto waiter = [task_completion_source](realm::Status status) {
            s_wait_callback(task_completion_source, status.code(), to_capi_value(status.reason()));
        };

        if (direction == CSharpNotifierType::Upload) {
            session->wait_for_upload_completion(waiter);
        } else {
            session->wait_for_download_completion(waiter);
        }
    });
}

enum class SessionErrorCategory : uint8_t {
    ClientError = 0,
    SessionError = 1
};

REALM_EXPORT void realm_syncsession_report_error_for_testing(const SharedSyncSession& session, int err, const uint16_t* message_buf, size_t message_len, bool is_fatal, int server_requests_action)
{
    Utf16StringAccessor message(message_buf, message_len);
    std::error_code error_code;

    sync::ProtocolErrorInfo protocol_error(err, message, is_fatal);
    sync::SessionErrorInfo error(protocol_error);
    error.server_requests_action = static_cast<realm::sync::ProtocolErrorInfo::Action>(server_requests_action);

    SyncSession::OnlyForTesting::handle_error(*session, std::move(error));
}

REALM_EXPORT void realm_syncsession_stop(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        session->pause();
    });
}

REALM_EXPORT void realm_syncsession_start(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        session->resume();
    });
}

REALM_EXPORT void realm_syncsession_shutdown_and_wait(const SharedSyncSession& session, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        session->shutdown_and_wait();
    });
}

} // extern "C"
} // namespace realm::binding
