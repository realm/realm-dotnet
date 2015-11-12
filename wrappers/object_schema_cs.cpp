/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#include <realm.hpp>
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "object-store/shared_realm.hpp"

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
