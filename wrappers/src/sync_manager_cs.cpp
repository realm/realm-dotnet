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

using namespace realm;
using namespace realm::binding;

using SharedSyncUser = std::shared_ptr<SyncUser>;

struct SyncConfiguration
{
    std::shared_ptr<SyncUser>* user;

    uint16_t* url;
    size_t url_len;
};

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
        Realm::Config config;
        config.schema_mode = SchemaMode::Additive;

        // by definition the key is only allowed to be 64 bytes long, enforced by C# code
        if (encryption_key) {
            config.encryption_key = std::vector<char>(encryption_key, encryption_key+64);
        }

        config.schema = create_schema(objects, objects_length, properties);
        config.schema_version = configuration.schema_version;

        std::string realm_url(Utf16StringAccessor(sync_configuration.url, sync_configuration.url_len));
        
        config.sync_config = std::make_shared<SyncConfig>(SyncConfig{*sync_configuration.user, realm_url, SyncSessionStopPolicy::AfterChangesUploaded, bind_session, handle_session_error});
        config.path = Utf16StringAccessor(configuration.path, configuration.path_len);
        return new SharedRealm(Realm::get_shared_realm(config));
    });
}
    
REALM_EXPORT size_t realm_syncmanager_get_path_for_realm(SharedSyncUser& user, uint16_t* url, size_t url_len, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::string realm_url(Utf16StringAccessor(url, url_len));
        auto path = SyncManager::shared().path_for_realm(user->identity(), realm_url);
        
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
    
REALM_EXPORT void realm_syncmanager_reconnect()
{
    SyncManager::shared().reconnect();
}

}

