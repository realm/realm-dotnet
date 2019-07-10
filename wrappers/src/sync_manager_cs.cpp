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

#include <future>
#include <realm.hpp>
#include <realm/util/uri.hpp>
#include "sync_manager_cs.hpp"
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_realm_cs.hpp"
#include "shared_realm.hpp"
#include "sync/sync_manager.hpp"
#include "sync/sync_user.hpp"
#include "sync/sync_config.hpp"
#include "sync/sync_session.hpp"
#include "sync_session_cs.hpp"
#include "sync/impl/sync_metadata.hpp"
#include "sync/partial_sync.hpp"

#if REALM_WINDOWS
#include <VersionHelpers.h>
#endif

using namespace realm;
using namespace realm::binding;

using LogMessageDelegate = void(const char* message, size_t message_len, util::Logger::Level level);

namespace realm {
namespace binding {
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

}


using SharedSyncUser = std::shared_ptr<SyncUser>;

extern "C" {
REALM_EXPORT void realm_syncmanager_configure(const uint16_t* base_path_buf, size_t base_path_len,
                                              const uint16_t* user_agent_buf, size_t user_agent_len,
                                              const SyncManager::MetadataMode* mode, const char* encryption_key_buf, bool reset_on_error,
                                              NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        Utf16StringAccessor base_path(base_path_buf, base_path_len);
        Utf16StringAccessor user_agent(user_agent_buf, user_agent_len);

        auto metadata_mode = SyncManager::MetadataMode::NoEncryption;
        if (mode) {
            metadata_mode = *mode;
#if REALM_PLATFORM_APPLE && !TARGET_OS_SIMULATOR
        } else {
            metadata_mode = SyncManager::MetadataMode::Encryption;
#endif
        }

        util::Optional<std::vector<char>> encryption_key;
        if (encryption_key_buf) {
            encryption_key = std::vector<char>(encryption_key_buf, encryption_key_buf + 64);
        }
        
        SyncManager::shared().configure(base_path, metadata_mode, user_agent, encryption_key, reset_on_error);
    });
}
    
REALM_EXPORT void realm_syncmanager_set_user_agent(const uint16_t* user_agent_buf, size_t user_agent_len, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        Utf16StringAccessor user_agent(user_agent_buf, user_agent_len);
        SyncManager::shared().set_user_agent(user_agent);
    });
}
    
REALM_EXPORT void realm_syncmanager_set_log_level(util::Logger::Level* level, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        SyncManager::shared().set_log_level(*level);
    });
}
    
REALM_EXPORT void realm_syncmanager_set_log_callback(LogMessageDelegate delegate, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        SyncManager::shared().set_logger_factory(*new realm::binding::SyncLoggerFactory(delegate));
    });
}

REALM_EXPORT util::Logger::Level realm_syncmanager_get_log_level()
{
    return SyncManager::shared().log_level();
}

REALM_EXPORT SharedRealm* shared_realm_open_with_sync(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {        
        Realm::Config config;
        config.schema_mode = SchemaMode::Additive;

        if (objects_length > 0) {
            config.schema = create_schema(objects, objects_length, properties);
        }
        
        config.schema_version = configuration.schema_version;

        std::string realm_url(Utf16StringAccessor(sync_configuration.url, sync_configuration.url_len));
        
        config.sync_config = std::make_shared<SyncConfig>(*sync_configuration.user, realm_url);
        config.sync_config->bind_session_handler = bind_session;
        config.sync_config->error_handler = handle_session_error;
        config.path = Utf16StringAccessor(configuration.path, configuration.path_len);
        
        // by definition the key is only allowed to be 64 bytes long, enforced by C# code
        if (encryption_key) {
            auto& key = *reinterpret_cast<std::array<char, 64>*>(encryption_key);
            
            config.encryption_key = std::vector<char>(key.begin(), key.end());
            config.sync_config->realm_encryption_key = key;
        }

#if !REALM_PLATFORM_APPLE
        if (sync_configuration.trusted_ca_path) {
            Utf16StringAccessor trusted_ca_path(sync_configuration.trusted_ca_path, sync_configuration.trusted_ca_path_len);
            config.sync_config->ssl_trust_certificate_path = trusted_ca_path.to_string();
        }
#endif
        
        config.sync_config->client_validate_ssl = sync_configuration.client_validate_ssl;
        config.sync_config->is_partial = sync_configuration.is_partial;
        
        if (sync_configuration.partial_sync_identifier) {
            Utf16StringAccessor partial_sync_identifier(sync_configuration.partial_sync_identifier, sync_configuration.partial_sync_identifier_len);
            config.sync_config->custom_partial_sync_identifier = partial_sync_identifier.to_string();
        }
        
        auto realm = Realm::get_shared_realm(config);
        if (!configuration.read_only)
            realm->refresh();
        
        return new SharedRealm(realm);
    });
}
    
REALM_EXPORT size_t realm_syncmanager_get_path_for_realm(SharedSyncUser& user, uint16_t* url, size_t url_len, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor realm_url(url, url_len);
        auto path = SyncManager::shared().path_for_realm(*user, realm_url);
        
        return stringdata_to_csharpstringbuffer(path, pathbuffer, pathbuffer_len);
    });
}
    
REALM_EXPORT bool realm_syncmanager_immediately_run_file_actions(uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::string path(Utf16StringAccessor(pathbuffer, pathbuffer_len));
        return SyncManager::shared().immediately_run_file_actions(path);
    });
}
    
REALM_EXPORT bool realm_syncmanager_cancel_pending_file_actions(uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::string path(Utf16StringAccessor(pathbuffer, pathbuffer_len));
        bool result;
        SyncManager::shared().perform_metadata_update([&](const auto& manager) {
            result = manager.delete_metadata_action(path);
        });
        return result;
    });
}

REALM_EXPORT void realm_syncmanager_reconnect()
{
    SyncManager::shared().reconnect();
}

REALM_EXPORT std::shared_ptr<SyncSession>* realm_syncmanager_get_session(uint16_t* pathbuffer, size_t pathbuffer_len, SyncConfiguration sync_configuration, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::string path(Utf16StringAccessor(pathbuffer, pathbuffer_len));
        std::string url(Utf16StringAccessor(sync_configuration.url, sync_configuration.url_len));
        
        SyncConfig config(*sync_configuration.user, url);
        config.bind_session_handler = bind_session;
        config.error_handler = handle_session_error;
        if (encryption_key) {
            config.realm_encryption_key = *reinterpret_cast<std::array<char, 64>*>(encryption_key);
        }
        
#if !REALM_PLATFORM_APPLE
        if (sync_configuration.trusted_ca_path) {
            Utf16StringAccessor trusted_ca_path(sync_configuration.trusted_ca_path, sync_configuration.trusted_ca_path_len);
            config.ssl_trust_certificate_path = trusted_ca_path.to_string();
        }
#endif
        
        config.client_validate_ssl = sync_configuration.client_validate_ssl;
        config.is_partial = sync_configuration.is_partial;
        
        if (sync_configuration.partial_sync_identifier) {
            Utf16StringAccessor partial_sync_identifier(sync_configuration.partial_sync_identifier, sync_configuration.partial_sync_identifier_len);
            config.custom_partial_sync_identifier = partial_sync_identifier.to_string();
        }

        return new std::shared_ptr<SyncSession>(SyncManager::shared().get_session(path, config)->external_reference());
    });
}
    
REALM_EXPORT uint8_t realm_syncmanager_get_realm_privileges(SharedRealm& sharedRealm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return static_cast<uint8_t>(sharedRealm->get_privileges());
    });
}

REALM_EXPORT uint8_t realm_syncmanager_get_class_privileges(SharedRealm& sharedRealm, uint16_t* class_buf, size_t class_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor class_name(class_buf, class_len);
        return static_cast<uint8_t>(sharedRealm->get_privileges(class_name));
    });
}

REALM_EXPORT uint8_t realm_syncmanager_get_object_privileges(SharedRealm& sharedRealm, Object& object, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return static_cast<uint8_t>(sharedRealm->get_privileges(object.row()));
    });
}

}

