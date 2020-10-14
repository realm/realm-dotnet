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
 
#include "schema_cs.hpp"

using namespace realm;

util::Optional<Schema> create_schema(SchemaObject* objects, int objects_length, SchemaProperty* properties)
{
    std::vector<ObjectSchema> object_schemas;
    object_schemas.reserve(objects_length);
    
    for (int i = 0; i < objects_length; i++) {
        SchemaObject& object = objects[i];
        
        ObjectSchema o;
        o.name = object.name;
        o.is_embedded = object.is_embedded;
        
        for (int n = object.properties_start; n < object.properties_end; n++) {
            SchemaProperty& property = properties[n];
            
            Property p;
            p.name = property.name;
            p.type = property.type;
            p.object_type = property.object_type ? property.object_type : "";
            p.link_origin_property_name = property.link_origin_property_name ? property.link_origin_property_name : "";
            p.is_indexed = property.is_indexed;
            
            if ((p.is_primary = property.is_primary)) {
                o.primary_key = p.name;
            }
            
            if (p.type == PropertyType::LinkingObjects) {
                o.computed_properties.push_back(std::move(p));
            } else {
                o.persisted_properties.push_back(std::move(p));
            }
        }
        
        object_schemas.push_back(std::move(o));
    }
    
    return util::Optional<Schema>(std::move(object_schemas));
}
