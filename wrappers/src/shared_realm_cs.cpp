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


#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "shared_realm_cs.hpp"

#include <realm.hpp>
#include <realm/object-store/object_store.hpp>
#include <realm/object-store/binding_context.hpp>
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>
#include <realm/object-store/sync/async_open_task.hpp>

#include <list>
#include <unordered_set>
#include <sstream>

using SharedAsyncOpenTask = std::shared_ptr<AsyncOpenTask>;

using namespace realm;
using namespace realm::binding;

namespace realm {
namespace binding {
    void (*s_open_realm_callback)(void* task_completion_source, ThreadSafeReference* ref, int32_t error_code, const char* message, size_t message_len);
    void (*s_realm_changed)(void* managed_state_handle);
    void (*s_get_native_schema)(SchemaForMarshaling schema, void* managed_callback);
    void (*s_on_binding_context_destructed)(void* managed_handle);

    CSharpBindingContext::CSharpBindingContext(void* managed_state_handle) : m_managed_state_handle(managed_state_handle) {}

    void CSharpBindingContext::did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated, bool version_changed)
    {
        s_realm_changed(m_managed_state_handle);
    }

    CSharpBindingContext::~CSharpBindingContext()
    {
        s_on_binding_context_destructed(m_managed_state_handle);
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

Realm::Config get_shared_realm_config(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key)
{
    Realm::Config config;
    config.schema_mode = SchemaMode::AdditiveDiscovered;

    if (objects_length > 0) {
        config.schema = create_schema(objects, objects_length, properties);
    }

    config.schema_version = configuration.schema_version;
    config.max_number_of_active_versions = configuration.max_number_of_active_versions;

    std::string realm_url(Utf16StringAccessor(sync_configuration.url, sync_configuration.url_len));

    config.sync_config = std::make_shared<SyncConfig>(*sync_configuration.user, realm_url);
    config.sync_config->error_handler = handle_session_error;
    config.sync_config->client_resync_mode = ClientResyncMode::Manual;
    config.sync_config->stop_policy = sync_configuration.session_stop_policy;
    config.path = Utf16StringAccessor(configuration.path, configuration.path_len);

    // by definition the key is only allowed to be 64 bytes long, enforced by C# code
    if (encryption_key) {
        auto& key = *reinterpret_cast<std::array<char, 64>*>(encryption_key);

        config.encryption_key = std::vector<char>(key.begin(), key.end());
        config.sync_config->realm_encryption_key = key;
    }

    config.cache = configuration.enable_cache;

    return config;
}
}

extern "C" {

typedef uint32_t realm_table_key;

REALM_EXPORT void shared_realm_install_callbacks(decltype(s_realm_changed) realm_changed, decltype(s_get_native_schema) get_schema, decltype(s_open_realm_callback) open_callback, decltype(s_on_binding_context_destructed) on_binding_context_destructed)
{
    s_realm_changed = realm_changed;
    s_get_native_schema = get_schema;
    s_open_realm_callback = open_callback;
    s_on_binding_context_destructed = on_binding_context_destructed;
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

REALM_EXPORT SharedAsyncOpenTask* shared_realm_open_with_sync_async(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, void* task_completion_source, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto config = get_shared_realm_config(configuration, sync_configuration, objects, objects_length, properties, encryption_key);

        auto task = Realm::get_synchronized_realm(config);
        task->start([task_completion_source](ThreadSafeReference ref, std::exception_ptr error) {
            if (error) {
                try {
                    std::rethrow_exception(error);
                }
                catch (const std::system_error& system_error) {
                    const std::error_code& ec = system_error.code();
                    s_open_realm_callback(task_completion_source, nullptr, ec.value(), ec.message().c_str(), ec.message().length());
                }
            }
            else {
                s_open_realm_callback(task_completion_source, new ThreadSafeReference(std::move(ref)), 0, nullptr, 0);
            }
        });

        return new SharedAsyncOpenTask(task);
    });
}

REALM_EXPORT SharedRealm* shared_realm_open_with_sync(Configuration configuration, SyncConfiguration sync_configuration, SchemaObject* objects, int objects_length, SchemaProperty* properties, uint8_t* encryption_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto config = get_shared_realm_config(configuration, sync_configuration, objects, objects_length, properties, encryption_key);

        auto realm = Realm::get_shared_realm(config);
        if (!configuration.read_only)
            realm->refresh();

        return new SharedRealm(realm);
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

REALM_EXPORT realm_table_key shared_realm_get_table_key(SharedRealm& realm, uint16_t* object_type_buf, size_t object_type_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor object_type(object_type_buf, object_type_len);
        return ObjectStore::table_for_object_type(realm->read_group(), object_type)->get_key().value;
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

enum class ThreadSafeReferenceType : uint8_t {
    Object = 0,
    List,
    Results,
    Set,
    Dictionary
};

REALM_EXPORT void* shared_realm_resolve_reference(SharedRealm& realm, ThreadSafeReference& reference, ThreadSafeReferenceType type, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]()-> void*{
        switch (type)
        {
        case ThreadSafeReferenceType::Object:
            return new Object(reference.resolve<Object>(realm));
        case ThreadSafeReferenceType::List:
            return new List(reference.resolve<List>(realm));
        case ThreadSafeReferenceType::Results:
            return new Results(reference.resolve<Results>(realm));
        case ThreadSafeReferenceType::Set:
            return new object_store::Set(reference.resolve<object_store::Set>(realm));
        case ThreadSafeReferenceType::Dictionary:
            return new object_store::Dictionary(reference.resolve<object_store::Dictionary>(realm));
        default:
            REALM_UNREACHABLE();
        }
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

REALM_EXPORT Object* shared_realm_create_object(SharedRealm& realm, TableKey table_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->verify_in_write();

        return new Object(realm, get_table(realm, table_key)->create_object());
    });
}

REALM_EXPORT Object* shared_realm_create_object_unique(const SharedRealm& realm, TableKey table_key, realm_value_t primitive, bool try_update, bool& is_new, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->verify_in_write();

        const TableRef table = get_table(realm, table_key);
        const ObjectSchema& object_schema = *realm->schema().find(table_key);
        const Property& primary_key_property = *object_schema.primary_key_property();

        if (!primary_key_property.type_is_nullable() && primitive.is_null()) {
            throw NotNullableException(object_schema.name, primary_key_property.name);
        }

        if (!primitive.is_null() && to_capi(primary_key_property.type) != primitive.type) {
            throw PropertyTypeMismatchException(object_schema.name, primary_key_property.name, to_string(primary_key_property.type), to_string(primitive.type));
        }

        auto val = from_capi(primitive);
        auto obj_key = table->find_first(primary_key_property.column_key, val);

        Obj obj;
        if (!obj_key) {
            is_new = true;
            obj = table->create_object_with_primary_key(val);
        }
        else if (!try_update) {
            std::ostringstream string_builder;
            string_builder << val;
            throw SetDuplicatePrimaryKeyValueException(object_schema.name,
                primary_key_property.name,
                string_builder.str());
        }
        else {
            obj = table->get_object(obj_key);
            is_new = false;
        }

        return new Object(realm, object_schema, obj);
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

        s_get_native_schema(SchemaForMarshaling {
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

REALM_EXPORT Object* shared_realm_get_object_for_primary_key(SharedRealm& realm, TableKey table_key, realm_value_t primitive, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() -> Object* {
        realm->verify_thread();

        const TableRef table = get_table(realm, table_key);
        const ObjectSchema& object_schema = *realm->schema().find(table_key); 
        if (object_schema.primary_key.empty()) {
            const std::string name(ObjectStore::object_type_for_table_name(table->get_name()));
            throw MissingPrimaryKeyException(name);
        }

        const Property& primary_key_property = *object_schema.primary_key_property();
        if (!primary_key_property.type_is_nullable() && primitive.is_null()) {
            return nullptr;
        }

        if (!primitive.is_null() && to_capi(primary_key_property.type) != primitive.type) {
            throw PropertyTypeMismatchException(object_schema.name, primary_key_property.name, to_string(primary_key_property.type), to_string(primitive.type));
        }

        const ColKey column_key = object_schema.primary_key_property()->column_key;
        const ObjKey obj_key = table->find_first(column_key, from_capi(primitive));
        if (!obj_key) {
            return nullptr;
        }

        return new Object(realm, object_schema, table->get_object(obj_key));
    });
}

REALM_EXPORT Results* shared_realm_create_results(SharedRealm& realm, TableKey table_key, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        realm->verify_thread();

        const TableRef table = get_table(realm, table_key);
        return new Results(realm, table);
    });
}

}
