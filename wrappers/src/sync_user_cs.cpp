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

#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_user.hpp>
#include <realm/object-store/sync/sync_session.hpp>
#include <realm/object-store/sync/app.hpp>
#include "app_cs.hpp"

using namespace realm;
using namespace realm::binding;
using namespace app;

using SharedSyncUser = std::shared_ptr<SyncUser>;
using SharedSyncSession = std::shared_ptr<SyncSession>;

struct UserApiKey {
    const char* id;
    size_t id_len;

    const char* key;
    size_t key_len;

    const char* name;
    size_t name_len;

    bool disabled;
};

namespace realm {
    namespace binding {
        void (*s_api_key_callback)(void* tcs_ptr, UserApiKey* api_keys, int api_keys_len, MarshaledAppError err);

        inline void invoke_api_key_callback(void* tcs_ptr, std::vector<App::UserAPIKey> keys, util::Optional<AppError> err) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->http_status_code);

                s_api_key_callback(tcs_ptr, nullptr, 0, app_error);
            }
            else {
                std::vector<UserApiKey> marshalled_keys(keys.size());
                std::vector<std::string> id_storage(keys.size());

                for (auto i = 0; i < keys.size(); i++) {
                    auto& api_key = keys[i];
                    UserApiKey marshaled_key;

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

                s_api_key_callback(tcs_ptr, marshalled_keys.data(), static_cast<int>(marshalled_keys.size()), MarshaledAppError());
            }
        }

        inline void invoke_api_key_callback(void* tcs_ptr, App::UserAPIKey key, util::Optional<AppError> err) {
            std::vector<App::UserAPIKey> api_keys;
            api_keys.push_back(key);

            invoke_api_key_callback(tcs_ptr, api_keys, err);
        }

        inline AuthProvider to_auth_provider(const std::string& provider) {
            if (provider == IdentityProviderAnonymous) {
                return AuthProvider::ANONYMOUS;
            }

            if (provider == IdentityProviderFacebook) {
                return AuthProvider::FACEBOOK;
            }

            if (provider == IdentityProviderGoogle) {
                return AuthProvider::GOOGLE;
            }

            if (provider == IdentityProviderApple) {
                return AuthProvider::APPLE;
            }

            if (provider == IdentityProviderCustom) {
                return AuthProvider::CUSTOM;
            }

            if (provider == IdentityProviderUsernamePassword) {
                return AuthProvider::USERNAME_PASSWORD;
            }

            if (provider == IdentityProviderFunction) {
                return AuthProvider::FUNCTION;
            }

            if (provider == IdentityProviderUserAPIKey) {
                return AuthProvider::USER_API_KEY;
            }

            if (provider == IdentityProviderServerAPIKey) {
                return AuthProvider::SERVER_API_KEY;
            }

            return (AuthProvider)999;
        }
    }

    void to_json(nlohmann::json& j, const SyncUserIdentity& i)
    {
        j = nlohmann::json{
            { "Id", i.id },
            { "Provider", to_auth_provider(i.provider_type)}
        };
    }
}

extern "C" {
    REALM_EXPORT void realm_sync_user_initialize(decltype(s_api_key_callback) api_key_callback)
    {
        s_api_key_callback = api_key_callback;
    }

    REALM_EXPORT void realm_syncuser_log_out(SharedSyncUser& user, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            user->log_out();
        });
    }

    REALM_EXPORT size_t realm_syncuser_get_id(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::string identity(user->identity());
            return stringdata_to_csharpstringbuffer(identity, buffer, buffer_length);
        });
    }

    REALM_EXPORT size_t realm_syncuser_get_refresh_token(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::string refresh_token(user->refresh_token());
            return stringdata_to_csharpstringbuffer(refresh_token, buffer, buffer_length);
        });
    }

    REALM_EXPORT size_t realm_syncuser_get_access_token(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::string access_token(user->access_token());
            return stringdata_to_csharpstringbuffer(access_token, buffer, buffer_length);
        });
    }

    REALM_EXPORT size_t realm_syncuser_get_device_id(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::string device_id(user->device_id());
            return stringdata_to_csharpstringbuffer(device_id, buffer, buffer_length);
        });
    }

    REALM_EXPORT SyncUser::State realm_syncuser_get_state(SharedSyncUser& user, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            return user->state();
        });
    }

    REALM_EXPORT AuthProvider realm_syncuser_get_auth_provider(SharedSyncUser& user, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            return to_auth_provider(user->provider_type());
        });
    }

    REALM_EXPORT size_t realm_syncuser_get_custom_data(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, bool& is_null, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            if (user->custom_data()) {
                is_null = false;
                std::string serialized_data = bson::Bson(user->custom_data().value()).to_string();
                return stringdata_to_csharpstringbuffer(serialized_data, buffer, buffer_length);
            }

            is_null = true;
            return (size_t)0;
        });
    }

    REALM_EXPORT void realm_syncuser_refresh_custom_data(SharedSyncUser& user, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            user->refresh_custom_data(get_callback_handler(tcs_ptr));
        });
    }

    enum class UserProfileField : uint8_t {
        name,
        email,
        picture_url,
        first_name,
        last_name,
        gender,
        birthday,
        min_age,
        max_age,
    };

    REALM_EXPORT size_t realm_syncuser_get_profile_data(SharedSyncUser& user, UserProfileField profile_field, uint16_t* string_buffer, size_t buffer_size, bool& is_null, NativeException::Marshallable& ex) {
        return handle_errors(ex, [&]() {
            util::Optional<std::string> field;

            switch (profile_field)
            {
            case UserProfileField::name:
                field = user->user_profile().name;
                break;
            case UserProfileField::email:
                field = user->user_profile().email;
                break;
            case UserProfileField::picture_url:
                field = user->user_profile().picture_url;
                break;
            case UserProfileField::first_name:
                field = user->user_profile().first_name;
                break;
            case UserProfileField::last_name:
                field = user->user_profile().last_name;
                break;
            case UserProfileField::gender:
                field = user->user_profile().gender;
                break;
            case UserProfileField::birthday:
                field = user->user_profile().birthday;
                break;
            case UserProfileField::min_age:
                field = user->user_profile().min_age;
                break;
            case UserProfileField::max_age:
                field = user->user_profile().max_age;
                break;
            default:
                REALM_UNREACHABLE();
            }

            if ((is_null = !field)) {
                return (size_t)0;
            }

            return stringdata_to_csharpstringbuffer(field.value(), string_buffer, buffer_size);
        });
    }

    REALM_EXPORT size_t realm_syncuser_get_serialized_identities(SharedSyncUser& user, uint16_t* string_buffer, size_t buffer_size, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() -> size_t {
            nlohmann::json j = user->identities();
            return stringdata_to_csharpstringbuffer(j.dump(), string_buffer, buffer_size);
        });
    }

    REALM_EXPORT SharedApp* realm_syncuser_get_app(SharedSyncUser& user, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            if (auto shared_app = user->sync_manager()->app().lock()) {
                return new SharedApp(shared_app);
            }

            return (SharedApp*)nullptr;
        });
    }

    REALM_EXPORT void realm_syncuser_call_function(SharedSyncUser& user, SharedApp& app, uint16_t* function_name_buf, size_t function_name_len, uint16_t* args_buf, size_t args_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            Utf16StringAccessor function_name(function_name_buf, function_name_len);

            auto args = to_array(args_buf, args_len);
            app->call_function(user, function_name, args, get_bson_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_link_credentials(SharedSyncUser& user, SharedApp& app, Credentials credentials, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto app_credentials = credentials.to_app_credentials();
            app->link_user(user, app_credentials, get_user_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_push_register(SharedSyncUser& user, SharedApp& app, uint16_t* service_buf, size_t service_len, uint16_t* token_buf, size_t token_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            Utf16StringAccessor service(service_buf, service_len);
            Utf16StringAccessor token(token_buf, token_len);
            app->push_notification_client(service).register_device(token, user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_push_deregister(SharedSyncUser& user, SharedApp& app, uint16_t* service_buf, size_t service_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            Utf16StringAccessor service(service_buf, service_len);
            app->push_notification_client(service).deregister_device(user, get_callback_handler(tcs_ptr));
        });
    }

#pragma region ApiKeys

    REALM_EXPORT void realm_syncuser_api_key_create(SharedSyncUser& user, SharedApp& app, uint16_t* name_buf, size_t name_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            Utf16StringAccessor name(name_buf, name_len);
            app->provider_client<App::UserAPIKeyProviderClient>().create_api_key(name, user, [tcs_ptr](App::UserAPIKey api_key, util::Optional<AppError> err) {
                invoke_api_key_callback(tcs_ptr, api_key, err);
            });
        });
    }

    REALM_EXPORT void realm_syncuser_api_key_fetch(SharedSyncUser& user, SharedApp& app, realm_value_t id, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            app->provider_client<App::UserAPIKeyProviderClient>().fetch_api_key(from_capi(id.object_id), user, [tcs_ptr](App::UserAPIKey api_key, util::Optional<AppError> err) {
                invoke_api_key_callback(tcs_ptr, api_key, err);
            });
        });
    }

    REALM_EXPORT void realm_syncuser_api_key_fetch_all(SharedSyncUser& user, SharedApp& app, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            app->provider_client<App::UserAPIKeyProviderClient>().fetch_api_keys(user, [tcs_ptr](std::vector<App::UserAPIKey> api_keys, util::Optional<AppError> err) {
                invoke_api_key_callback(tcs_ptr, api_keys, err);
            });
        });
    }

    REALM_EXPORT void realm_syncuser_api_key_delete(SharedSyncUser& user, SharedApp& app, realm_value_t id, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            app->provider_client<App::UserAPIKeyProviderClient>().delete_api_key(from_capi(id.object_id), user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_api_key_disable(SharedSyncUser& user, SharedApp& app, realm_value_t id, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            app->provider_client<App::UserAPIKeyProviderClient>().disable_api_key(from_capi(id.object_id), user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_api_key_enable(SharedSyncUser& user, SharedApp& app, realm_value_t id, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            app->provider_client<App::UserAPIKeyProviderClient>().enable_api_key(from_capi(id.object_id), user, get_callback_handler(tcs_ptr));
        });
    }

#pragma endregion

    REALM_EXPORT void realm_syncuser_destroy(SharedSyncUser* user)
    {
        delete user;
    }
}
