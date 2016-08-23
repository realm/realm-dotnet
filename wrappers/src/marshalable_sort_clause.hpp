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

struct MarshalableSortClause {
    size_t offset;
    size_t count;
    bool ascending;
};

inline void unflatten_sort_clauses(MarshalableSortClause* sort_clauses, size_t clause_count, size_t* flattened_column_indices, 
    std::vector<std::vector<size_t>>& column_indices, std::vector<bool>& ascending)
{
    ascending.reserve(clause_count);
    column_indices.reserve(clause_count);

    for(auto i = 0; i < clause_count; ++i) {
        ascending.push_back(sort_clauses[i].ascending);
        column_indices.push_back(std::vector<size_t>(flattened_column_indices + sort_clauses[i].offset, 
            flattened_column_indices + sort_clauses[i].offset + sort_clauses[i].count));
    }
}

#endif //REALM_MARSHALABLE_SORT_CLAUSE_HPP
