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
 
#ifndef SORT_ORDER_WRAPPER_HPP
#define SORT_ORDER_WRAPPER_HPP

#include <memory>
#include "object-store/src/results.hpp"

namespace realm {
  
/// Simple wrapper to keep the Table* we need to lookup column indices when we add clauses to a SortOrder.
struct SortOrderWrapper {
    SortOrder sort_order;
    Table* table;

    SortOrderWrapper(Table* in_table) : table(in_table) {}

    void add_sort(size_t col, bool ascendingCol)
    {
      sort_order.column_indices.push_back(col);
      sort_order.ascending.push_back(ascendingCol);
    }
};

}

#endif  // SORT_ORDER_WRAPPER_HPP
