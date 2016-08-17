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

#ifndef REALM_SORT_DESCRIPTOR_WRAPPER_HPP
#define REALM_SORT_DESCRIPTOR_WRAPPER_HPP

#include <vector>

struct MarshalableSortClause {
        size_t offset;
        size_t count;
        bool ascending;
};

inline void unflatten_sort_clauses(MarshalableSortClause* sort_clauses, size_t clause_count, size_t* flattened_column_indices, 
    std::vector<std::vector<size_t>>& column_indices, std::vector<bool>& ascending)
{
    for(auto i = 0; i < clause_count; ++i) {
        ascending.push_back(sort_clauses[i].ascending);
        column_indices.push_back(std::vector<size_t>(flattened_column_indices + sort_clauses[i].offset, 
            flattened_column_indices + sort_clauses[i].offset + sort_clauses[i].count));
    }
}

//struct SortDescriptorMarshaller {
//    struct SortClause {
//    };
//
//    SortClause* clauses;
//    size_t clause_count;
//
//    size_t* flattened_column_indices;
//
//    realm::Table* table;
//
//    operator realm::SortDescriptor() const
//    {
//        std::vector<std::vector<size_t>> column_indices;
//        std::vector<bool> ascending;
//
//        for(auto i = 0; i != clause_count; ++i) {
//            auto& clause = clauses[i];
//
//            column_indices.push_back(std::vector<size_t>(flattened_column_indices + clause.offset, 
//                flattened_column_indices + clause.offset + clause.count));
//            ascending.push_back(clause.ascending);
//        }
//
//        return realm::SortDescriptor(*table, column_indices, ascending);
//    }
//};

#endif //REALM_SORT_DESCRIPTOR_WRAPPER_HPP
