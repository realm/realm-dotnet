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
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"

using namespace realm;
using namespace realm::binding;

extern "C" {

REALM_EXPORT void row_destroy(Row* row_ptr)
{
    handle_errors([&]() {
        delete row_ptr;
    });
}

REALM_EXPORT size_t row_get_row_index(const Row* row_ptr)
{
    return handle_errors([&]() {
        if (!row_ptr->is_attached())
            throw RowDetachedException();
        return row_ptr->get_index();
    });
}

REALM_EXPORT size_t row_get_is_attached(const Row* row_ptr)
{
    return handle_errors([&]() {
        return bool_to_size_t(row_ptr->is_attached());
    });
}

}   // extern "C"
