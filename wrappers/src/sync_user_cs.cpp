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


// #include json.hpp needs to be before #include realm.hpp due to https://github.com/nlohmann/json/issues/2129
#include <external/json/json.hpp>

#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_user.hpp>
#include <realm/object-store/sync/sync_session.hpp>
#include <realm/object-store/sync/app.hpp>
#include "app_cs.hpp"
#include "marshalling.hpp"
#include <realm/object-store/sync/app_user.hpp>

using namespace realm;
using namespace realm::binding;

using SharedSyncUser = std::shared_ptr<User>;
using SharedSyncSession = std::shared_ptr<SyncSession>;
using UserChangedCallbackT = void(void* managed_user_handle);

std::function<UserChangedCallbackT> s_user_changed_callback;

namespace realm {
namespace binding {
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

    if (provider == IdentityProviderAPIKey) {
        return AuthProvider::API_KEY;
    }

    return (AuthProvider)999;
}
}
}

namespace realm::app {
void to_json(nlohmann::json& j, const UserIdentity& i)
{
    j = nlohmann::json{
        { "Id", i.id },
        { "Provider", to_auth_provider(i.provider_type)}
    };
}
}

extern "C" {
    REALM_EXPORT void realm_syncuser_install_callbacks(UserChangedCallbackT* user_changed_callback)
    {
        s_user_changed_callback = wrap_managed_callback(user_changed_callback);

        realm::binding::s_can_call_managed = true;
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
            std::string user_id(user->user_id());
            return stringdata_to_csharpstringbuffer(user_id, buffer, buffer_length);
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

    REALM_EXPORT size_t realm_syncuser_get_custom_data(SharedSyncUser& user, uint16_t* buffer, size_t buffer_length, bool& is_null, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            if (user->custom_data()) {
                is_null = false;
                std::string serialized_data = bson::Bson(*user->custom_data()).to_string();
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

    REALM_EXPORT Subscribable<User>::Token* realm_syncuser_register_changed_callback(SharedSyncUser& user, void* managed_user_handle, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&] {
            auto token = user->subscribe([managed_user_handle](const SyncUser&) {
                s_user_changed_callback(managed_user_handle);
            });
            return new Subscribable<User>::Token(std::move(token));
        });
    }

    REALM_EXPORT void realm_syncuser_unregister_property_changed_callback(SharedSyncUser& user, Subscribable<User>::Token& token, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            user->unsubscribe(token);
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
                field = user->user_profile().name();
                break;
            case UserProfileField::email:
                field = user->user_profile().email();
                break;
            case UserProfileField::picture_url:
                field = user->user_profile().picture_url();
                break;
            case UserProfileField::first_name:
                field = user->user_profile().first_name();
                break;
            case UserProfileField::last_name:
                field = user->user_profile().last_name();
                break;
            case UserProfileField::gender:
                field = user->user_profile().gender();
                break;
            case UserProfileField::birthday:
                field = user->user_profile().birthday();
                break;
            case UserProfileField::min_age:
                field = user->user_profile().min_age();
                break;
            case UserProfileField::max_age:
                field = user->user_profile().max_age();
                break;
            default:
                REALM_UNREACHABLE();
            }

            if ((is_null = !field)) {
                return (size_t)0;
            }

            return stringdata_to_csharpstringbuffer(*field, string_buffer, buffer_size);
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
            // If the user is detached from the sync manager, we'll hit an assert, so this early check avoids that.
            if (user->state() != SyncUser::State::Removed)
            {
                if (auto shared_app = user->app()) {
                    return new SharedApp(shared_app);
                }
            }

            return (SharedApp*)nullptr;
        });
    }

    REALM_EXPORT void realm_syncuser_call_function(SharedSyncUser& user,
        SharedApp& app,
        uint16_t* function_name_buf, size_t function_name_len,
        uint16_t* args_buf, size_t args_len,
        uint16_t* service_buf, size_t service_len,
        void* tcs_ptr, NativeException::Marshallable& ex)
    {
        handle_errors(ex, [&] {
            Utf16StringAccessor function_name(function_name_buf, function_name_len);
            Utf16StringAccessor args(args_buf, args_len);
            if (service_buf) {
                Utf16StringAccessor service(service_buf, service_len);
                app->call_function(user, function_name, args, service, get_string_callback_handler(tcs_ptr));
            }
            else {
                app->call_function(user, function_name, args, std::nullopt, get_string_callback_handler(tcs_ptr));
            }
        });
    }

    REALM_EXPORT void realm_syncuser_link_credentials(SharedSyncUser& user, SharedApp& app, Credentials credentials, void* tcs_ptr, NativeException::Marshallable& ex) {
        handle_errors(ex, [&]() {
            auto app_credentials = credentials.to_app_credentials();
            app->link_user(user, app_credentials, get_user_callback_handler(tcs_ptr));
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

    REALM_EXPORT size_t realm_syncuser_get_path_for_realm(SharedSyncUser& user, uint16_t* partition_buf, size_t partition_len, uint16_t* pathbuffer, size_t pathbuffer_len, NativeException::Marshallable& ex)
    {
        return handle_errors(ex, [&]() {
            std::string path;
            if (partition_buf) {
                Utf16StringAccessor partition(partition_buf, partition_len);
                auto sync_config = SyncConfig(user, partition);
                path = user->path_for_realm(std::move(sync_config));
            }
            else {
                auto sync_config = SyncConfig(user, realm::SyncConfig::FLXSyncEnabled{});
                path = user->path_for_realm(std::move(sync_config), "default");
            }

            return stringdata_to_csharpstringbuffer(path, pathbuffer, pathbuffer_len);
        });
    }
}
