////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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
#include "marshalling.hpp"
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "shared_realm_cs.hpp"
#include "sync_session_cs.hpp"
#include "transport_cs.hpp"
#include "debug.hpp"
#include "app_cs.hpp"

#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/app_credentials.hpp>
#include <realm/object-store/sync/app.hpp>
#include <realm/sync/config.hpp>
#include <realm/object-store/thread_safe_reference.hpp>
#include <realm/object-store/sync/sync_session.hpp>

using namespace realm;
using namespace realm::binding;
using namespace app;

using SharedSyncUser = std::shared_ptr<SyncUser>;

using UserCallbackT = void(void* tcs_ptr, SharedSyncUser* user, MarshaledAppError err);
using VoidCallbackT = void(void* tcs_ptr, MarshaledAppError err);
using StringCallbackT = void(void* tcs_ptr, realm_value_t response, MarshaledAppError err);
using ApiKeysCallbackT = void(void* tcs_ptr, UserApiKey* api_keys, size_t api_keys_len, MarshaledAppError err);

namespace realm {
    namespace binding {
        std::string s_framework;
        std::string s_framework_version;
        std::string s_sdk_version;
        std::string s_platform_version;
        std::string s_device_name;
        std::string s_device_version;
        std::string s_bundle_id;

        std::function<UserCallbackT> s_user_callback;
        std::function<VoidCallbackT> s_void_callback;
        std::function<StringCallbackT> s_string_callback;
        std::function<ApiKeysCallbackT> s_api_keys_callback;

        struct AppConfiguration
        {
            uint16_t* app_id;
            size_t app_id_len;

            uint16_t* base_file_path;
            size_t base_file_path_len;

            uint16_t* base_url;
            size_t base_url_len;

            uint16_t* local_app_name;
            size_t local_app_name_len;

            uint16_t* local_app_version;
            size_t local_app_version_len;

            uint64_t request_timeout_ms;

            realm::SyncClientConfig::MetadataMode metadata_mode;

            bool metadata_mode_has_value;

            void* managed_http_client;

            uint64_t sync_connect_timeout_ms;

            uint64_t sync_connection_linger_time_ms;

            uint64_t sync_ping_keep_alive_period_ms;

            uint64_t sync_pong_keep_alive_timeout_ms;

            uint64_t sync_fast_reconnect_limit;

            bool use_cache;
        };
    }
}

extern "C" {
    REALM_EXPORT void shared_app_initialize(uint16_t* framework, size_t framework_len,
        uint16_t* framework_version, size_t framework_version_len,
        uint16_t* sdk_version, size_t sdk_version_len,
        uint16_t* platform_version, size_t platform_version_len,
        uint16_t* device_name, size_t device_name_len,
        uint16_t* device_version, size_t device_version_len,
        uint16_t* bundle_id, size_t bundle_id_len,
        UserCallbackT* user_callback,
        VoidCallbackT* void_callback,
        StringCallbackT* string_callback,
        ApiKeysCallbackT* api_keys_callback)
    {
        s_framework = Utf16StringAccessor(framework, framework_len);
        s_framework_version = Utf16StringAccessor(framework_version, framework_version_len);
        s_sdk_version = Utf16StringAccessor(sdk_version, sdk_version_len);
        s_platform_version = Utf16StringAccessor(platform_version, platform_version_len);
        s_device_name = Utf16StringAccessor(device_name, device_name_len);
        s_device_version = Utf16StringAccessor(device_version, device_version_len);
        s_bundle_id = Utf16StringAccessor(bundle_id, bundle_id_len);

        s_user_callback = wrap_managed_callback(user_callback);
        s_void_callback = wrap_managed_callback(void_callback);
        s_string_callback = wrap_managed_callback(string_callback);
        s_api_keys_callback = wrap_managed_callback(api_keys_callback);

        realm::binding::s_can_call_managed = true;
    }

    REALM_EXPORT SharedApp* shared_app_create(AppConfiguration app_config, uint8_t* encryption_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            App::Config config;
            config.app_id = Utf16StringAccessor(app_config.app_id, app_config.app_id_len);

            config.device_info.framework_name = s_framework;
            config.device_info.framework_version = s_framework_version;
            config.device_info.sdk_version = s_sdk_version;
            config.device_info.sdk = "Dotnet";
            config.device_info.platform_version = s_platform_version;
            config.device_info.device_name = s_device_name;
            config.device_info.device_version = s_device_version;
            config.device_info.bundle_id = s_bundle_id;

            config.transport = std::make_shared<HttpClientTransport>(app_config.managed_http_client);
            config.base_url = Utf16StringAccessor(app_config.base_url, app_config.base_url_len).to_string();

            if (app_config.local_app_name != nullptr) {
                config.local_app_name = Utf16StringAccessor(app_config.local_app_name, app_config.local_app_name_len).to_string();
            }

            if (app_config.local_app_version != nullptr) {
                config.local_app_version = Utf16StringAccessor(app_config.local_app_version, app_config.local_app_version_len).to_string();
            }

            if (app_config.request_timeout_ms > 0) {
                config.default_request_timeout_ms = app_config.request_timeout_ms;
            }

            SyncClientConfig sync_client_config;
            sync_client_config.base_file_path = Utf16StringAccessor(app_config.base_file_path, app_config.base_file_path_len);
            sync_client_config.timeouts.connection_linger_time = app_config.sync_connection_linger_time_ms;
            sync_client_config.timeouts.connect_timeout = app_config.sync_connect_timeout_ms;
            sync_client_config.timeouts.fast_reconnect_limit = app_config.sync_fast_reconnect_limit;
            sync_client_config.timeouts.ping_keepalive_period = app_config.sync_ping_keep_alive_period_ms;
            sync_client_config.timeouts.pong_keepalive_timeout = app_config.sync_pong_keep_alive_timeout_ms;

            if (app_config.metadata_mode_has_value) {
                sync_client_config.metadata_mode = app_config.metadata_mode;
            }
            else {
#if REALM_PLATFORM_APPLE && !TARGET_OS_SIMULATOR && !TARGET_OS_MACCATALYST
                sync_client_config.metadata_mode = SyncManager::MetadataMode::Encryption;
#else
                sync_client_config.metadata_mode = SyncManager::MetadataMode::NoEncryption;
#endif
            }

            if (encryption_key) {
                auto& key = *reinterpret_cast<std::array<char, 64>*>(encryption_key);
                sync_client_config.custom_encryption_key = std::vector<char>(key.begin(), key.end());
            }

            SharedApp app = app_config.use_cache
                ? App::get_shared_app(std::move(config), std::move(sync_client_config))
                : App::get_uncached_app(std::move(config), std::move(sync_client_config));

            return new SharedApp(app);
        });
    }

    REALM_EXPORT SharedSyncUser* shared_app_get_current_user(SharedApp& app, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> SharedSyncUser* {
            auto ptr = app->current_user();
            if (ptr == nullptr) {
                return nullptr;
            }

            return new SharedSyncUser(ptr);
        });
    }

    REALM_EXPORT size_t shared_app_get_logged_in_users(SharedApp& app, SharedSyncUser** buffer, size_t buffer_length, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> size_t {
            auto users = app->all_users();
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

    REALM_EXPORT void shared_app_switch_user(SharedApp& app, SharedSyncUser& user, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            app->switch_user(user);
        });
    }

    REALM_EXPORT void shared_app_login_user(SharedApp& app, Credentials credentials, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto app_credentials = credentials.to_app_credentials();
            app->log_in_with_credentials(app_credentials, get_user_callback_handler(tcs_ptr));
        });
}

    REALM_EXPORT SharedSyncUser* shared_app_get_user_for_testing(
        SharedApp& app,
        uint16_t* id_buf, size_t id_len,
        uint16_t* refresh_token_buf, size_t refresh_token_len,
        uint16_t* access_token_buf, size_t access_token_len,
        NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            Utf16StringAccessor id(id_buf, id_len);
            Utf16StringAccessor refresh_token(refresh_token_buf, refresh_token_len);
            Utf16StringAccessor access_token(access_token_buf, access_token_len);
            return new SharedSyncUser(
                app->sync_manager()->get_user(
                    id,
                    refresh_token,
                    access_token,
                    "testing",
                    "my-device-id"));
        });
    }

    REALM_EXPORT void shared_app_remove_user(SharedApp& app, SharedSyncUser& user, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            app->remove_user(user, [tcs_ptr](util::Optional<AppError>) {
                // ignore errors
                s_void_callback(tcs_ptr, MarshaledAppError());
            });
        });
    }

    REALM_EXPORT void shared_app_delete_user(SharedApp& app, SharedSyncUser& user, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            app->delete_user(user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT bool shared_app_sync_immediately_run_file_actions(SharedApp& app, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            std::string path(Utf16StringAccessor(pathbuffer, pathbuffer_len));
            return app->sync_manager()->immediately_run_file_actions(path);
        });
    }

    REALM_EXPORT void shared_app_sync_reconnect(SharedApp& app)
    {
        app->sync_manager()->reconnect();
    }

    REALM_EXPORT void shared_app_destroy(SharedApp* app)
    {
        delete app;
    }

    REALM_EXPORT void shared_app_reset_for_testing(SharedApp& app) {
        auto users = app->all_users();
        for (size_t i = 0; i < users.size(); i++) {
            auto &user = users[i];
            user->log_out();
        }

        while (app->sync_manager()->has_existing_sessions()) {
            sleep_ms(5);
        }

        bool did_reset = false;
        while (!did_reset) {
            try {
                app->sync_manager()->reset_for_testing();
                did_reset = true;
            }
            catch (...) {

            }
        }

        App::clear_cached_apps();
    }

    REALM_EXPORT size_t shared_app_get_base_file_path(SharedApp& app, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            std::string base_file_path(app->sync_manager()->config().base_file_path);
            return stringdata_to_csharpstringbuffer(base_file_path, buffer, buffer_length);
        });
    }

    REALM_EXPORT realm_string_t shared_app_get_base_uri(SharedApp& app, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return to_capi(app->base_url());
        });
    }

    REALM_EXPORT realm_string_t shared_app_get_id(SharedApp& app, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return to_capi(app->config().app_id);
        });
    }

    REALM_EXPORT bool shared_app_is_same_instance(SharedApp& lhs, SharedApp& rhs, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            return lhs == rhs;  // just compare raw pointers inside the smart pointers
        });
    }

#pragma region EmailPassword

    REALM_EXPORT void shared_app_email_register_user(SharedApp& app, uint16_t* username_buf, size_t username_len, uint16_t* password_buf, size_t password_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            Utf16StringAccessor username(username_buf, username_len);
            Utf16StringAccessor password(password_buf, password_len);
            app->provider_client<App::UsernamePasswordProviderClient>().register_email(username, password, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void shared_app_email_confirm_user(SharedApp& app, uint16_t* token_buf, size_t token_len, uint16_t* token_id_buf, size_t token_id_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            Utf16StringAccessor token(token_buf, token_len);
            Utf16StringAccessor token_id(token_id_buf, token_id_len);
            app->provider_client<App::UsernamePasswordProviderClient>().confirm_user(token, token_id, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void shared_app_email_resend_confirmation_email(SharedApp& app, uint16_t* email_buf, size_t email_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            Utf16StringAccessor email(email_buf, email_len);
            app->provider_client<App::UsernamePasswordProviderClient>().resend_confirmation_email(email, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void shared_app_email_send_reset_password_email(SharedApp& app, uint16_t* email_buf, size_t email_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            Utf16StringAccessor email(email_buf, email_len);
            app->provider_client<App::UsernamePasswordProviderClient>().send_reset_password_email(email, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void shared_app_email_reset_password(SharedApp& app, uint16_t* password_buf, size_t password_len, uint16_t* token_buf, size_t token_len, uint16_t* token_id_buf, size_t token_id_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            Utf16StringAccessor password(password_buf, password_len);
            Utf16StringAccessor token(token_buf, token_len);
            Utf16StringAccessor token_id(token_id_buf, token_id_len);
            app->provider_client<App::UsernamePasswordProviderClient>().reset_password(password, token, token_id, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void shared_app_email_call_reset_password_function(SharedApp& app, uint16_t* username_buf, size_t username_len, uint16_t* password_buf, size_t password_len, uint16_t* args_buf, size_t args_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            Utf16StringAccessor username(username_buf, username_len);
            Utf16StringAccessor password(password_buf, password_len);
            Utf16StringAccessor serialized_args(args_buf, args_len);

            auto args = static_cast<bson::BsonArray>(bson::parse(serialized_args.to_string()));
            app->provider_client<App::UsernamePasswordProviderClient>().call_reset_password_function(std::move(username), std::move(password), std::move(args), get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void shared_app_clear_cached_apps(NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&]() {
            app::App::clear_cached_apps();
        });
    }

#pragma endregion

}
