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

#include "object_accessor.hpp"

namespace realm {
    inline void verify_can_get(const Object* object_ptr) {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_thread();
    }
    
    inline void verify_can_set(const Object* object_ptr) {
        if (object_ptr->realm()->is_closed())
            throw RealmClosedException();
        
        if (!object_ptr->is_valid())
            throw RowDetachedException();
        
        object_ptr->realm()->verify_in_write();
    }
    
    inline size_t get_column_index(const Object* object_ptr, size_t property_index) {
        return object_ptr->get_object_schema().persisted_properties[property_index].table_column;
    }
}
