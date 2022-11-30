////////////////////////////////////////////////////////////////////////////
//
// Copyright 2021 Realm Inc.
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
#include <realm/util/uri.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include <realm/sync/protocol.hpp>
#include <realm/object-store/sync/sync_manager.hpp>
#include <realm/object-store/sync/sync_session.hpp>
#include "sync_session_cs.hpp"
#include <realm/sync/subscriptions.hpp>

using namespace realm;
using namespace realm::binding;
using namespace realm::sync;

using SharedSubscriptionSet = std::shared_ptr<SubscriptionSet>;
using SharedMutableSubscriptionSet = std::shared_ptr<MutableSubscriptionSet>;

struct CSharpSubscription {
    realm_value_t id = realm_value_t{};

    realm_value_t name = realm_value_t{};

    realm_value_t object_type = realm_value_t{};

    realm_value_t query = realm_value_t{};

    realm_value_t created_at = realm_value_t{};

    realm_value_t updated_at = realm_value_t{};

    bool has_value = false;
};

enum class CSharpState : uint8_t {
    Pending = 0,
    Complete,
    Error,
    Superseded
};

using StateWaitCallbackT = void(void* task_completion_source, CSharpState state, realm_value_t message);
using SubscriptionCallbackT = void(void* managed_callback, CSharpSubscription sub);

namespace realm {
namespace binding {
    std::function<StateWaitCallbackT> s_state_wait_callback;
    std::function<SubscriptionCallbackT> s_get_subscription_callback;

    inline void get_subscription(void* callback, NativeException::Marshallable& ex, const std::function<util::Optional<Subscription>()>& lambda)
    {
        handle_errors(ex, [&] {
            auto sub = lambda();
            if (sub) {
                auto sub_value = *sub;
                auto csharp_sub = CSharpSubscription{
                    to_capi_value(sub_value.id()),
                    to_capi_value(sub_value.name()),
                    to_capi_value(sub_value.object_class_name()),
                    to_capi_value(sub_value.query_string()),
                    to_capi_value(sub_value.created_at()),
                    to_capi_value(sub_value.updated_at()),
                    true
                };
                s_get_subscription_callback(callback, csharp_sub);
            }
            else {
                s_get_subscription_callback(callback, CSharpSubscription{});
            }
        });
    }

    inline CSharpState core_to_csharp_state(SubscriptionSet::State state) {
        switch (state) {
        case SubscriptionSet::State::Uncommitted:
        case SubscriptionSet::State::Pending:
        case SubscriptionSet::State::Bootstrapping:
            return CSharpState::Pending;
        case SubscriptionSet::State::Complete:
            return CSharpState::Complete;
        case SubscriptionSet::State::Error:
            return CSharpState::Error;
        case SubscriptionSet::State::Superseded:
            return CSharpState::Superseded;
        default:
            REALM_UNREACHABLE();
        }
    }
}
}
extern "C" {

REALM_EXPORT void realm_subscriptionset_install_callbacks(SubscriptionCallbackT* get_subscription_callback, StateWaitCallbackT* state_wait_callback)
{
    s_get_subscription_callback = wrap_managed_callback(get_subscription_callback);
    s_state_wait_callback = wrap_managed_callback(state_wait_callback);
    realm::binding::s_can_call_managed = true;
}

REALM_EXPORT size_t realm_subscriptionset_get_count(SharedSubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return subs->size();
    });
}



REALM_EXPORT CSharpState realm_subscriptionset_get_state(SharedSubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return core_to_csharp_state(subs->state());
    });
}

REALM_EXPORT int64_t realm_subscriptionset_get_version(SharedSubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return subs->version();
    });
}


REALM_EXPORT void realm_subscriptionset_get_at_index(SharedSubscriptionSet& subs, size_t index, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&] {
        const size_t count = subs->size();
        if (index >= count)
            throw IndexOutOfRangeException("Get from SubscriptionSet", index, count);

        return subs->at(index);
    });
}

REALM_EXPORT void realm_subscriptionset_find_by_name(SharedSubscriptionSet& subs, uint16_t* name_buf, size_t name_len, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        Utf16StringAccessor name(name_buf, name_len);
        auto it = subs->find(name);
        if (it == subs->end()) {
            return util::none;
        }

        return *it;
    });
}

REALM_EXPORT void realm_subscriptionset_find_by_query(SharedSubscriptionSet& subs, Results& results, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        auto it = subs->find(results.get_query());
        if (it == subs->end()) {
            return util::none;
        }

        return *it;
    });
}

REALM_EXPORT void realm_subscriptionset_add_results(SharedMutableSubscriptionSet& subs,
    Results& results,
    uint16_t* name_buf, size_t name_len,
    bool update_existing, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        auto object_type = results.get_object_type();
        auto query_str = results.get_query().get_description();

        if (name_buf) {
            Utf16StringAccessor name(name_buf, name_len);
            auto it = subs->find(name);
            if (it == subs->end() || update_existing || (it->object_class_name() == object_type && it->query_string() == query_str)) {
                return *subs->insert_or_assign(name.to_string(), results.get_query()).first;
            }
            else {
                throw DuplicateSubscriptionException(name.to_string(), std::string(it->query_string()), query_str);
            }
        }
        else {
            return *subs->insert_or_assign(results.get_query()).first;
        }
    });
}

REALM_EXPORT bool realm_subscriptionset_remove(SharedMutableSubscriptionSet& subs, uint16_t* name_buf, size_t name_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        Utf16StringAccessor name(name_buf, name_len);
        if (auto it = subs->find(name); it != subs->end()) {
            subs->erase(it);
            return true;
        }
        
        return false;
    });
}

REALM_EXPORT bool realm_subscriptionset_remove_by_id(SharedMutableSubscriptionSet& subs, realm_value_t id, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto subId = from_capi(id.object_id);
        for (auto it = subs->begin(); it != subs->end(); it++) {
            if (it->id() == subId) {
                subs->erase(it);
                return true;
            }
        }

        return false;
    });
}

REALM_EXPORT size_t realm_subscriptionset_remove_by_query(SharedMutableSubscriptionSet& subs, Results& results, bool remove_named, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        size_t removed = 0;

        const auto query_desc = results.get_query().get_description();
        const auto class_name = results.get_object_type();
        
        for (auto it = subs->begin(); it != subs->end();) {
            if (it->object_class_name() == class_name && it->query_string() == query_desc && (remove_named || it->name().empty())) {
                it = subs->erase(it);
                removed++;
            }
            else {
                it++;
            }
        }

        return removed;
    });
}

REALM_EXPORT size_t realm_subscriptionset_remove_by_type(SharedMutableSubscriptionSet& subs, uint16_t* type_buf, size_t type_len, bool remove_named, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        size_t removed = 0;
        Utf16StringAccessor type(type_buf, type_len);

        for (auto it = subs->begin(); it != subs->end();) {
            if (it->object_class_name() == type && (remove_named || it->name().empty())) {
                it = subs->erase(it);
                removed++;
            }
            else {
                it++;
            }
        }

        return removed;
    });
}

REALM_EXPORT size_t realm_subscriptionset_remove_all(SharedMutableSubscriptionSet& subs, bool remove_named, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        if (remove_named) {
            auto size = subs->size();
            subs->clear();
            return size;
        }

        size_t removed = 0;
        for (auto it = subs->begin(); it != subs->end();) {
            if (it->name().empty()) {
                it = subs->erase(it);
                removed++;
            }
            else {
                it++;
            }
        }

        return removed;
    });
}

REALM_EXPORT void realm_subscriptionset_destroy(SharedSubscriptionSet* subs)
{
    delete subs;
}

REALM_EXPORT void realm_subscriptionset_destroy_mutable(SharedMutableSubscriptionSet* subs)
{
    delete subs;
}

REALM_EXPORT SharedMutableSubscriptionSet* realm_subscriptionset_begin_write(SharedSubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto p = new MutableSubscriptionSet(subs->make_mutable_copy());
        return new SharedMutableSubscriptionSet(p);
    });
}

REALM_EXPORT SharedSubscriptionSet* realm_subscriptionset_commit_write(SharedMutableSubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto p = new SubscriptionSet(std::move(*subs).commit());
        return new SharedSubscriptionSet(p);
    });
}

REALM_EXPORT size_t realm_subscriptionset_get_error(SharedSubscriptionSet& subs, uint16_t* buffer, size_t buffer_length, bool& is_null, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        is_null = subs->error_str().is_null();
        if (is_null) {
            return (size_t)0;
        }

        return stringdata_to_csharpstringbuffer(subs->error_str(), buffer, buffer_length);
    });
}

REALM_EXPORT void realm_subscriptionset_wait_for_state(SharedSubscriptionSet& subs, void* task_completion_source, NativeException::Marshallable& ex)
{
    using WeakSubscriptionSet = std::weak_ptr<SubscriptionSet>;
    handle_errors(ex, [&] {
        subs->get_state_change_notification(SubscriptionSet::State::Complete)
            .get_async([task_completion_source, weak_subs=WeakSubscriptionSet(subs)](StatusWith<SubscriptionSet::State> status) mutable noexcept {
                try {
                    if (auto subs = weak_subs.lock()) {
                        subs->refresh();
                        if (status.is_ok()) {
                            s_state_wait_callback(task_completion_source, core_to_csharp_state(status.get_value()), realm_value_t{});
                        }
                        else {
                            s_state_wait_callback(task_completion_source, CSharpState::Error, to_capi_value(status.get_status().reason()));
                        }
                    }
                    else {
                        s_state_wait_callback(task_completion_source, CSharpState::Error, to_capi(-1));
                    }
                }
                catch (...) {
                    auto inner_ex = convert_exception();
                    s_state_wait_callback(task_completion_source, CSharpState::Error, to_capi_value(inner_ex.to_string()));
                }
            });
    });
}

}

