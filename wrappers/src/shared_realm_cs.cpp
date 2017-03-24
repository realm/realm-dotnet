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
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "marshalling.hpp"
#include "object_accessor.hpp"
#include "object-store/src/object_store.hpp"
#include "object-store/src/binding_context.hpp"
#include <list>
#include "shared_realm_cs.hpp"
#include "object-store/src/binding_context.hpp"
#include <unordered_set>
#include "object-store/src/thread_safe_reference.hpp"

using namespace realm;
using namespace realm::binding;

using NotifyRealmChangedDelegate = void(void* managed_state_handle);
NotifyRealmChangedDelegate* notify_realm_changed = nullptr;

using NotifyRealmObjectChangedDelegate = bool(void* managed_realm_object_handle, size_t property_ndx);
NotifyRealmObjectChangedDelegate* notify_realm_object_changed = nullptr;

using FreeGCHandleDelegate = void(void* managed_handle);
FreeGCHandleDelegate* free_gc_handle = nullptr;

namespace realm {
namespace binding {
    inline size_t get_property_index(const ObjectSchema& schema, const size_t column_index) {
        auto const& props = schema.persisted_properties;
        for (size_t i = 0; i < props.size(); ++i) {
            if (props[i].table_column == column_index) {
                return i;
            }
        }
        
        return -1;
    }
    
    CSharpBindingContext::CSharpBindingContext(void* managed_state_handle) : m_managed_state_handle(managed_state_handle) {}
    
    void CSharpBindingContext::did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated, bool version_changed)
    {
        std::unordered_set<void*> toRemove;

        for (auto const& o : observed) {
            for (auto const& change : o.changes) {
                if (change.kind == CSharpBindingContext::ColumnInfo::Kind::Set) {
                    auto const& observed_object_details = static_cast<ObservedObjectDetails*>(o.info);
                    
                    if (toRemove.find(observed_object_details->managed_object_handle) == toRemove.end() &&
                        !notify_realm_object_changed(observed_object_details->managed_object_handle, get_property_index(observed_object_details->schema, change.initial_column_index))) {
                        toRemove.insert(observed_object_details->managed_object_handle);
                    }
                }
            }
        }
        
        for (auto const& o : invalidated) {
            remove_observed_row(o);
        }
        
        for (auto const& o : toRemove) {
            remove_observed_row(o);
        }
        
        notify_realm_changed(m_managed_state_handle);
    }
    
    void CSharpBindingContext::add_observed_row(const Object& object, void* managed_object_handle)
    {
        auto observer_state = BindingContext::ObserverState();
        observer_state.row_ndx = object.row().get_index();
        observer_state.table_ndx = object.row().get_table()->get_index_in_group();
        observer_state.info = new ObservedObjectDetails(object.get_object_schema(), managed_object_handle);
        m_observed_rows.push_back(std::move(observer_state));
    }
    
    void CSharpBindingContext::remove_observed_row(void* managed_object_handle)
    {
        remove_observed_rows([&](auto const* observer, auto const* details) {
            return details->managed_object_handle == managed_object_handle;
        });
    }
    
    void CSharpBindingContext::notify_change(const size_t row_ndx, const size_t table_ndx, const size_t property_index)
    {
        std::vector<void*> toNotify;
        
        for (auto const& o : m_observed_rows) {
            if (o.row_ndx == row_ndx && o.table_ndx == table_ndx) {
                auto const& details = static_cast<ObservedObjectDetails*>(o.info);
                toNotify.push_back(details->managed_object_handle);
            }
        }
        
        std::unordered_set<void*> toRemove;
        for (void* handle : toNotify) {
            if (toRemove.find(handle) == toRemove.end() &&
                !notify_realm_object_changed(handle, property_index)) {
                toRemove.insert(handle);
            }
        }
        
        for (auto const& o : toRemove) {
            remove_observed_row(o);
        }
    }
    
    void CSharpBindingContext::notify_removed(const size_t row_ndx, const size_t table_ndx)
    {
        remove_observed_rows([&](auto const* observer, auto const* details) {
            return observer->row_ndx == row_ndx && observer->table_ndx == table_ndx;
        });
    }

    CSharpBindingContext::~CSharpBindingContext()
    {
        free_gc_handle(m_managed_state_handle);
    }
}
    
}

extern "C" {
    
    
REALM_EXPORT void register_notify_realm_changed(NotifyRealmChangedDelegate notifier)
{
    notify_realm_changed = notifier;
}
    
REALM_EXPORT void register_notify_realm_object_changed(NotifyRealmObjectChangedDelegate notifier)
{
    notify_realm_object_changed = notifier;
}

REALM_EXPORT void realm_install_gchandle_deleter(FreeGCHandleDelegate deleter)
{
    free_gc_handle = deleter;
}

REALM_EXPORT SharedRealm* shared_realm_open(Configuration configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor pathStr(configuration.path, configuration.path_len);

        Realm::Config config;
        config.path = pathStr.to_string();
        config.in_memory = configuration.in_memory;

        // by definition the key is only allowwed to be 64 bytes long, enforced by C# code
        if (encryption_key )
          config.encryption_key = std::vector<char>(encryption_key, encryption_key+64);

        if (configuration.read_only) {
            config.schema_mode = SchemaMode::ReadOnly;
        } else if (configuration.delete_if_migration_needed) {
            config.schema_mode = SchemaMode::ResetFile;
        }
        
        config.schema = create_schema(objects, objects_length, properties);
        config.schema_version = configuration.schema_version;

        if (configuration.managed_migration_handle) {
            config.migration_function = [&configuration](SharedRealm oldRealm, SharedRealm newRealm, Schema schema) {
                std::vector<SchemaObject> schema_objects;
                std::vector<SchemaProperty> schema_properties;
                
                for (auto& object : oldRealm->schema()) {
                    schema_objects.push_back(SchemaObject::for_marshalling(object, schema_properties));
                }
                
                SchemaForMarshaling schema_for_marshaling {
                    schema_objects.data(),
                    static_cast<int>(schema_objects.size()),
                    
                    schema_properties.data()
                };
                
                if (!configuration.migration_callback(&oldRealm, &newRealm, schema_for_marshaling, oldRealm->schema_version(), configuration.managed_migration_handle)) {
                    throw ManagedExceptionDuringMigration();
                }
            };
        }
        
        return new SharedRealm{Realm::get_shared_realm(config)};
    });
}

REALM_EXPORT void shared_realm_set_managed_state_handle(SharedRealm& realm, void* managed_state_handle, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        REALM_ASSERT(realm->m_binding_context == nullptr);
        realm->m_binding_context = std::unique_ptr<realm::BindingContext>(new CSharpBindingContext(managed_state_handle));
        realm->m_binding_context->realm = realm;
    });
}

REALM_EXPORT void* shared_realm_get_managed_state_handle(SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> void* {
        if (realm->m_binding_context == nullptr) {
            return nullptr;
        }
        
        auto const& csharp_context = static_cast<CSharpBindingContext*>(realm->m_binding_context.get());
        return csharp_context->get_managed_state_handle();
    });
}
    
REALM_EXPORT void shared_realm_add_observed_object(SharedRealm& realm, const Object& object, void* managed_realm_object_handle, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        auto const& csharp_context = static_cast<CSharpBindingContext*>(realm->m_binding_context.get());
        csharp_context->add_observed_row(object, managed_realm_object_handle);
    });
}
    
REALM_EXPORT void shared_realm_remove_observed_object(SharedRealm& realm, void* managed_realm_object_handle, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        if (realm->m_binding_context != nullptr) {
            auto const& csharp_context = static_cast<CSharpBindingContext*>(realm->m_binding_context.get());
            csharp_context->remove_observed_row(managed_realm_object_handle);
        }
    });
}
    
REALM_EXPORT void shared_realm_destroy(SharedRealm* realm)
{
    delete realm;
}

REALM_EXPORT void shared_realm_close_realm(SharedRealm* realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        (*realm)->close();
    });
}

REALM_EXPORT Table* shared_realm_get_table(SharedRealm* realm, uint16_t* object_type, size_t object_type_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor str(object_type, object_type_len);

        std::string table_name = ObjectStore::table_name_for_object_type(str);
        auto result = LangBindHelper::get_table((*realm)->read_group(), table_name);
        if (!result)
            throw std::logic_error("The table named '" + table_name + "' was not found");

        return result;
    });
}

REALM_EXPORT uint64_t shared_realm_get_schema_version(SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return (*realm)->schema_version();
    });
}

REALM_EXPORT void shared_realm_begin_transaction(SharedRealm* realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        (*realm)->begin_transaction();
    });
}

REALM_EXPORT void shared_realm_commit_transaction(SharedRealm* realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        (*realm)->commit_transaction();
    });
}

REALM_EXPORT void shared_realm_cancel_transaction(SharedRealm* realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        (*realm)->cancel_transaction();
    });
}

REALM_EXPORT size_t shared_realm_is_in_transaction(SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return bool_to_size_t((*realm)->is_in_transaction());
    });
}

REALM_EXPORT size_t shared_realm_is_same_instance(SharedRealm* lhs, SharedRealm* rhs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return *lhs == *rhs;  // just compare raw pointers inside the smart pointers
    });
}

REALM_EXPORT size_t shared_realm_refresh(SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return bool_to_size_t((*realm)->refresh());
    });
}

REALM_EXPORT bool shared_realm_compact(SharedRealm* realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return (*realm)->compact();
    });
}
    
REALM_EXPORT Object* shared_realm_resolve_object_reference(SharedRealm* realm, ThreadSafeReference<Object>& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Object((*realm)->resolve_thread_safe_reference(std::move(reference)));
    });
}

REALM_EXPORT List* shared_realm_resolve_list_reference(SharedRealm* realm, ThreadSafeReference<List>& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new List((*realm)->resolve_thread_safe_reference(std::move(reference)));
    });
}

REALM_EXPORT Results* shared_realm_resolve_query_reference(SharedRealm* realm, ThreadSafeReference<Results>& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results((*realm)->resolve_thread_safe_reference(std::move(reference)));
    });
}
    
REALM_EXPORT void thread_safe_reference_destroy(ThreadSafeReferenceBase* reference)
{
    delete reference;
}
    
}
