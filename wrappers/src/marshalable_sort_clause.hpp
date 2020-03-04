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

#pragma once

#include <vector>
#include <property.hpp>

using namespace realm;

struct MarshalableSortClause {
    size_t offset;
    size_t count;
    bool ascending;
};

inline void unflatten_sort_clauses(MarshalableSortClause* sort_clauses, size_t clause_count, size_t* flattened_property_indices,
                                   std::vector<std::vector<ColKey>>& column_keys, std::vector<bool>& ascending, const std::vector<Property>& properties)
{
    ascending.reserve(clause_count);
    column_keys.reserve(clause_count);

    std::vector<ColKey> current_keys;
    for (size_t i = 0; i < clause_count; ++i) {
        ascending.push_back(sort_clauses[i].ascending);

        current_keys.reserve(sort_clauses[i].count);
        for(auto j = sort_clauses[i].offset; j < sort_clauses[i].offset + sort_clauses[i].count; ++j) {
            const Property& property = properties[flattened_property_indices[j]];
            const ColKey& column_key = property.column_key;
            current_keys.push_back(column_key);
        }
        column_keys.push_back(std::move(current_keys));
    }
}
