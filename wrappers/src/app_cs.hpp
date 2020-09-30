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

using namespace realm::app;

namespace realm {
namespace binding {
    struct MarshaledAppError
    {
        bool is_null = true;
        const char* message_buf = nullptr;
        size_t message_len = 0;
        const char* error_category_buf = nullptr;
        size_t error_category_len = 0;
        int error_code = 0;

        MarshaledAppError()
        {
        }

        MarshaledAppError(const std::string& message, const std::string& error_category, int err_code)
        {
            is_null = false;

            message_buf = message.c_str();
            message_len = message.size();

            error_category_buf = error_category.c_str();
            error_category_len = error_category.size();

            error_code = err_code;
        }
    };

    extern void (*s_void_callback)(void* tcs_ptr, MarshaledAppError err);

    inline std::function<void(util::Optional<AppError>)> get_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](util::Optional<AppError> err) {
            if (err) {
                std::string message = err->message;
                std::string error_category = err->error_code.message();
                MarshaledAppError app_error(message, error_category, err->error_code.value());
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
