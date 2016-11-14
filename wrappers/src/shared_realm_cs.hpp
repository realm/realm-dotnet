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

using NotifyRealmChangedT = void(*)(void* managed_realm_handle);
NotifyRealmChangedT notify_realm_changed = nullptr;

using NotifyRealmObjectChangedT = void(*)(void* managed_realm_object_handle, size_t property_ndx);
NotifyRealmObjectChangedT notify_realm_object_changed = nullptr;

namespace realm {
namespace binding {
    
    struct ObservedObjectDetails {
        ObservedObjectDetails(const ObjectSchema& schema, void* managed_object_handle) : schema(schema), managed_object_handle(managed_object_handle) {}
        
        const ObjectSchema& schema;
        void* managed_object_handle;
    };
    
    class CSharpBindingContext: public BindingContext {
    public:
        CSharpBindingContext(void* managed_realm_handle) : m_managed_realm_handle(managed_realm_handle) {}
        
        void did_change(std::vector<ObserverState> const& observed, std::vector<void*> const& invalidated) override
        {
            for (auto const& o : observed) {
                for (auto const& change : o.changes) {
                    if (change.kind == ColumnInfo::Kind::Set) {
                        auto const& observed_object_details = static_cast<ObservedObjectDetails*>(o.info);
                        notify_realm_object_changed(observed_object_details->managed_object_handle, get_property_index(observed_object_details->schema, change.initial_column_index));
                    }
                }
            }
            
            for (auto const& o : invalidated) {
                remove_observed_row(o);
            }
            
            notify_realm_changed(m_managed_realm_handle);
        }
        
        std::vector<ObserverState> get_observed_rows() override
        {
            return observed_rows;
        }
        
        void add_observed_row(const Object& object, void* managed_object_handle)
        {
            auto observer_state = BindingContext::ObserverState();
            observer_state.row_ndx = object.row().get_index();
            observer_state.table_ndx = object.row().get_table()->get_index_in_group();
            observer_state.info = new ObservedObjectDetails(object.get_object_schema(), managed_object_handle);
            observed_rows.push_back(observer_state);
        }
        
        void remove_observed_row(void* managed_object_handle)
        {
            if (!observed_rows.empty()) {
                observed_rows.erase(std::remove_if(observed_rows.begin(), observed_rows.end(), [&](auto const& row) { return get_managed_object_handle(row.info) == managed_object_handle; }));
            }
        }
        
        void* get_managed_realm_handle() const {
            return m_managed_realm_handle;
        }
        
        void notify_change(const size_t row_ndx, const size_t table_ndx, const size_t property_index) const
        {
            for (auto const& o : observed_rows) {
                if (o.row_ndx == row_ndx && o.table_ndx == table_ndx) {
                    notify_realm_object_changed(get_managed_object_handle(o.info), property_index);
                }
            }
        }
        
        void notify_removed(const size_t row_ndx, const size_t table_ndx)
        {
            if (!observed_rows.empty()) {
                observed_rows.erase(std::remove_if(observed_rows.begin(), observed_rows.end(),
                                                   [&](auto const& row) { return row.row_ndx == row_ndx && row.table_ndx == table_ndx; }));
            }
        }
    private:
        void* m_managed_realm_handle;
        std::vector<BindingContext::ObserverState> observed_rows;
        
        inline size_t get_property_index(const ObjectSchema& schema, const size_t column_index) {
            auto const& props = schema.persisted_properties;
            for (size_t i = 0; i < props.size(); ++i) {
                if (props[i].table_column == column_index) {
                    return i;
                }
            }
            
            return -1;
        }
        
        inline void* get_managed_object_handle(void* info) const {
            return static_cast<ObservedObjectDetails*>(info)->managed_object_handle;
        }
    };
}
    
}

#endif /* defined(SHARED_REALM_CS_HPP) */
