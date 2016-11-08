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

#ifndef SHARED_REALM_CS_HPP
#define SHARED_REALM_CS_HPP

#include "shared_realm.hpp"
#include "schema_cs.hpp"

class ManagedExceptionDuringMigration : public std::runtime_error
{
public:
    ManagedExceptionDuringMigration() : std::runtime_error("Uncaught .NET exception during Realm migration") {
    }
};

struct Configuration
{
    uint16_t* path;
    size_t path_len;
    
    bool read_only;
    
    bool in_memory;
    
    bool delete_if_migration_needed;
    
    uint64_t schema_version;
    
    bool (*migration_callback)(realm::SharedRealm* old_realm, realm::SharedRealm* new_realm, SchemaForMarshaling, uint64_t schema_version, void* managed_migration_handle);
    void* managed_migration_handle;
};

#endif /* defined(SHARED_REALM_CS_HPP) */
