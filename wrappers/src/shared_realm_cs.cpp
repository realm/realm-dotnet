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
#include "object-store/src/object_store.hpp"
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/schema.hpp"
#include "object-store/src/property.hpp"
#include "object-store/src/object_schema.hpp"
#include "object-store/src/binding_context.hpp"
#include <list>
#include "schema_cs.hpp"
#include "shared_realm_cs.hpp"

using namespace realm;
using namespace realm::binding;

using NotifyRealmChangedT = void(*)(void* managed_realm_handle);
NotifyRealmChangedT notify_realm_changed = nullptr;

namespace realm {
namespace binding {

class CSharpBindingContext: public BindingContext {
public:
    CSharpBindingContext(void* managed_realm_handle) : m_managed_realm_handle(managed_realm_handle) {}

    void did_change(std::vector<ObserverState> const&, std::vector<void*> const&) override
    {
        notify_realm_changed(m_managed_realm_handle);
    }

private:
    void* m_managed_realm_handle;
};

}
}

extern "C" {
    
REALM_EXPORT void register_notify_realm_changed(NotifyRealmChangedT notifier)
{
    notify_realm_changed = notifier;
}

struct Configuration
{
    uint16_t* path;
    size_t path_len;
    
    bool read_only;
    
    bool in_memory;
    
    bool delete_if_migration_needed;

    uint64_t schema_version;
    
     bool (__cdecl *migration_callback)(SharedRealm* old_realm, SharedRealm* new_realm, SchemaForMarshaling, uint64_t schema_version, void* managed_migration_handle);
    void* managed_migration_handle;
};
    
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
            config.schema_mode = SchemaMode::Automatic;

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


REALM_EXPORT void shared_realm_bind_to_managed_realm_handle(SharedRealm* realm, void* managed_realm_handle, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        (*realm)->m_binding_context = std::unique_ptr<realm::BindingContext>(new CSharpBindingContext(managed_realm_handle));
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

REALM_EXPORT uint64_t  shared_realm_get_schema_version(SharedRealm* realm, NativeException::Marshallable& ex)
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

}
