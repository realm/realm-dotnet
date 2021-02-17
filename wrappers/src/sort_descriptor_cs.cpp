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

#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_realm_cs.hpp"

#include <realm.hpp>
#include <realm/object-store/shared_realm.hpp>
#include <realm/object-store/schema.hpp>

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void sort_descriptor_destroy(DescriptorOrdering* descriptor)
{
    delete descriptor;
}

REALM_EXPORT void sort_descriptor_add_clause(DescriptorOrdering& descriptor, TableKey table_key, SharedRealm& realm, size_t* property_chain, size_t properties_count, bool ascending, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        std::vector<ColKey> column_keys;
        column_keys.reserve(properties_count);

        const std::vector<Property>* properties = &realm->schema().find(table_key)->persisted_properties;

        for (size_t i = 0; i < properties_count; ++i) {
            const Property& property = properties->at(property_chain[i]);
            column_keys.push_back(property.column_key);

            if (property.type == PropertyType::Object) {
                properties = &realm->schema().find(property.object_type)->persisted_properties;
            }
        }

        descriptor.append_sort(SortDescriptor({ column_keys }, { ascending }), SortDescriptor::MergeMode::append);
    });
}

}   // extern "C"
