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

#pragma once

#include <vector>
#include "object-store/src/schema.hpp"
#include "object-store/src/property.hpp"

struct SchemaProperty
{
    const char* name;
    realm::PropertyType type;
    const char* object_type;
    bool is_nullable;
    bool is_primary;
    bool is_indexed;
    
    static SchemaProperty for_marshalling(const realm::Property&);
};

struct SchemaObject
{
    const char* name;
    int properties_start;
    int properties_end;
    
    static SchemaObject for_marshalling(const realm::ObjectSchema&, std::vector<SchemaProperty>&);
};

struct SchemaForMarshaling
{
    realm::Schema* handle;
    
    SchemaObject* objects;
    realm::ObjectSchema** object_handles;
    int objects_len;
    
    SchemaProperty* properties;
    
};

SchemaProperty SchemaProperty::for_marshalling(const realm::Property& property)
{
    return {
        property.name.c_str(),
        property.type,
        property.object_type.c_str(),
        property.is_nullable,
        property.is_primary,
        property.is_indexed
    };
}

SchemaObject SchemaObject::for_marshalling(const realm::ObjectSchema& object, std::vector<SchemaProperty>& properties)
{
    SchemaObject ret;
    ret.name = object.name.c_str();
    
    ret.properties_start = static_cast<int>(properties.size());
    for (const auto& property : object.persisted_properties) {
        properties.push_back(SchemaProperty::for_marshalling(property));
    }
    ret.properties_end = static_cast<int>(properties.size());
    
    return ret;
}