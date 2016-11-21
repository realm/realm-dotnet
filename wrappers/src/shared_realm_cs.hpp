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
#include "object-store/src/binding_context.hpp"
#include "object_accessor.hpp"

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

namespace realm {
namespace binding {
    
    struct ObservedObjectDetails {
        ObservedObjectDetails(const ObjectSchema& schema, void* managed_object_handle) : schema(schema), managed_object_handle(managed_object_handle) {}

        const ObjectSchema& schema;
        void* managed_object_handle;
    };
    
    class CSharpBindingContext: public BindingContext {
    public:
        CSharpBindingContext(void* managed_realm_handle);
        void did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated) override;
        void add_observed_row(const Object& object, void* managed_object_handle);
        void remove_observed_row(void* managed_object_handle);
        void notify_change(const size_t row_ndx, const size_t table_ndx, const size_t property_index);
        void notify_removed(const size_t row_ndx, const size_t table_ndx);
        
        void* get_managed_realm_handle()
        {
            return m_managed_realm_handle;
        }
        
        std::vector<CSharpBindingContext::ObserverState> get_observed_rows() override
        {
            return m_observed_rows;
        }
    private:
        void* m_managed_realm_handle;
        std::vector<BindingContext::ObserverState> m_observed_rows;

        inline void remove_observed_rows(std::function<bool (const BindingContext::ObserverState*, const ObservedObjectDetails*)> filter)
        {
            if (!m_observed_rows.empty()) {
                for (auto it = m_observed_rows.begin(); it != m_observed_rows.end();) {
                    auto const& details = static_cast<ObservedObjectDetails*>(it->info);
                    if (filter(&*it, details)) {
                        delete(details);
                        it = m_observed_rows.erase(it);
                    } else {
                        ++it;
                    }
                    
                }
            }
        }
    };
}
    
}

#endif /* defined(SHARED_REALM_CS_HPP) */
