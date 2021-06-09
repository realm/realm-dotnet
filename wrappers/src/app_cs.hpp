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

#ifndef APP_CS_HPP
#define APP_CS_HPP

#include <realm/object-store/sync/generic_network_transport.hpp>
#include <realm/object-store/sync/app_credentials.hpp>
#include <realm/object-store/sync/app.hpp>
#include <realm/object-store/sync/sync_user.hpp>
#include <realm/object-store/sync/sync_manager.hpp>

using namespace realm;
using namespace realm::app;

using SharedSyncUser = std::shared_ptr<SyncUser>;

namespace realm {
namespace binding {
    struct MarshaledAppError
    {
        bool is_null = true;
        const char* message_buf = nullptr;
        size_t message_len = 0;
        const char* error_category_buf = nullptr;
        size_t error_category_len = 0;
        const char* logs_link_buf = nullptr;
        size_t logs_link_len = 0;
        int http_status_code = 0;

        MarshaledAppError()
        {
        }

        MarshaledAppError(const std::string& message, const std::string& error_category, const std::string& logs_link, util::Optional<int> http_code)
        {
            is_null = false;

            message_buf = message.c_str();
            message_len = message.size();

            error_category_buf = error_category.c_str();
            error_category_len = error_category.size();

            logs_link_buf = logs_link.c_str();
            logs_link_len = logs_link.size();

            http_status_code = http_code.value_or(0);
        }
    };

    struct Credentials
    {
        AuthProvider provider;

        uint16_t* token;
        size_t token_len;

        uint16_t* additional_info;
        size_t additional_info_len;

        AppCredentials to_app_credentials() {
            switch (provider)
            {
            case AuthProvider::ANONYMOUS:
                return AppCredentials::anonymous();

            case AuthProvider::FACEBOOK:
                return AppCredentials::facebook(Utf16StringAccessor(token, token_len));

            case AuthProvider::GOOGLE: {
                Utf16StringAccessor google_credential_type(additional_info, additional_info_len);
                if (google_credential_type == "AuthCode") {
                    return AppCredentials::google(AuthCode{ Utf16StringAccessor(token, token_len).to_string() });
                }

                if (google_credential_type == "IdToken") {
                    return AppCredentials::google(IdToken{ Utf16StringAccessor(token, token_len).to_string() });
                }

                realm::util::terminate("Invalid google credential type", __FILE__, __LINE__);
            }
            case AuthProvider::APPLE:
                return AppCredentials::apple(Utf16StringAccessor(token, token_len));

            case AuthProvider::CUSTOM:
                return AppCredentials::custom(Utf16StringAccessor(token, token_len));

            case AuthProvider::USERNAME_PASSWORD:
                return AppCredentials::username_password(Utf16StringAccessor(token, token_len), Utf16StringAccessor(additional_info, additional_info_len));

            case AuthProvider::FUNCTION:
                return AppCredentials::function(Utf16StringAccessor(token, token_len));

            case AuthProvider::USER_API_KEY:
                return AppCredentials::user_api_key(Utf16StringAccessor(token, token_len));

            case AuthProvider::SERVER_API_KEY:
                return AppCredentials::server_api_key(Utf16StringAccessor(token, token_len));

            default:
                REALM_UNREACHABLE();
            }
        }
    };

    struct UserApiKey {
        const char* id;
        size_t id_len;

        const char* key;
        size_t key_len;

        const char* name;
        size_t name_len;

        bool disabled;
    };

    using UserCallbackT = void(void* tcs_ptr, SharedSyncUser* user, MarshaledAppError err);
    using VoidCallbackT = void(void* tcs_ptr, MarshaledAppError err);
    using BsonCallbackT = void(void* tcs_ptr, realm_value_t response, MarshaledAppError err);
    using ApiKeysCallbackT = void(void* tcs_ptr, UserApiKey* api_keys, int api_keys_len, MarshaledAppError err);

    extern std::function<VoidCallbackT> s_void_callback;
    extern std::function<UserCallbackT> s_user_callback;
    extern std::function<BsonCallbackT> s_bson_callback;
    extern std::function<ApiKeysCallbackT> s_api_keys_callback;

    inline std::function<void(std::shared_ptr<SyncUser> user, util::Optional<AppError>)> get_user_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](std::shared_ptr<SyncUser> user, util::Optional<AppError> err) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);

                s_user_callback(tcs_ptr, nullptr, app_error);
            }
            else {
                s_user_callback(tcs_ptr, new SharedSyncUser(user), MarshaledAppError());
            }
        };
    }

    inline std::function<void(util::Optional<AppError>)> get_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](util::Optional<AppError> err) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);
                s_void_callback(tcs_ptr, app_error);
            }
            else {
                s_void_callback(tcs_ptr, MarshaledAppError());
            }
        };
    }

    inline std::function<void(util::Optional<AppError>, util::Optional<bson::Bson>)> get_bson_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](util::Optional<AppError> err, util::Optional<bson::Bson> response) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);

                s_bson_callback(tcs_ptr, realm_value_t{}, app_error);
            }
            else if (response) {
                std::string serialized = response->to_string();
                realm_value_t payload{};
                payload.string = to_capi(serialized);
                payload.type = realm_value_type::RLM_TYPE_STRING;
                s_bson_callback(tcs_ptr, payload, MarshaledAppError());
            }
            else {
                s_bson_callback(tcs_ptr, realm_value_t{}, MarshaledAppError());
            }
        };
    }

    inline void invoke_api_key_callback(void* tcs_ptr, std::vector<App::UserAPIKey> keys, util::Optional<AppError> err) {
        if (err) {
            std::string error_category = err->error_code.message();
            MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);

            s_api_keys_callback(tcs_ptr, nullptr, 0, app_error);
        }
        else {
            std::vector<UserApiKey> marshalled_keys(keys.size());
            std::vector<std::string> id_storage(keys.size());

            for (auto i = 0; i < keys.size(); i++) {
                auto& api_key = keys[i];
                UserApiKey marshaled_key{};

                id_storage[i] = api_key.id.to_string();
                marshaled_key.id = id_storage[i].c_str();
                marshaled_key.id_len = id_storage[i].size();

                if (api_key.key) {
                    marshaled_key.key = api_key.key->c_str();
                    marshaled_key.key_len = api_key.key->size();
                }
                else {
                    marshaled_key.key = nullptr;
                    marshaled_key.key_len = 0;
                }

                marshaled_key.name = api_key.name.c_str();
                marshaled_key.name_len = api_key.name.size();
                marshaled_key.disabled = api_key.disabled;

                marshalled_keys[i] = marshaled_key;
            }

            s_api_keys_callback(tcs_ptr, marshalled_keys.data(), marshalled_keys.size(), MarshaledAppError());
        }
    }

    inline void invoke_api_key_callback(void* tcs_ptr, App::UserAPIKey key, util::Optional<AppError> err) {
        std::vector<App::UserAPIKey> api_keys;
        api_keys.push_back(key);

        invoke_api_key_callback(tcs_ptr, api_keys, err);
    }


    inline bson::BsonDocument to_document(uint16_t* buf, size_t len) {
        if (buf == nullptr) {
            return bson::BsonDocument();
        }

        Utf16StringAccessor json(buf, len);
        return static_cast<bson::BsonDocument>(bson::parse(json.to_string()));
    }

    inline bson::BsonArray to_array(uint16_t* buf, size_t len) {
        Utf16StringAccessor json(buf, len);
        return static_cast<bson::BsonArray>(bson::parse(json.to_string()));
    }
}
}

#endif /* defined(APP_CS_HPP) */
