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
#include <sstream>

using namespace realm;
using namespace realm::binding;

using NotifyRealmChangedDelegate = void(void* managed_state_handle);
NotifyRealmChangedDelegate* notify_realm_changed = nullptr;

namespace realm {
namespace binding {
    CSharpBindingContext::CSharpBindingContext(void* managed_state_handle) : m_managed_state_handle(managed_state_handle) {}
    
    void CSharpBindingContext::did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated, bool version_changed)
    {
        notify_realm_changed(m_managed_state_handle);
    }
}
}

extern "C" {
    
    
REALM_EXPORT void register_notify_realm_changed(NotifyRealmChangedDelegate notifier)
{
    notify_realm_changed = notifier;
}
    
REALM_EXPORT SharedRealm* shared_realm_open(Configuration configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor pathStr(configuration.path, configuration.path_len);

        Realm::Config config;
        config.path = pathStr.to_string();
        config.in_memory = configuration.in_memory;

        // by definition the key is only allowed to be 64 bytes long, enforced by C# code
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
        
        if (configuration.managed_should_compact_delegate) {
#ifndef _WIN32
            config.should_compact_on_launch_function = [&configuration](uint64_t total_bytes, uint64_t used_bytes) {
                return configuration.should_compact_callback(configuration.managed_should_compact_delegate, total_bytes, used_bytes);
            };
#else
            throw std::logic_error("Compact isn't supported on Windows yet.");
#endif
            
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
    return handle_errors(ex, [&]() -> bool {
#ifndef _WIN32
        return (*realm)->compact();
#else
        throw std::logic_error("Compact isn't supported on Windows yet.");
#endif
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
    
REALM_EXPORT void shared_realm_write_copy(SharedRealm* realm, uint16_t* path, size_t path_len, char* encryption_key, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor pathStr(path, path_len);

        // by definition the key is only allowed to be 64 bytes long, enforced by C# code
        realm->get()->write_copy(pathStr, BinaryData(encryption_key, encryption_key ? 64 : 0));
    });
}

#pragma mark - Creating objects

inline const ObjectSchema& find_schema(const SharedRealm& realm, const Table& table)
{
    const StringData object_name(ObjectStore::object_type_for_table_name(table.get_name()));
    return *realm->schema().find(object_name);
}

namespace realm
{
// specialize overload
template <>
size_t Table::find_first(size_t column_ndx, null) const
{
    return find_first_null(column_ndx);
}

// ditto
template <class OS>
OS& operator<<(OS& os, const null&)
{
    os << "(null)";
    return os;
}


template <>
size_t Table::set_unique(size_t column_ndx, size_t row_ndx, util::Optional<int64_t> value)
{
    if (value) {
        return set_unique(column_ndx, row_ndx, *value);
    } else {
        return set_unique(column_ndx, row_ndx, null());
    }
}
}

template<class KeyType>
Object* create_object_unique(const SharedRealm& realm, Table& table, const KeyType& key, bool try_update, bool& is_new)
{
    realm->verify_in_write();
    const ObjectSchema& object_schema(find_schema(realm, table));

    const Property& primary_key_property = *object_schema.primary_key_property();
    size_t column_index = primary_key_property.table_column;

    size_t row_index = table.find_first(column_index, key);

    if (row_index == realm::not_found) {
        is_new = true;
#if REALM_ENABLE_SYNC
        row_index = sync::create_object_with_primary_key(realm->read_group(), table, key);
#else
        row_index = table.add_empty_row();
        table.set_unique(column_index, row_index, key);
#endif
    } else if (!try_update) {
        std::ostringstream string_builder;
        string_builder << key;
        throw SetDuplicatePrimaryKeyValueException(object_schema.name,
                                                   primary_key_property.name,
                                                   string_builder.str());
    } else {
        is_new = false;
    }


    return new Object(realm, object_schema, table.get(row_index));
}

extern "C" {

REALM_EXPORT Object* shared_realm_create_object(SharedRealm* realm, Table* table, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        (*realm)->verify_in_write();
        
#if REALM_ENABLE_SYNC
        size_t row_index = sync::create_object((*realm)->read_group(), *table);
#else
        size_t row_index = table->add_empty_row();
#endif // REALM_ENABLE_SYNC

        const std::string object_name(ObjectStore::object_type_for_table_name(table->get_name()));
        auto& object_schema = *realm->get()->schema().find(object_name);
        
        return new Object(*realm, object_schema, table->get(row_index));
    });
}

REALM_EXPORT Object* shared_realm_create_object_int_unique(const SharedRealm& realm, Table& table, int64_t key, bool is_nullable, bool try_update, bool& is_new, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (is_nullable) {
            return create_object_unique(realm, table, util::some<int64_t>(key), try_update, is_new);
        } else {
            return create_object_unique(realm, table, key, try_update, is_new);
        }
    });
}

REALM_EXPORT Object* shared_realm_create_object_string_unique(const SharedRealm& realm, Table& table, uint16_t* key_buf, size_t key_len, bool try_update, bool& is_new, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor key(key_buf, key_len);
        return create_object_unique(realm, table, StringData(key), try_update, is_new);
    });
}

REALM_EXPORT Object* shared_realm_create_object_null_unique(const SharedRealm& realm, Table& table, bool try_update, bool& is_new, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return create_object_unique(realm, table, null(), try_update, is_new);
    });
}

}
