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
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/property.hpp"

using namespace realm;

extern "C" {

REALM_EXPORT ObjectSchema* object_schema_create(const char* name)
{
    return handle_errors([&]() {
        auto p = new ObjectSchema;
        p->name = name;
        return p;
    });
}

REALM_EXPORT void object_schema_destroy(ObjectSchema* object_schema)
{
    handle_errors([&]() {
        delete object_schema;
    });
}

REALM_EXPORT void object_schema_add_property(ObjectSchema* cls, const char* name, DataType type, const char* object_type,
                                      bool is_primary, bool is_indexed, bool is_nullable)
{
    handle_errors([&]() {
        Property p;
        p.name = name;
        p.type = static_cast<PropertyType>(type);
        p.object_type = object_type ? std::string(object_type) : std::string();
        p.is_primary = is_primary;
        p.is_indexed = is_indexed;
        p.is_nullable = is_nullable;
        cls->properties.push_back(std::move(p));
    });
}

}   // extern "C"
