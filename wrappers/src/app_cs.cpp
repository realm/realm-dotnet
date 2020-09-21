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

#include "sync/async_open_task.hpp"
#include "sync/sync_manager.hpp"
#include "sync/app.hpp"
#include "sync/sync_config.hpp"
#include "thread_safe_reference.hpp"
#include "sync/sync_session.hpp"

using namespace realm;
using namespace realm::binding;
using namespace app;

using LogMessageDelegate = void(const char* message, size_t message_len, util::Logger::Level level);
using SharedSyncUser = std::shared_ptr<SyncUser>;
using SharedSyncSession = std::shared_ptr<SyncSession>;
using SharedAsyncOpenTask = std::shared_ptr<AsyncOpenTask>;

namespace realm {
    namespace binding {
        std::string s_platform;
        std::string s_platform_version;
        std::string s_sdk_version;
        
        void (*s_open_realm_callback)(void* task_completion_source, ThreadSafeReference* ref, int32_t error_code, const char* message, size_t message_len);

        struct AppConfig
        {
            uint16_t* app_id;
            size_t app_id_len;

            uint16_t* base_url;
            size_t base_url_len;

            uint16_t* local_app_name;
            size_t local_app_name_len;

            uint16_t* local_app_version;
            size_t local_app_version_len;

            uint64_t request_timeout_ms;
        };

        struct SyncClientConfiguration
        {
            // V10TODO: Why is that needed?
            // uint16_t* base_path;
            // size_t base_path_len;
            
            realm::SyncClientConfig::MetadataMode* metadata_mode;

            const char* encryption_key_buf;

            bool reset_metadata_on_error;

            util::Logger::Level log_level;

            LogMessageDelegate* log_delegate;

            // V10TODO - do we need those?
            // std::string user_agent_binding_info;
            // std::string user_agent_application_info;
        };

        struct SyncConfiguration
        {
            SharedSyncUser* user;

            uint16_t* url;
            size_t url_len;

            realm::ClientResyncMode client_resync_mode;
        };

        class SyncLogger : public util::RootLogger {
        public:
            SyncLogger(LogMessageDelegate* delegate)
                : m_log_message_delegate(delegate)
            {
            }

            void do_log(util::Logger::Level level, std::string message) {
                m_log_message_delegate(message.c_str(), message.length(), level);
            }
        private:
            LogMessageDelegate* m_log_message_delegate;
        };

        class SyncLoggerFactory : public realm::SyncLoggerFactory {
        public:
            SyncLoggerFactory(LogMessageDelegate* delegate)
                : m_log_message_delegate(delegate)
            {
            }

            std::unique_ptr<util::Logger> make_logger(util::Logger::Level level)
            {
                auto logger = std::make_unique<SyncLogger>(m_log_message_delegate);
                logger->set_level_threshold(level);
                return std::unique_ptr<util::Logger>(logger.release());
            }
        private:
            LogMessageDelegate* m_log_message_delegate;
        };
    }

    Realm::Config get_shared_realm_config(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key)
    {
        Realm::Config config;
        config.schema_mode = SchemaMode::Additive;

        if (objects_length > 0) {
            config.schema = create_schema(objects, objects_length, properties);
        }

        config.schema_version = configuration.schema_version;
        config.max_number_of_active_versions = configuration.max_number_of_active_versions;

        std::string realm_url(Utf16StringAccessor(sync_configuration.url, sync_configuration.url_len));

        config.sync_config = std::make_shared<SyncConfig>(*sync_configuration.user, realm_url);
        config.sync_config->error_handler = handle_session_error;
        config.sync_config->client_resync_mode = sync_configuration.client_resync_mode;
        config.path = Utf16StringAccessor(configuration.path, configuration.path_len);

        // by definition the key is only allowed to be 64 bytes long, enforced by C# code
        if (encryption_key) {
            auto& key = *reinterpret_cast<std::array<char, 64>*>(encryption_key);

            config.encryption_key = std::vector<char>(key.begin(), key.end());
            config.sync_config->realm_encryption_key = key;
        }

        config.cache = configuration.enable_cache;

        return config;
    }
}

extern "C" {
    REALM_EXPORT void shared_app_initialize(uint16_t* platform, size_t platform_len,
        uint16_t* platform_version, size_t platform_version_len, 
        uint16_t* sdk_version, size_t sdk_version_len,
        decltype(s_open_realm_callback) open_callback)
    {
        s_platform = Utf16StringAccessor(platform, platform_len);
        s_platform_version = Utf16StringAccessor(platform_version, platform_version_len);
        s_sdk_version = Utf16StringAccessor(sdk_version, sdk_version_len);

        s_open_realm_callback = open_callback;
    }

    REALM_EXPORT SharedApp* shared_app_create(AppConfig app_config, SyncClientConfiguration sync_config)
    {
        App::Config config;
        config.app_id = Utf16StringAccessor(app_config.app_id, app_config.app_id_len);
        config.platform = s_platform;
        config.platform_version = s_platform_version;
        config.sdk_version = s_sdk_version;
        config.transport_generator = nullptr; // V10TODO: implement me!

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
        sync_client_config.reset_metadata_on_error = sync_config.reset_metadata_on_error;
        sync_client_config.log_level = sync_config.log_level;
        if (sync_config.metadata_mode) {
            sync_client_config.metadata_mode = *sync_config.metadata_mode;
        } else {
#if REALM_PLATFORM_APPLE && !TARGET_OS_SIMULATOR
            sync_client_config.metadata_mode = SyncManager::MetadataMode::Encryption;
#else
            sync_client_config.metadata_mode = SyncManager::MetadataMode::NoEncryption;
#endif
        }

        if (sync_config.encryption_key_buf) {
            sync_client_config.custom_encryption_key = std::vector<char>(sync_config.encryption_key_buf, sync_config.encryption_key_buf + 64);
        }

        if (sync_config.log_delegate) {
            sync_client_config.logger_factory = new realm::binding::SyncLoggerFactory(sync_config.log_delegate);
        }

        return new SharedApp(App::get_shared_app(std::move(config), std::move(sync_client_config)));
    }

    REALM_EXPORT SharedSyncUser* shared_app_get_current_user(SharedApp& app, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> SharedSyncUser* {
            auto ptr = app->current_user();
            if (ptr == nullptr) {
                return nullptr;
            }

            return new SharedSyncUser(std::move(ptr));
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

    REALM_EXPORT SharedSyncSession* shared_app_sync_get_session_from_path(SharedApp& app, const uint16_t* path_buf, size_t path_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            Utf16StringAccessor path(path_buf, path_len);
            return new SharedSyncSession(app->sync_manager()->get_existing_active_session(path));
        });
    }

    REALM_EXPORT SharedAsyncOpenTask* shared_app_sync_open_realm_async(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, void* task_completion_source, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto config = get_shared_realm_config(configuration, sync_configuration, objects, objects_length, properties, encryption_key);

            auto task = Realm::get_synchronized_realm(config);
            task->start([task_completion_source](ThreadSafeReference ref, std::exception_ptr error) {
                if (error) {
                    try {
                        std::rethrow_exception(error);
                    } catch (const std::system_error& system_error) {
                        const std::error_code& ec = system_error.code();
                        s_open_realm_callback(task_completion_source, nullptr, ec.value(), ec.message().c_str(), ec.message().length());
                    }
                } else {
                    s_open_realm_callback(task_completion_source, new ThreadSafeReference(std::move(ref)), 0, nullptr, 0);
                }
            });

            return new SharedAsyncOpenTask(task);
        });
    }

    REALM_EXPORT SharedRealm* shared_app_sync_open_realm(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            auto config = get_shared_realm_config(configuration, sync_configuration, objects, objects_length, properties, encryption_key);

            auto realm = Realm::get_shared_realm(config);
            if (!configuration.read_only)
                realm->refresh();

            return new SharedRealm(realm);
        });
    }

    REALM_EXPORT size_t shared_app_sync_get_path_for_realm(SharedApp& app, SharedSyncUser& user, uint16_t* url, size_t url_len, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            Utf16StringAccessor realm_url(url, url_len);
            auto path = app->sync_manager()->path_for_realm(*user, realm_url);

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

    REALM_EXPORT SharedSyncSession* shared_app_sync_get_session(SharedApp& app, uint16_t* pathbuffer, size_t pathbuffer_len, SyncConfiguration sync_configuration, uint8_t* encryption_key, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            std::string path(Utf16StringAccessor(pathbuffer, pathbuffer_len));
            std::string url(Utf16StringAccessor(sync_configuration.url, sync_configuration.url_len));

            SyncConfig config(*sync_configuration.user, url);
            config.error_handler = handle_session_error;
            if (encryption_key) {
                config.realm_encryption_key = *reinterpret_cast<std::array<char, 64>*>(encryption_key);
            }

            return new SharedSyncSession(app->sync_manager()->get_session(path, config)->external_reference());
        });
    }
}
