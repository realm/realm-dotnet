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
#include <realm/sync/feature_token.hpp>
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

using SharedSyncUser = std::shared_ptr<SyncUser>;

#if REALM_HAVE_FEATURE_TOKENS
static std::unique_ptr<sync::FeatureGate> _features;

inline bool should_gate_sync()
{
#if defined(__linux__)
	return true;
#elif REALM_WINDOWS
	return IsWindowsServer();
#else
	return false;
#endif
}

bool realm::binding::has_feature(StringData feature) {
    return _features && _features->has_feature(feature);
}

#endif
namespace realm {
namespace binding {

void (*s_subscribe_for_objects_callback)(Results* results, void* task_completion_source, NativeException::Marshallable nativeException);
    
}
}

extern "C" {
REALM_EXPORT void realm_syncmanager_configure_file_system(const uint16_t* base_path_buf, size_t base_path_len,
                                                          const SyncManager::MetadataMode* mode, const char* encryption_key_buf, bool reset_on_error,
                                                          NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        Utf16StringAccessor base_path(base_path_buf, base_path_len);

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

        SyncManager::shared().configure_file_system(base_path, metadata_mode, encryption_key, reset_on_error);
    });
}
    
REALM_EXPORT SharedRealm* shared_realm_open_with_sync(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
#if REALM_HAVE_FEATURE_TOKENS
        if (should_gate_sync() && !realm::binding::has_feature("Sync")) {
            throw RealmFeatureUnavailableException("The Sync feature is not available on Linux or Windows Server. If you are using the Professional or Enterprise editions, make sure to call Realms.Sync.SyncConfiguration.SetFeatureToken before opening any synced Realms. Otherwise, contact sales@realm.io for more information.");
        }
#endif
        
        Realm::Config config;
        config.schema_mode = SchemaMode::Additive;

        config.schema = create_schema(objects, objects_length, properties);
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
        
        return new std::shared_ptr<SyncSession>(SyncManager::shared().get_session(path, config)->external_reference());
    });
}

REALM_EXPORT void realm_syncmanager_set_feature_token(const uint16_t* token_buf, size_t token_len, NativeException::Marshallable& ex)
{
#if REALM_HAVE_FEATURE_TOKENS
    handle_errors(ex, [&]() {
        Utf16StringAccessor token(token_buf, token_len);
        _features.reset(new sync::FeatureGate(token));
    });
#endif
}
    
REALM_EXPORT void realm_syncmanager_subscribe_for_objects(SharedRealm& sharedRealm, uint16_t* class_buf, size_t class_len, uint16_t* query_buf, size_t query_len, void* task_completion_source, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor class_name(class_buf, class_len);
        Utf16StringAccessor query(query_buf, query_len);

        partial_sync::register_query(sharedRealm, class_name, query, [=](Results results, std::exception_ptr err) {
            if (err) {
                try {
                    std::rethrow_exception(err);
                }
                catch (...) {
                    NativeException::Marshallable nex = convert_exception().for_marshalling();
                    s_subscribe_for_objects_callback(nullptr, task_completion_source, nex);
                }
            } else {
                s_subscribe_for_objects_callback(new Results(results), task_completion_source, NativeException::Marshallable{RealmErrorType::NoError});
            }
        });
    });
}
    
REALM_EXPORT void realm_syncmanager_install_callbacks(decltype(s_subscribe_for_objects_callback) subscribe_callback)
{
    s_subscribe_for_objects_callback = subscribe_callback;
}

}

