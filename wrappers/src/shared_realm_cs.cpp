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


#include "shared_realm_cs.hpp"
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "marshalling.hpp"

#include <object_store.hpp>
#include <binding_context.hpp>
#include <realm.hpp>
#include <object_accessor.hpp>
#include <thread_safe_reference.hpp>

#include <list>
#include <unordered_set>
#include <sstream>

using namespace realm;
using namespace realm::binding;

using NotifyRealmChangedDelegate = void(void* managed_state_handle);
using GetNativeSchemaDelegate = void(SchemaForMarshaling schema, void* managed_callback);
NotifyRealmChangedDelegate* notify_realm_changed = nullptr;
GetNativeSchemaDelegate* get_native_schema = nullptr;

namespace realm {
namespace binding {
    CSharpBindingContext::CSharpBindingContext(void* managed_state_handle) : m_managed_state_handle(managed_state_handle) {}

    void CSharpBindingContext::did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated, bool version_changed)
    {
        notify_realm_changed(m_managed_state_handle);
    }
}

// the name of this class is an ugly hack to get around get_shared_group being private
class TestHelper {
public:
    static bool has_changed(const SharedRealm& realm)
    {
        auto transaction = Realm::Internal::get_transaction_ref(*realm);
        return Realm::Internal::get_db(*realm)->has_changed(transaction);
    }
};
}

extern "C" {

REALM_EXPORT void shared_realm_install_callbacks(NotifyRealmChangedDelegate realm_changed, GetNativeSchemaDelegate get_schema)
{
    notify_realm_changed = realm_changed;
    get_native_schema = get_schema;
}

REALM_EXPORT SharedRealm* shared_realm_open(Configuration configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor pathStr(configuration.path, configuration.path_len);

        Realm::Config config;
        config.path = pathStr.to_string();
        config.in_memory = configuration.in_memory;
        config.max_number_of_active_versions = configuration.max_number_of_active_versions;

        // by definition the key is only allowed to be 64 bytes long, enforced by C# code
        if (encryption_key )
          config.encryption_key = std::vector<char>(encryption_key, encryption_key+64);

        if (configuration.read_only) {
            config.schema_mode = SchemaMode::Immutable;
        } else if (configuration.delete_if_migration_needed) {
            config.schema_mode = SchemaMode::ResetFile;
        }

        if (objects_length > 0) {
            config.schema = create_schema(objects, objects_length, properties);
        }

        config.schema_version = configuration.schema_version;

        if (configuration.managed_migration_handle) {
            config.migration_function = [&configuration](SharedRealm oldRealm, SharedRealm newRealm, Schema schema) {
                std::vector<SchemaObject> schema_objects;
                std::vector<SchemaProperty> schema_properties;

                for (auto& object : oldRealm->schema()) {
                    schema_objects.push_back(SchemaObject::for_marshalling(object, schema_properties, object.is_embedded));
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
            config.should_compact_on_launch_function = [&configuration](uint64_t total_bytes, uint64_t used_bytes) {
                return configuration.should_compact_callback(configuration.managed_should_compact_delegate, total_bytes, used_bytes);
            };
        }

        config.cache = configuration.enable_cache;

        auto realm = Realm::get_shared_realm(config);
        if (!configuration.read_only)
            realm->refresh();

        return new SharedRealm{realm};
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

REALM_EXPORT void shared_realm_close_realm(SharedRealm& realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        realm->close();
    });
}

REALM_EXPORT TableRef* shared_realm_get_table_info(SharedRealm& realm, uint16_t* object_type_buf, size_t object_type_len, ColKey* columns_buffer, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor object_type(object_type_buf, object_type_len);

        auto properties = realm->schema().find(object_type)->persisted_properties;
        for (size_t i = 0; i < properties.size(); i++) {
            columns_buffer[i] = properties.at(i).column_key;
        }
        return new TableRef(ObjectStore::table_for_object_type(realm->read_group(), object_type));
    });
}

REALM_EXPORT uint64_t shared_realm_get_schema_version(SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return realm->schema_version();
    });
}

REALM_EXPORT void shared_realm_begin_transaction(SharedRealm& realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        realm->begin_transaction();
    });
}

REALM_EXPORT void shared_realm_commit_transaction(SharedRealm& realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        realm->commit_transaction();
    });
}

REALM_EXPORT void shared_realm_cancel_transaction(SharedRealm& realm, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        realm->cancel_transaction();
    });
}

REALM_EXPORT bool shared_realm_is_in_transaction(SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return realm->is_in_transaction();
    });
}

REALM_EXPORT bool shared_realm_is_same_instance(SharedRealm& lhs, SharedRealm& rhs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return lhs == rhs;  // just compare raw pointers inside the smart pointers
    });
}

REALM_EXPORT bool shared_realm_refresh(SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return realm->refresh();
    });
}

REALM_EXPORT bool shared_realm_compact(SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> bool {
        return realm->compact();
    });
}

REALM_EXPORT Object* shared_realm_resolve_object_reference(SharedRealm& realm, ThreadSafeReference& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Object(reference.resolve<Object>(realm));
    });
}

REALM_EXPORT List* shared_realm_resolve_list_reference(SharedRealm& realm, ThreadSafeReference& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new List(reference.resolve<List>(realm));
    });
}

REALM_EXPORT Results* shared_realm_resolve_query_reference(SharedRealm& realm, ThreadSafeReference& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(reference.resolve<Results>(realm));
    });
}

REALM_EXPORT SharedRealm* shared_realm_resolve_realm_reference(ThreadSafeReference& reference, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new SharedRealm(Realm::get_shared_realm(std::move(reference)));
    });
}

REALM_EXPORT void thread_safe_reference_destroy(ThreadSafeReference* reference)
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

}

inline const ObjectSchema& find_schema(const SharedRealm& realm, ConstTableRef& table)
{
    realm->read_group();
    const StringData object_name(ObjectStore::object_type_for_table_name(table->get_name()));
    return *realm->schema().find(object_name);
}

namespace realm
{

template<class KeyType>
Object* create_object_unique(const SharedRealm& realm, TableRef& table, const KeyType& key, bool try_update, bool& is_new)
{
    realm->verify_in_write();
    const ObjectSchema& object_schema(find_schema(realm, table));

    const Property& primary_key_property = *object_schema.primary_key_property();

    ObjKey obj_key = table->find_first(primary_key_property.column_key, key);
    Obj obj;

    if (!obj_key) {
        is_new = true;
        obj = table->create_object_with_primary_key(key);
    } else if (!try_update) {
        std::ostringstream string_builder;
        string_builder << key;
        throw SetDuplicatePrimaryKeyValueException(object_schema.name,
                                                   primary_key_property.name,
                                                   string_builder.str());
    } else {
        obj = table->get_object(obj_key);
        is_new = false;
    }

    return new Object(realm, object_schema, obj);
}

extern "C" {

REALM_EXPORT Object* shared_realm_create_object(SharedRealm& realm, TableRef& table, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->verify_in_write();

        return new Object(realm, table->create_object());
    });
}

REALM_EXPORT Object* shared_realm_create_object_int_unique(const SharedRealm& realm, TableRef& table, int64_t key, bool has_value, bool is_nullable, bool try_update, bool& is_new, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (is_nullable) {
            return create_object_unique(realm, table, has_value ? util::some<int64_t>(key) : null(), try_update, is_new);
        } else {
            return create_object_unique(realm, table, key, try_update, is_new);
        }
    });
}

REALM_EXPORT Object* shared_realm_create_object_string_unique(const SharedRealm& realm, TableRef& table, uint16_t* key_buf, size_t key_len, bool try_update, bool& is_new, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        if (key_buf == nullptr) {
            return create_object_unique(realm, table, StringData(), try_update, is_new);
        }

        Utf16StringAccessor key(key_buf, key_len);
        return create_object_unique(realm, table, StringData(key), try_update, is_new);
    });
}

REALM_EXPORT void shared_realm_get_schema(const SharedRealm& realm, void* managed_callback, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        std::vector<SchemaObject> schema_objects;
        std::vector<SchemaProperty> schema_properties;

        for (auto& object : realm->schema()) {
            schema_objects.push_back(SchemaObject::for_marshalling(object, schema_properties, object.is_embedded));
        }

        get_native_schema(SchemaForMarshaling {
            schema_objects.data(),
            static_cast<int>(schema_objects.size()),
            schema_properties.data()
        }, managed_callback);
    });
}

REALM_EXPORT bool shared_realm_has_changed(const SharedRealm& realm)
{
    return TestHelper::has_changed(realm);
}

REALM_EXPORT bool shared_realm_get_is_frozen(const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return realm->is_frozen();
    });
}

REALM_EXPORT SharedRealm* shared_realm_freeze(const SharedRealm& realm, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto frozen_realm = realm->freeze();
        return new SharedRealm{ frozen_realm };
    });
}

}
}
