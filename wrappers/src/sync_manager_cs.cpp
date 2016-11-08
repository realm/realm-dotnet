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
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_realm_cs.hpp"
#include "shared_realm.hpp"
#include "sync/sync_manager.hpp"
#include "sync/sync_user.hpp"
#include "sync/sync_config.hpp"
#include "sync/sync_session.hpp"

using namespace realm;
using namespace realm::binding;

struct SyncConfiguration
{
    std::shared_ptr<SyncUser>* user;

    uint16_t* url;
    size_t url_len;
};

extern "C" {
REALM_EXPORT void realm_initialize_sync(const uint16_t* base_path_buf, size_t base_path_len)
{
    Utf16StringAccessor base_path(base_path_buf, base_path_len);
    SyncManager::shared().configure_file_system(base_path);
}
    
    
REALM_EXPORT SharedRealm* shared_realm_open_with_sync(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Realm::Config config;
        config.schema_mode = SchemaMode::Additive;

        // by definition the key is only allowwed to be 64 bytes long, enforced by C# code
        if (encryption_key)
          config.encryption_key = std::vector<char>(encryption_key, encryption_key+64);

        config.schema = create_schema(objects, objects_length, properties);
        config.schema_version = configuration.schema_version;

        Utf16StringAccessor realm_url(sync_configuration.url, sync_configuration.url_len);
        auto handler = [=](const std::string& path, const realm::SyncConfig& config, std::shared_ptr<SyncSession> session) {
            if (config.user->is_admin()) {
                std::async([session, user=config.user]() {
                    session->refresh_access_token(user->refresh_token(), user->server_url());
                });
            }
            //TODO
        };
        config.sync_config = std::make_shared<SyncConfig>(*sync_configuration.user, realm_url.to_string(), SyncSessionStopPolicy::AfterChangesUploaded, handler);
        config.path = SyncManager::shared().path_for_realm((*sync_configuration.user)->identity(), realm_url.to_string());
        return new SharedRealm(Realm::get_shared_realm(config));
    });
}

}

