////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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

#include "realm_export_decls.hpp"
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "notifications_cs.hpp"
#include "sync_manager_cs.hpp"
#include "sync_session_cs.hpp"

#include <server/global_notifier.hpp>
#include <sync/sync_manager.hpp>

using namespace realm;
using namespace realm::binding;
using NotifierHandle = std::shared_ptr<GlobalNotifier>;

struct MarshaledModificationInfo {
    ObjKey obj;
    MarshaledVector<ColKey> changed_columns;
};

struct MarshaledChangeSet {
    StringData class_name = {};

    MarshaledVector<ObjKey> deletions;
    MarshaledVector<ObjKey> insertions;
    MarshaledVector<MarshaledModificationInfo> modifications;
};

struct MarshaledChangeNotification {
    StringData path = {};

    StringData path_on_disk = {};

    SharedRealm* previous = nullptr;
    SharedRealm* current = nullptr;

    MarshaledVector<MarshaledChangeSet> changesets;
};

bool (*s_should_handle_callback)(const void* managed_instance, const char* path, size_t path_len);
void (*s_enqueue_calculation_callback)(const void* managed_instance, const char* path, size_t path_len, GlobalNotifier::ChangeNotification*);
void (*s_start_callback)(const void* managed_instance, int32_t error_code, const char* message, size_t message_len);
void (*s_calculation_complete_callback)(MarshaledChangeNotification& change, const void* managed_callback);

class Callback : public GlobalNotifier::Callback {
public:
    Callback(void* managed_instance)
    : m_managed_instance(managed_instance)
    , m_logger(SyncManager::shared().make_logger())
    { }

    virtual void download_complete() {
        m_did_download = true;
        m_logger->trace("ManagedGlobalNotifier: download_complete()");
        s_start_callback(m_managed_instance, 0, nullptr, 0);
    }

    virtual void error(std::exception_ptr error) {
        m_logger->trace("ManagedGlobalNotifier: error()");
        if (!m_did_download) {
            try {
                std::rethrow_exception(error);
            } catch (const std::system_error& system_error) {
                const std::error_code& ec = system_error.code();
                s_start_callback(m_managed_instance, ec.value(), ec.message().c_str(), ec.message().length());
            } catch (const std::exception& e) {
                m_logger->fatal("ManagedGlobalNotifier fatal error: %1", e.what());
                realm::util::terminate("Unhandled GlobalNotifier exception type", __FILE__, __LINE__);
            }
        } else {
            realm::util::terminate("Unhandled GlobalNotifier runtime error", __FILE__, __LINE__);
        }
    }

    virtual bool realm_available(StringData, StringData virtual_path) {
        m_logger->trace("ManagedGlobalNotifier: realm_available(%1)", virtual_path);
        return s_should_handle_callback(m_managed_instance, virtual_path.data(), virtual_path.size());
    }

    virtual void realm_changed(GlobalNotifier* notifier) {
        m_logger->trace("ManagedGlobalNotifier: realm_changed()");
        while (auto change = notifier->next_changed_realm()) {
            s_enqueue_calculation_callback(m_managed_instance, change->realm_path.c_str(), change->realm_path.size(), new GlobalNotifier::ChangeNotification(std::move(change.value())));
        }
    }
private:
    const void* m_managed_instance;
    const std::unique_ptr<util::Logger> m_logger;
    bool m_did_download = false;
};

extern "C" {
REALM_EXPORT void realm_server_install_callbacks(decltype(s_should_handle_callback) should_handle_callback,
                                                 decltype(s_enqueue_calculation_callback) enqueue_calculation_callback,
                                                 decltype(s_start_callback) start_callback,
                                                 decltype(s_calculation_complete_callback) calculation_complete_callback)
{
    s_should_handle_callback = should_handle_callback;
    s_enqueue_calculation_callback = enqueue_calculation_callback;
    s_start_callback = start_callback;
    s_calculation_complete_callback = calculation_complete_callback;
}

REALM_EXPORT NotifierHandle* realm_server_create_global_notifier(void* managed_instance,
                                                                 SyncConfiguration configuration,
                                                                 uint8_t* encryption_key,
                                                                 NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        Utf16StringAccessor realm_url(configuration.url, configuration.url_len);
        SyncConfig config(*configuration.user, std::move(realm_url));

        config.bind_session_handler = bind_session;
        config.error_handler = handle_session_error;
        config.stop_policy = SyncSessionStopPolicy::Immediately;

        config.client_validate_ssl = configuration.client_validate_ssl;
        config.ssl_trust_certificate_path = std::string(Utf16StringAccessor(configuration.trusted_ca_path, configuration.trusted_ca_path_len));

        // the partial_sync_identifier field was hijacked to carry the working directory
        Utf16StringAccessor working_dir(configuration.partial_sync_identifier, configuration.partial_sync_identifier_len);

        auto callback = std::make_unique<Callback>(managed_instance);
        auto notifier = std::make_shared<GlobalNotifier>(std::move(callback), std::move(working_dir), std::move(config));
        notifier->start();
        return new NotifierHandle(std::move(notifier));
    });
}

REALM_EXPORT SharedRealm* realm_server_global_notifier_get_realm_for_writing(SharedRealm& current_realm,
                                                                             NativeException::Marshallable& ex)
{
    return handle_errors(ex, [current_realm] {
        return new SharedRealm(Realm::get_shared_realm(current_realm->config()));
    });
}

REALM_EXPORT void realm_server_global_notifier_destroy(NotifierHandle* notifier)
{
    delete notifier;
}

REALM_EXPORT void realm_server_global_notifier_notification_get_changes(GlobalNotifier::ChangeNotification& change,
                                                                     void* managed_callback,
                                                                     NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        MarshaledChangeNotification notification;

        auto changes = change.get_changes();
        std::vector<MarshaledChangeSet> changesets;
        changesets.reserve(changes.size());

        std::vector<std::vector<ObjKey>> objkeys_storage;
        std::vector<std::vector<MarshaledModificationInfo>> modifications_storage;
        std::vector<std::vector<ColKey>> columns_storage;
        objkeys_storage.reserve(changes.size() * 2);
        modifications_storage.reserve(changes.size());
        columns_storage.reserve(changes.size());

        for (auto& changeset : changes) {
            MarshaledChangeSet c;
            c.class_name = changeset.first;

            auto get_objkeys = [](const ObjectChangeSet::ObjectSet& set) {
                return std::vector<ObjKey>(set.begin(), set.end());
            };

            auto get_colkeys = [](const ObjectChangeSet::ObjectMapToColumnSet::mapped_type& set) {
                return std::vector<ColKey>(set.begin(), set.end());
            };

            objkeys_storage.push_back(get_objkeys(changeset.second.get_deletions()));
            c.deletions = objkeys_storage.back();

            objkeys_storage.push_back(get_objkeys(changeset.second.get_insertions()));
            c.insertions = objkeys_storage.back();

            modifications_storage.emplace_back();
            for (auto& modification : changeset.second.get_modifications()) {
                columns_storage.push_back(get_colkeys(modification.second));
                modifications_storage.back().push_back({ ObjKey(modification.first), columns_storage.back() });
            }
            c.modifications = modifications_storage.back();

            changesets.push_back(std::move(c));
        }

        notification.changesets = changesets;

        notification.path = change.realm_path;

        if (auto previous = change.get_old_realm()) {
            notification.previous = new SharedRealm(std::move(previous));
        }
        auto newRealm = change.get_new_realm();

        notification.path_on_disk = newRealm->config().path;

        notification.current = new SharedRealm(std::move(newRealm));

        s_calculation_complete_callback(notification, managed_callback);
    });
}

REALM_EXPORT void realm_server_global_notifier_notification_destroy(GlobalNotifier::ChangeNotification* notification)
{
    delete notification;
}

}
