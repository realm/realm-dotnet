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

#include "sync/generic_network_transport.hpp"
#include "sync/app_credentials.hpp"
#include "sync/app.hpp"
#include "sync/sync_user.hpp"
#include "sync/sync_manager.hpp"

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
        int error_code = 0;

        MarshaledAppError()
        {
        }

        MarshaledAppError(const std::string& message, const std::string& error_category, const std::string& logs_link, int err_code)
        {
            is_null = false;

            message_buf = message.c_str();
            message_len = message.size();

            error_category_buf = error_category.c_str();
            error_category_len = error_category.size();

            logs_link_buf = logs_link.c_str();
            logs_link_len = logs_link.size();

            error_code = err_code;
        }
    };

    struct Credentials
    {
        AuthProvider provider;

        uint16_t* token;
        size_t token_len;

        uint16_t* password;
        size_t password_len;

        AppCredentials to_app_credentials() {
            switch (provider)
            {
            case AuthProvider::ANONYMOUS:
                return AppCredentials::anonymous();

            case AuthProvider::FACEBOOK:
                return AppCredentials::facebook(Utf16StringAccessor(token, token_len));

            case AuthProvider::GOOGLE:
                return AppCredentials::google(Utf16StringAccessor(token, token_len));
            case AuthProvider::APPLE:
                return AppCredentials::apple(Utf16StringAccessor(token, token_len));

            case AuthProvider::CUSTOM:
                return AppCredentials::custom(Utf16StringAccessor(token, token_len));

            case AuthProvider::USERNAME_PASSWORD:
                return AppCredentials::username_password(Utf16StringAccessor(token, token_len), Utf16StringAccessor(password, password_len));

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

    extern void (*s_void_callback)(void* tcs_ptr, MarshaledAppError err);
    extern void (*s_user_callback)(void* tcs_ptr, SharedSyncUser* user, MarshaledAppError err);

    inline std::function<void(std::shared_ptr<SyncUser> user, util::Optional<AppError>)> get_user_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](std::shared_ptr<SyncUser> user, util::Optional<AppError> err) {
            if (err) {
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->error_code.value());

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
                MarshaledAppError app_error(err->message, error_category, err->link_to_server_logs, err->error_code.value());
                s_void_callback(tcs_ptr, app_error);
            }
            else {
                s_void_callback(tcs_ptr, MarshaledAppError());
            }
        };
    }
}
}

#endif /* defined(APP_CS_HPP) */
