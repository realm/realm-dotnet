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

#pragma once

#include "schema_cs.hpp"
#include "sync_session_cs.hpp"

#include <realm/object-store/shared_realm.hpp>
#include <realm/object-store/binding_context.hpp>
#include <realm/object-store/object_accessor.hpp>
#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_session.hpp>
#include <realm/sync/config.hpp>
#include <realm/object-store/sync/app_user.hpp>

namespace realm::binding {
using SharedSyncUser = std::shared_ptr<app::User>;

struct Configuration
{
    realm_string_t path;
    
    realm_string_t fallback_path;

    NativeSchema schema;
    uint64_t schema_version;

    uint64_t max_number_of_active_versions;

    void* managed_config;

    MarshaledVector<uint8_t> encryption_key;

    bool read_only;
    
    bool in_memory;
    
    bool delete_if_migration_needed;
    
    bool enable_cache;

    bool use_legacy_guid_representation;

    bool invoke_should_compact_callback;

    bool invoke_initial_data_callback;

    bool invoke_migration_callback;

    bool automatically_migrate_embedded;

    bool relaxed_schema;
};

struct SyncConfiguration
{
    SharedSyncUser* user;

    uint16_t* partition;
    size_t partition_len;

    SyncSessionStopPolicy session_stop_policy;

    SchemaMode schema_mode;

    bool is_flexible_sync;

    ClientResyncMode client_resync_mode;

    bool cancel_waits_on_nonfatal_error;
};

inline const TableRef get_table(const SharedRealm& realm, TableKey table_key)
{
    return realm->read_group().get_table(table_key);
}
    
extern std::function<void(void*)> s_release_gchandle;

struct GCHandleHolder {
public:
    GCHandleHolder(void* handle)
        : m_handle(handle)
    { }

    GCHandleHolder(const GCHandleHolder&) = delete;
    GCHandleHolder& operator=(const GCHandleHolder&) = delete;

    GCHandleHolder(GCHandleHolder&& other) noexcept
    {
        m_handle = other.m_handle;
        other.m_handle = nullptr;
    }

    ~GCHandleHolder()
    {
        if (m_handle != nullptr) {
            s_release_gchandle(m_handle);
            m_handle = nullptr;
        }
    }

    void* handle() const
    {
        return m_handle;
    }
private:
    void* m_handle;
};

class TcsRegistryWithVersion {
public:
    uint64_t add(DB::version_type version, void* tcs)
    {
        uint64_t token = m_next_token++;
        m_tcs.emplace_hint(m_tcs.end(), token, std::make_pair(version, tcs));
        return token;
    }

    void remove(uint64_t token)
    {
        m_tcs.erase(token);
    }

    std::vector<void*> remove_for_version(DB::version_type version)
    {
        std::vector<uint64_t> tokens;
        std::vector<void*> tcs_vector;

        for (const auto& [token, request] : m_tcs) {
            if (auto& [expected, tcs] = request; expected <= version) {
                tcs_vector.push_back(tcs);
                tokens.push_back(token);
            }
        }
        for (const auto& token : tokens) {
            remove(token);
        }

        return tcs_vector;
    }

private:
    std::map<uint64_t, std::pair<uint64_t, void*>> m_tcs;
    uint64_t m_next_token = 0;
};

class CSharpBindingContext : public BindingContext {
public:
    CSharpBindingContext(GCHandleHolder managed_state_handle);
    void did_change(std::vector<CSharpBindingContext::ObserverState> const& observed, std::vector<void*> const& invalidated, bool version_changed) override;

    void* get_managed_state_handle()
    {
        return m_managed_state_handle.handle();
    }

    TcsRegistryWithVersion& pending_refresh_callbacks()
    {
        return m_pending_refresh_callbacks;
    }

    // TODO: this should go away once https://github.com/realm/realm-core/issues/4584 is resolved
    Schema m_realm_schema;

private:
    GCHandleHolder m_managed_state_handle;
    TcsRegistryWithVersion m_pending_refresh_callbacks;
};

} // namespace realm::binding

