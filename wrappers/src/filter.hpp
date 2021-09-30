////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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

#include <realm/object-store/keypath_helpers.hpp>
#include <realm/table_ref.hpp>
#include <realm/query.hpp>

#include "marshalling.hpp"

inline Results* get_filtered_results(const SharedRealm& realm, const ConstTableRef table, 
                                        Query query, uint16_t* query_buf, size_t query_len,
                                        realm_value_t* arguments, size_t args_count, DescriptorOrdering new_order)
{
    Utf16StringAccessor query_string(query_buf, query_len);

    query_parser::KeyPathMapping mapping;
    realm::populate_keypath_mapping(mapping, *realm);

    std::vector<Mixed> mixed_args;
    mixed_args.reserve(args_count);
    for (size_t i = 0; i < args_count; ++i) {
        if (arguments[i].type != realm_value_type::RLM_TYPE_LINK) {
            mixed_args.push_back(from_capi(arguments[i]));
        }
        else {
            mixed_args.push_back(from_capi(arguments[i].link.object, true));
        }
    }

    Query parsed_query = table->query(query_string, mixed_args, mapping);
    if (auto parsed_ordering = parsed_query.get_ordering()) {
        new_order.append(*parsed_ordering);
    }

    return new Results(realm, query.and_query(std::move(parsed_query)), std::move(new_order));
}
