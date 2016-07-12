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
#include "object-store/src/schema.hpp"
#include "object-store/src/property.hpp"

using namespace realm;

extern "C" {

struct SchemaProperty
{
    char* name;
    PropertyType type;
    char* object_type;
    bool is_nullable;
    bool is_primary;
    bool is_indexed;
};

struct SchemaObject
{
    char* name;
    int properties_start;
    int properties_end;
};

REALM_EXPORT Schema* schema_create(SchemaObject* objects, int objects_length, SchemaProperty* properties, ObjectSchema** handles, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        std::vector<ObjectSchema> object_schemas;
        
        for (int i = 0; i < objects_length; i++) {
            SchemaObject& object = objects[i];
            
            ObjectSchema o;
            o.name = object.name;
            
            for (int n = object.properties_start; n < object.properties_end; n++) {
                SchemaProperty& property = properties[n];
                
                Property p;
                p.name = property.name;
                p.type = property.type;
                p.object_type = property.object_type ? property.object_type : "";
                p.is_nullable = property.is_nullable;
                p.is_primary = property.is_primary;
                p.is_indexed = property.is_indexed;
                
                o.persisted_properties.push_back(std::move(p));
            }
            
            object_schemas.push_back(std::move(o));
        }
        
        Schema* schema = new Schema(object_schemas);
        
        for (auto i = 0; i < objects_length; i++) {
            handles[i] = &(*schema->find(objects[i].name));
        }
        
        return schema;
    });
}
    
REALM_EXPORT Schema* schema_clone(Schema* schema, ObjectSchema** handles, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto clone = new Schema(*schema);
        for (auto i = 0; i < clone->size(); i++) {
            handles[i] = &(*clone->find(*handles[i]));
        }
        
        return clone;
    });
}

REALM_EXPORT void schema_destroy(Schema* schema)
{
    delete schema;
}
}   // extern "C"
