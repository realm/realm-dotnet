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
 
#ifndef ERROR_HANDLING_HPP
#define ERROR_HANDLING_HPP

#include <string>
#include <realm.hpp>
#include "realm_error_type.hpp"

namespace realm {
struct NativeException {
    RealmErrorType type;
    std::string message;
    
    struct Marshallable {
        RealmErrorType type;
        const char* messagesBytes;
        size_t messageLength;
    };
    
    // the return value of this method is tied to the lifetime of this
    // it's ususally fine to pass it as a parameter to a .net callback,
    // but take care of object lifetime when *returning* this value to .net
    Marshallable for_marshalling() const {
        return {
            type,
            message.data(),
            message.size()
        };
    }
};
    
class RowDetachedException : public std::runtime_error {
public:
    RowDetachedException() : std::runtime_error("Attempted to access detached row") {}
};

NativeException convert_exception();

void throw_managed_exception(const NativeException& exception);
    
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
        throw_managed_exception(convert_exception());
        return Default<RetVal>::default_value();
    }
}

} // namespace realm

#endif // ERROR_HANDLING_HPP
