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
#include "sync/sync_manager.hpp"
#include "sync/sync_user.hpp"
#include "sync/sync_session.hpp"
#include "sync/app.hpp"
#include "app_cs.hpp"

using namespace realm;
using namespace realm::binding;
using namespace app;

using SharedSyncUser = std::shared_ptr<SyncUser>;
using SharedSyncSession = std::shared_ptr<SyncSession>;

struct UserApiKey {
    PrimitiveValue id;

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
                std::string message = err->message;
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(message, error_category, err->error_code.value());

                s_api_key_callback(tcs_ptr, nullptr, 0, app_error);
            }
            else {
                std::vector<UserApiKey> marshalled_keys(keys.size());
                for (auto i = 0; i < keys.size(); i++) {
                    auto& api_key = keys[i];
                    UserApiKey marshalled_key;
                    marshalled_key.id.type = realm::PropertyType::ObjectId;
                    marshalled_key.id.has_value = true;
                    auto bytes = api_key.id.to_bytes();
                    for (int i = 0; i < 12; i++)
                    {
                        marshalled_key.id.value.object_id_bytes[i] = bytes[i];
                    }

                    if (api_key.key) {
                        marshalled_key.key = api_key.key->c_str();
                        marshalled_key.key_len = api_key.key->size();
                    }

                    marshalled_key.name = api_key.name.c_str();
                    marshalled_key.name_len = api_key.name.length();
                    marshalled_key.disabled = api_key.disabled;

                    marshalled_keys[i] = marshalled_key;
                }

                s_api_key_callback(tcs_ptr, marshalled_keys.data(), static_cast<int>(marshalled_keys.size()), MarshaledAppError());
            }
        }

        inline void invoke_api_key_callback(void* tcs_ptr, App::UserAPIKey key, util::Optional<AppError> err) {
            std::vector<App::UserAPIKey> api_keys;
            api_keys.push_back(key);

            invoke_api_key_callback(tcs_ptr, api_keys, err);
        }
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
            auto provider = user->provider_type();
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

    REALM_EXPORT SharedApp* realm_syncuser_get_app(SharedSyncUser& user, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            if (auto shared_app = user->sync_manager()->app().lock()) {
                return new SharedApp(shared_app);
            }

            return (SharedApp*)nullptr;
        });
    }

    REALM_EXPORT void realm_syncuser_create_api_key(SharedSyncUser& user, SharedApp& app, uint16_t* name_buf, size_t name_len, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            Utf16StringAccessor name(name_buf, name_len);
            app->provider_client<App::UserAPIKeyProviderClient>().create_api_key(name, user, [tcs_ptr](App::UserAPIKey api_key, util::Optional<AppError> err) {
                invoke_api_key_callback(tcs_ptr, api_key, err);
            });
        });
    }

    REALM_EXPORT void realm_syncuser_fetch_api_key(SharedSyncUser& user, SharedApp& app, uint8_t* id_bytes, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::array<uint8_t, 12> bytes;
            std::copy(id_bytes, id_bytes + 12, bytes.begin());

            app->provider_client<App::UserAPIKeyProviderClient>().fetch_api_key(ObjectId(std::move(bytes)), user, [tcs_ptr](App::UserAPIKey api_key, util::Optional<AppError> err) {
                invoke_api_key_callback(tcs_ptr, api_key, err);
            });
        });
    }

    REALM_EXPORT void realm_syncuser_fetch_api_keys(SharedSyncUser& user, SharedApp& app, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            app->provider_client<App::UserAPIKeyProviderClient>().fetch_api_keys(user, [tcs_ptr](std::vector<App::UserAPIKey> api_keys, util::Optional<AppError> err) {
                invoke_api_key_callback(tcs_ptr, api_keys, err);
            });
        });
    }

    REALM_EXPORT void realm_syncuser_delete_api_key(SharedSyncUser& user, SharedApp& app, uint8_t* id_bytes, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::array<uint8_t, 12> bytes;
            std::copy(id_bytes, id_bytes + 12, bytes.begin());

            app->provider_client<App::UserAPIKeyProviderClient>().delete_api_key(ObjectId(std::move(bytes)), user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_disable_api_key(SharedSyncUser& user, SharedApp& app, uint8_t* id_bytes, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::array<uint8_t, 12> bytes;
            std::copy(id_bytes, id_bytes + 12, bytes.begin());

            app->provider_client<App::UserAPIKeyProviderClient>().disable_api_key(ObjectId(std::move(bytes)), user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_enable_api_key(SharedSyncUser& user, SharedApp& app, uint8_t* id_bytes, void* tcs_ptr, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            std::array<uint8_t, 12> bytes;
            std::copy(id_bytes, id_bytes + 12, bytes.begin());

            app->provider_client<App::UserAPIKeyProviderClient>().enable_api_key(ObjectId(std::move(bytes)), user, get_callback_handler(tcs_ptr));
        });
    }

    REALM_EXPORT void realm_syncuser_destroy(SharedSyncUser* user)
    {
        delete user;
    }
}
