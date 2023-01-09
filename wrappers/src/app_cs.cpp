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

using LogMessageCallbackT = void(void* managed_handler, realm_value_t message, util::Logger::Level level);
using UserCallbackT = void(void* tcs_ptr, SharedSyncUser* user, MarshaledAppError err);
using VoidCallbackT = void(void* tcs_ptr, MarshaledAppError err);
using StringCallbackT = void(void* tcs_ptr, realm_value_t response, MarshaledAppError err);
using ApiKeysCallbackT = void(void* tcs_ptr, UserApiKey* api_keys, size_t api_keys_len, MarshaledAppError err);

namespace realm {
    namespace binding {
        std::string s_framework;
        std::string s_framework_version;
        std::string s_sdk_version;
        std::string s_platform;
        std::string s_platform_version;
        std::string s_cpu_arch;
        std::string s_device_name;
        std::string s_device_version;

        std::function<LogMessageCallbackT> s_log_message_callback;
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

            util::Logger::Level log_level;

            void* managed_logger;

            void* managed_http_client;
        };

        class SyncLogger : public util::Logger {
        public:
            SyncLogger(void* delegate)
                : managed_logger(delegate)
            {
            }
        protected:
            void do_log(util::Logger::Level level, const std::string& message) override final {
                s_log_message_callback(managed_logger, to_capi(Mixed(message)), level);
            }
        private:
            void* managed_logger;
        };
    }
}

extern "C" {
    REALM_EXPORT void shared_app_initialize(uint16_t* framework, size_t framework_len,
        uint16_t* framework_version, size_t framework_version_len,
        uint16_t* sdk_version, size_t sdk_version_len,
        uint16_t* platform_version, size_t platform_version_len,
        uint16_t* cpu_arch, size_t cpu_arch_len,
        uint16_t* device_name, size_t device_name_len,
        uint16_t* device_version, size_t device_version_len,
        UserCallbackT* user_callback,
        VoidCallbackT* void_callback,
        StringCallbackT* string_callback,
        LogMessageCallbackT* log_message_callback,
        ApiKeysCallbackT* api_keys_callback)
    {
        s_framework = Utf16StringAccessor(framework, framework_len);
        s_framework_version = Utf16StringAccessor(framework_version, framework_version_len);
        s_sdk_version = Utf16StringAccessor(sdk_version, sdk_version_len);
        s_platform_version = Utf16StringAccessor(platform_version, platform_version_len);
        s_cpu_arch = Utf16StringAccessor(cpu_arch, cpu_arch_len);
        s_device_name = Utf16StringAccessor(device_name, device_name_len);
        s_device_version = Utf16StringAccessor(device_version, device_version_len);

#if REALM_ANDROID
        s_platform = "Android";
#elif REALM_WINDOWS
        s_platform = "Windows";
#elif REALM_UWP
        s_platform = "UWP";
#elif REALM_IOS
        s_platform = "iOS";
#elif REALM_PLATFORM_APPLE
        s_platform = "macOS";
#else
        s_platform = "Linux";
#endif

        s_user_callback = wrap_managed_callback(user_callback);
        s_void_callback = wrap_managed_callback(void_callback);
        s_string_callback = wrap_managed_callback(string_callback);
        s_log_message_callback = wrap_managed_callback(log_message_callback);
        s_api_keys_callback = wrap_managed_callback(api_keys_callback);

        realm::binding::s_can_call_managed = true;
    }

    REALM_EXPORT SharedApp* shared_app_create(AppConfiguration app_config, uint8_t* encryption_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            App::Config config;
            config.app_id = Utf16StringAccessor(app_config.app_id, app_config.app_id_len);

            // TODO: fix
            config.device_info.framework_name = s_framework;
            config.device_info.framework_version = s_framework_version;
            config.device_info.sdk_version = s_sdk_version;
            config.device_info.sdk = "Dotnet";
            config.device_info.platform = s_platform;
            config.device_info.platform_version = s_platform_version;
            config.device_info.cpu_arch = s_cpu_arch;
            config.device_info.device_name = s_device_name;
            config.device_info.device_version = s_device_version;

            config.transport = std::make_shared<HttpClientTransport>(app_config.managed_http_client);

            if (app_config.base_url != nullptr) {
                config.base_url = Utf16StringAccessor(app_config.base_url, app_config.base_url_len).to_string();
            }

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
            sync_client_config.log_level = app_config.log_level;
            sync_client_config.base_file_path = Utf16StringAccessor(app_config.base_file_path, app_config.base_file_path_len);

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

            void* managed_logger = app_config.managed_logger;
            if (managed_logger) {
                sync_client_config.logger_factory = [managed_logger](util::Logger::Level level) {
                    auto logger = std::make_shared<SyncLogger>(managed_logger);
                    logger->set_level_threshold(level);
                    return logger;
                };
            }

            return new SharedApp(App::get_shared_app(std::move(config), std::move(sync_client_config)));
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

    REALM_EXPORT size_t shared_app_sync_get_path_for_realm(SharedApp& app, SharedSyncUser& user, uint16_t* partition_buf, size_t partition_len, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            std::string path;
            if (partition_buf) {
                Utf16StringAccessor partition(partition_buf, partition_len);
                auto sync_config = SyncConfig(user, partition);
                path = app->sync_manager()->path_for_realm(std::move(sync_config));
            }
            else {
                // We're using flx, so we don't have a partition. However, the default
                // path in core is flx_sync_default while the .NET SDK has been using
                // `default`. To avoid losing users' data, we're passing `default` as
                // custom_file_name. We're passing a dummy partition to the SyncConfig
                // because Core asserts that the name when using FLX config is flx_sync_default.
                // TODO: revisit once https://github.com/realm/realm-core/issues/5473 is addressed.
                auto sync_config = SyncConfig(user, "\"\"");
                path = app->sync_manager()->path_for_realm(std::move(sync_config), util::Optional<std::string>("default"));
            }

            return stringdata_to_csharpstringbuffer(path, pathbuffer, pathbuffer_len);
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
