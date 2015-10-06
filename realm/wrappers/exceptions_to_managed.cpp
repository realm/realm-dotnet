/*
* Copyright 2015 Realm Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

#include "exceptions_to_managed.hpp"
#include "realm_export_decls.h"
#include <cassert>

using namespace realm;

using ManagedExceptionThrowerT = void(*)(size_t exceptionCode, void* utf8Str, size_t strLen);

// CALLBACK TO THROW IN MANAGED SPACE
static ManagedExceptionThrowerT ManagedExceptionThrower = nullptr;

void realm::ThrowManaged(RealmExceptionCodes exceptionCode, const std::string& message)
{
    assert(ManagedExceptionThrower != nullptr);
    ManagedExceptionThrower((size_t)exceptionCode, (void*)message.data(), message.size());
}


void realm::ThrowManaged()
{
    assert(ManagedExceptionThrower != nullptr);
    ManagedExceptionThrower((size_t)RealmExceptionCodes::RealmError, 0, 0);
}



extern "C" {
    
    REALM_EXPORT void set_exception_thrower(ManagedExceptionThrowerT userThrower)
    {
        ManagedExceptionThrower = userThrower;
    }

}