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

#ifndef REALM_MARSHALABLE_SORT_CLAUSE_HPP
#define REALM_MARSHALABLE_SORT_CLAUSE_HPP

#include <vector>
#include "property.hpp"

using namespace realm;

struct MarshalableSortClause {
    size_t offset;
    size_t count;
    bool ascending;
};

inline void unflatten_sort_clauses(MarshalableSortClause* sort_clauses, size_t clause_count, size_t* flattened_property_indices,
                                   std::vector<std::vector<size_t>>& column_indices, std::vector<bool>& ascending, const std::vector<Property>& properties)
{
    ascending.reserve(clause_count);
    column_indices.reserve(clause_count);

    std::vector<size_t> current_indices;
    for(size_t i = 0; i < clause_count; ++i) {
        ascending.push_back(sort_clauses[i].ascending);
        
        current_indices.clear();
        current_indices.reserve(sort_clauses[i].count);
        for(auto j = sort_clauses[i].offset; j < sort_clauses[i].offset + sort_clauses[i].count; ++j) {
            size_t column_index = properties[flattened_property_indices[j]].table_column;
            current_indices.push_back(column_index);
        }
        column_indices.push_back(std::move(current_indices));
    }
}

#endif //REALM_MARSHALABLE_SORT_CLAUSE_HPP
