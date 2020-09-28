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

extern "C" {
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

    REALM_EXPORT void realm_syncuser_destroy(SharedSyncUser* user)
    {
        delete user;
    }
}
