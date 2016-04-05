/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
#include <realm.hpp>
#include <realm/lang_bind_helper.hpp>
#include "error_handling.hpp"
#include "realm_export_decls.hpp"
#include "marshalling.hpp"
#include "object-store/src/shared_realm.hpp"
#include "object-store/src/schema.hpp"
#include "object-store/src/binding_context.hpp"
#include <list>

#ifdef REALM_PLATFORM_ANDROID
#include "object-store/src/impl/android/weak_realm_notifier.hpp"
#endif


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

REALM_EXPORT SharedRealm* shared_realm_open(Schema* schema, uint16_t* path, size_t path_len, bool read_only, SharedGroup::DurabilityLevel durability,
                        uint8_t* encryption_key, uint64_t schemaVersion)
{
    return handle_errors([&]() {
        Utf16StringAccessor pathStr(path, path_len);

        Realm::Config config;
        config.path = pathStr.to_string();
        config.read_only = read_only;
        config.in_memory = durability != SharedGroup::durability_Full;

        // by definition the key is only allowwed to be 64 bytes long, enforced by C# code
        if (encryption_key == nullptr)
          config.encryption_key = std::vector<char>();
        else
          config.encryption_key = std::vector<char>(encryption_key, encryption_key+64);

        config.schema.reset(schema);
        config.schema_version = schemaVersion;

        return new SharedRealm{Realm::get_shared_realm(config)};
    });
}


REALM_EXPORT void shared_realm_bind_to_managed_realm_handle(SharedRealm* realm, void* managed_realm_handle)
{
    handle_errors([&]() {
        (*realm)->m_binding_context = std::unique_ptr<realm::BindingContext>(new CSharpBindingContext(managed_realm_handle));
    });
}

REALM_EXPORT void shared_realm_destroy(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->close();
        delete realm;
    });
}

REALM_EXPORT size_t shared_realm_has_table(SharedRealm* realm, uint16_t* table_name, size_t table_name_len)
{
    return handle_errors([&]() {
        Group* g = (*realm)->read_group();
        Utf16StringAccessor str(table_name, table_name_len);

        return bool_to_size_t(g->has_table(str));
    });
}

REALM_EXPORT Table* shared_realm_get_table(SharedRealm* realm, uint16_t* table_name, size_t table_name_len)
{
    return handle_errors([&]() {
      Group* g = (*realm)->read_group();
      Utf16StringAccessor str(table_name, table_name_len);

      bool dummy; // get_or_add_table sets this to true if the table was added.
      return LangBindHelper::get_or_add_table(*g, str, &dummy);
    });
}

REALM_EXPORT uint64_t  shared_realm_get_schema_version(SharedRealm* realm)
{
    return handle_errors([&]() {
      return (*realm)->config().schema_version;
    });
}

REALM_EXPORT void shared_realm_begin_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->begin_transaction();
    });
}

REALM_EXPORT void shared_realm_commit_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->commit_transaction();
    });
}

REALM_EXPORT void shared_realm_cancel_transaction(SharedRealm* realm)
{
    handle_errors([&]() {
        (*realm)->cancel_transaction();
    });
}

REALM_EXPORT size_t shared_realm_is_in_transaction(SharedRealm* realm)
{
    return handle_errors([&]() {
        return bool_to_size_t((*realm)->is_in_transaction());
    });
}


REALM_EXPORT size_t shared_realm_is_same_instance(SharedRealm* lhs, SharedRealm* rhs)
{
    return handle_errors([&]() {
        return *lhs == *rhs;  // just compare raw pointers inside the smart pointers
    });
}

REALM_EXPORT size_t shared_realm_refresh(SharedRealm* realm)
{
    return handle_errors([&]() {
        return bool_to_size_t((*realm)->refresh());
    });
}

#ifdef REALM_PLATFORM_ANDROID

REALM_EXPORT void bind_handler_functions(realm::_impl::create_handler_function create_function, 
    realm::_impl::notify_handler_function notify_function,
    realm::_impl::destroy_handler_function destroy_function) 
{
    handle_errors([&]() {
        realm::_impl::create_handler_for_current_thread = create_function;
        realm::_impl::notify_handler = notify_function;
        realm::_impl::destroy_handler = destroy_function;
    });
}

REALM_EXPORT void notify_realm(Realm* realm)
{
    handle_errors([&]() {
        realm->notify();
    });
}

#endif

}
