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

using namespace realm;
using namespace realm::binding;

struct SyncConfiguration
{
    std::shared_ptr<SyncUser>* user;

    uint16_t* url;
    size_t url_len;
};

void (*s_refresh_access_token_callback)(std::shared_ptr<SyncUser>*, std::shared_ptr<SyncSession>*, const char* path, size_t path_len);
void (*s_session_error_callback)(std::shared_ptr<SyncSession>*, int32_t error_code, const char* message, size_t message_len, SyncSessionError);

extern "C" {
REALM_EXPORT void realm_initialize_sync(const uint16_t* base_path_buf, size_t base_path_len, decltype(s_refresh_access_token_callback) refresh_callback, decltype(s_session_error_callback) session_error_callback)
{
    Utf16StringAccessor base_path(base_path_buf, base_path_len);
    SyncManager::shared().configure_file_system(base_path, SyncManager::MetadataMode::NoEncryption);

    s_refresh_access_token_callback = refresh_callback;
    s_session_error_callback = session_error_callback;
}

static void bind_session(const std::string&, const realm::SyncConfig& config, std::shared_ptr<SyncSession> session)
{
    if (config.user->is_admin()) {
        std::async([session, user=config.user]() {
            session->bind_with_admin_token(user->refresh_token(), user->server_url());
        });
    }
    else {
        s_refresh_access_token_callback(new std::shared_ptr<SyncUser>(config.user), new std::shared_ptr<SyncSession>(session), config.realm_url.c_str(), config.realm_url.size());
    }
}

static void handle_session_error(std::shared_ptr<SyncSession> session, int error_code, std::string message, SyncSessionError error)
{
    s_session_error_callback(new std::shared_ptr<SyncSession>(session), error_code, message.c_str(), message.length(), error);
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
    
REALM_EXPORT size_t shared_realm_get_path(uint16_t* identity, size_t identity_len, uint16_t* url, size_t url_len, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::string realm_url(Utf16StringAccessor(url, url_len));
        std::string user_identity(Utf16StringAccessor(identity, identity_len));
        auto path = SyncManager::shared().path_for_realm(user_identity, realm_url);
        
        return stringdata_to_csharpstringbuffer(path, pathbuffer, pathbuffer_len);
    });
}

}

