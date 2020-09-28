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
    extern void (*s_void_callback)(void* tcs_ptr, const char* message_buf, size_t message_len, const char* error_category_buf, size_t error_category_len, int error_code);

    inline std::function<void(util::Optional<AppError>)> get_callback_handler(void* tcs_ptr) {
        return [tcs_ptr](util::Optional<AppError> err) {
            if (err) {
                s_void_callback(tcs_ptr, err->message.c_str(), err->message.length(),
                    err->error_code.message().c_str(), err->error_code.message().length(),
                    err->error_code.value());
            }
            else {
                s_void_callback(tcs_ptr, nullptr, 0, nullptr, 0, 0);
            }
        };
    }
}
}

#endif /* defined(APP_CS_HPP) */
