////////////////////////////////////////////////////////////////////////////
//
// Copyright 2015 Realm Inc.
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

#ifndef ERROR_HANDLING_HPP
#define ERROR_HANDLING_HPP

#include <string>
#include <realm.hpp>
#include "object-store/realm_binding_context.hpp"
#include "object-store/realm_error_type.hpp"

namespace realm {

void throw_exception(RealmErrorType error_type, const std::string message, RealmBindingContext* binding_context);
void convert_exception();

template <class T>
struct Default {
    static T default_value() {
        return T{};
    }
};
template <>
struct Default<void> {
    static void default_value() {}
};

template <class F>
auto handle_errors(F&& func) -> decltype(func())
{
    using RetVal = decltype(func());
    try {
        return func();
    }
    catch (...) {
        convert_exception();
        return Default<RetVal>::default_value();
    }
}

#define HANDLE_ERRORS_OPEN handle_errors([&]() {
#define HANDLE_ERRORS_CLOSE });

} // namespace realm

#endif // ERROR_HANDLING_HPP