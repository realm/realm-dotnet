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
        realm_value_t message = realm_value_t{};
        realm_value_t error_category = realm_value_t{};
        realm_value_t logs_link = realm_value_t{};
        int http_status_code = 0;

        MarshaledAppError()
        {
        }

        MarshaledAppError(const std::string& message_str, const std::string& error_category_str, const std::string& logs_link_str, util::Optional<int> http_code)
        {
            is_null = false;

            message = to_capi_value(message_str);
            error_category = to_capi_value(error_category_str);
            logs_link = to_capi_value(logs_link_str);

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
            case AuthProvider::ANONYMOUS: {
                Utf16StringAccessor reuse_existing(additional_info, additional_info_len);
                return AppCredentials::anonymous(reuse_existing == "true");
            }

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
        realm_value_t id = realm_value_t{};

        realm_value_t key = realm_value_t{};

        realm_value_t name = realm_value_t{};

        bool disabled;
    };

    using UserCallbackT = void(void* tcs_ptr, SharedSyncUser* user, MarshaledAppError err);
    using VoidCallbackT = void(void* tcs_ptr, MarshaledAppError err);
    using BsonCallbackT = void(void* tcs_ptr, realm_value_t response, MarshaledAppError err);
    using ApiKeysCallbackT = void(void* tcs_ptr, UserApiKey* api_keys, size_t api_keys_len, MarshaledAppError err);

    extern std::function<VoidCallbackT> s_void_callback;
    extern std::function<UserCallbackT> s_user_callback;
    extern std::function<BsonCallbackT> s_bson_callback;
    extern std::function<ApiKeysCallbackT> s_api_keys_callback;

    inline auto get_string_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](const std::string* response, util::Optional<AppError> err) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);

                s_bson_callback(tcs_ptr, realm_value_t{}, app_error);
            } else if (response) {
                s_bson_callback(tcs_ptr, to_capi_value(*response), MarshaledAppError());
            }
            else {
                s_bson_callback(tcs_ptr, realm_value_t{}, MarshaledAppError());
            }
        };
    }

    inline auto get_user_callback_handler(void* tcs_ptr) {
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

    inline auto get_callback_handler(void* tcs_ptr) {
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

    inline auto get_bson_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](util::Optional<bson::Bson> response, util::Optional<AppError> err) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);

                s_bson_callback(tcs_ptr, realm_value_t{}, app_error);
            }
            else if (response) {
                std::string serialized = response->to_string();
                s_bson_callback(tcs_ptr, to_capi_value(serialized), MarshaledAppError());
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

            for (size_t i = 0; i < keys.size(); i++) {
                auto& api_key = keys[i];
                UserApiKey marshaled_key{};

                id_storage[i] = api_key.id.to_string();
                marshaled_key.id = to_capi_value(id_storage[i]);

                if (api_key.key) {
                    marshaled_key.key = to_capi_value(*api_key.key);
                }
 
                marshaled_key.name = to_capi_value(api_key.name);
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
