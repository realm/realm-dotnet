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
#include "notifications_cs.hpp"

#include <realm.hpp>
#include <realm/object-store/object_store.hpp>
#include <realm/object-store/binding_context.hpp>
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/thread_safe_reference.hpp>
#include <realm/object-store/sync/async_open_task.hpp>
#include <realm/object-store/impl/realm_coordinator.hpp>
#include <realm/object-store/sync/app.hpp>

#include <list>
#include <unordered_set>
#include <sstream>

using SharedAsyncOpenTask = std::shared_ptr<AsyncOpenTask>;

using namespace realm;
using namespace realm::binding;

using OpenRealmCallbackT = void(void* task_completion_source, ThreadSafeReference* ref, NativeException::Marshallable ex);
using RealmChangedT = void(void* managed_state_handle);
using GetNativeSchemaT = void(SchemaForMarshaling schema, void* managed_callback);
using OnBindingContextDestructedT = void(void* managed_handle);
using LogMessageT = void(realm_value_t message, util::Logger::Level level);
using MigrationCallbackT = bool(realm::SharedRealm* old_realm, realm::SharedRealm* new_realm, Schema* migration_schema, SchemaForMarshaling, uint64_t schema_version, void* managed_migration_handle);
using ShouldCompactCallbackT = bool(void* managed_config_handle, uint64_t total_size, uint64_t data_size);
namespace realm {
    std::function<ObjectNotificationCallbackT> s_object_notification_callback;
    std::function<DictionaryNotificationCallbackT> s_dictionary_notification_callback;

namespace binding {
    std::function<OpenRealmCallbackT> s_open_realm_callback;
    std::function<RealmChangedT> s_realm_changed;
    std::function<GetNativeSchemaT> s_get_native_schema;
    std::function<OnBindingContextDestructedT> s_on_binding_context_destructed;
    std::function<LogMessageT> s_log_message;
    std::function<MigrationCallbackT> s_on_migration;
    std::function<ShouldCompactCallbackT> s_should_compact;

    std::atomic<bool> s_can_call_managed;

    CSharpBindingContext::CSharpBindingContext(void* managed_state_handle) : m_managed_state_handle(managed_state_handle) {}

    void CSharpBindingContext::did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated, bool version_changed)
    {
        s_realm_changed(m_managed_state_handle);
    }

    CSharpBindingContext::~CSharpBindingContext()
    {
        s_on_binding_context_destructed(m_managed_state_handle);
        m_realm_schema = realm::Schema();
    }

    void log_message(std::string message, util::Logger::Level level)
    {
        s_log_message(to_capi(Mixed(message)), level);
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
    config.schema_mode = sync_configuration.schema_mode;

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
    }

    config.cache = configuration.enable_cache;

    return config;
}
}

extern "C" {

typedef uint32_t realm_table_key;

REALM_EXPORT void shared_realm_install_callbacks(
    RealmChangedT* realm_changed, 
    GetNativeSchemaT* get_schema, 
    OpenRealmCallbackT* open_callback, 
    OnBindingContextDestructedT* on_binding_context_destructed,
    LogMessageT* log_message,
    ObjectNotificationCallbackT* notify_object,
    DictionaryNotificationCallbackT* notify_dictionary,
    MigrationCallbackT* on_migration,
    ShouldCompactCallbackT* should_compact)
{
    s_realm_changed = wrap_managed_callback(realm_changed);
    s_get_native_schema = wrap_managed_callback(get_schema);
    s_open_realm_callback = wrap_managed_callback(open_callback);
    s_on_binding_context_destructed = wrap_managed_callback(on_binding_context_destructed);
    s_log_message = wrap_managed_callback(log_message);
    realm::s_object_notification_callback = wrap_managed_callback(notify_object);
    realm::s_dictionary_notification_callback = wrap_managed_callback(notify_dictionary);
    s_on_migration = wrap_managed_callback(on_migration);
    s_should_compact = wrap_managed_callback(should_compact);

    realm::binding::s_can_call_managed = true;
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
            config.migration_function = [&configuration](SharedRealm oldRealm, SharedRealm newRealm, Schema migrationSchema) {

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

                if (!s_on_migration(&oldRealm, &newRealm, &migrationSchema, schema_for_marshaling, oldRealm->schema_version(), configuration.managed_migration_handle)) {
                    throw ManagedExceptionDuringMigration();
                }
            };
        }

        if (configuration.managed_should_compact_delegate) {
            config.should_compact_on_launch_function = [&configuration](uint64_t total_bytes, uint64_t used_bytes) {
                return s_should_compact(configuration.managed_should_compact_delegate, total_bytes, used_bytes);
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
                auto native_ex = realm::convert_exception(error).for_marshalling();
                s_open_realm_callback(task_completion_source, nullptr, std::move(native_ex));
            }
            else {
                s_open_realm_callback(task_completion_source, new ThreadSafeReference(std::move(ref)), { RealmErrorType::NoError });
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

REALM_EXPORT void shared_realm_delete_files(uint16_t* path_buf, size_t path_len, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        Utf16StringAccessor path_string(path_buf, path_len);
        Realm::delete_files(path_string);
    });
}

REALM_EXPORT void shared_realm_close_all_realms(NativeException::Marshallable& ex)
{
    s_can_call_managed = false;

    handle_errors(ex, [&]() {
        realm::_impl::RealmCoordinator::clear_all_caches();
        app::App::clear_cached_apps();
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

REALM_EXPORT void shared_realm_rename_property(const SharedRealm& realm, uint16_t* type_name_buf, size_t type_name_len,
    uint16_t* old_name_buf, size_t old_Name_len, uint16_t* new_name_buf, size_t new_name_len, Schema* migration_schema, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor type_name_str(type_name_buf, type_name_len);
        Utf16StringAccessor old_name_str(old_name_buf, old_Name_len);
        Utf16StringAccessor new_name_str(new_name_buf, new_name_len);

        ObjectStore::rename_property(realm->read_group(), *migration_schema, type_name_str, old_name_str, new_name_str);
    });
}

REALM_EXPORT bool shared_realm_remove_type(const SharedRealm& realm, uint16_t* type_name_buf, size_t type_name_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        Utf16StringAccessor type_name_str(type_name_buf, type_name_len);

        auto table = ObjectStore::table_for_object_type(realm->read_group(), type_name_str);
        if (!table)
        {
            return false;
        }

        const auto obj_schema = realm->schema().find(type_name_str);
        if (obj_schema != realm->schema().end())
        {
            throw RemoveTypeInSchemaException();
        }

        realm->read_group().remove_table(table->get_key());
        return true;
    });
}

}
