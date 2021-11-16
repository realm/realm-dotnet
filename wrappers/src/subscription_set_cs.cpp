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

struct CSharpSubscription {
    realm_value_t name = realm_value_t{};

    realm_value_t object_type = realm_value_t{};

    realm_value_t query = realm_value_t{};

    bool has_value = false;
};

using StateWaitCallbackT = void(void* task_completion_source, SubscriptionSet::State state, realm_value_t message);
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
                auto csharp_sub = CSharpSubscription{
                    to_capi_value(sub.value().name()),
                    to_capi_value(sub.value().object_class_name()),
                    to_capi_value(sub.value().query_string()),
                    true
                };
                s_get_subscription_callback(callback, csharp_sub);
            }
            else {
                s_get_subscription_callback(callback, CSharpSubscription{});
            }
        });
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

REALM_EXPORT size_t realm_subscriptionset_get_count(SubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return subs.size();
    });
}

REALM_EXPORT SubscriptionSet::State realm_subscriptionset_get_state(SubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return subs.state();
    });
}

REALM_EXPORT void realm_subscriptionset_get_at_index(SubscriptionSet& subs, size_t index, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&] {
        const size_t count = subs.size();
        if (index >= count)
            throw IndexOutOfRangeException("Get from SubscriptionSet", index, count);

        return subs.get_at_index(index);
    });
}

REALM_EXPORT void realm_subscriptionset_find_by_name(SubscriptionSet& subs, uint16_t* name_buf, size_t name_len, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        Utf16StringAccessor name(name_buf, name_len);
        auto it = subs.find(name);
        if (it == subs.end()) {
            return util::none;
        }

        return *it;
    });
}

REALM_EXPORT void realm_subscriptionset_find_by_query(SubscriptionSet& subs, Results& results, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        auto it = subs.find(results.get_query());
        if (it == subs.end()) {
            return util::none;
        }

        return *it;
    });
}

REALM_EXPORT void realm_subscriptionset_add(SubscriptionSet& subs,
    uint16_t* type_buf, size_t type_len, 
    uint16_t* query_buf, size_t query_len,
    realm_value_t* arguments, size_t args_count,
    uint16_t* name_buf, size_t name_len, 
    bool update_existing, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        // TODO: implement me
        return util::none;
    });
}

REALM_EXPORT void realm_subscriptionset_add_results(SubscriptionSet& subs,
    Results& results,
    uint16_t* name_buf, size_t name_len,
    bool update_existing, void* callback, NativeException::Marshallable& ex)
{
    get_subscription(callback, ex, [&]() -> util::Optional<Subscription> {
        auto object_type = results.get_object_type();
        auto query_str = results.get_query().get_description();

        if (name_buf) {
            Utf16StringAccessor name(name_buf, name_len);
            auto it = subs.find(name);
            if (it == subs.end() || update_existing || (it->object_class_name() == object_type && it->query_string() == query_str)) {
                return *subs.insert_or_assign(name.to_string(), results.get_query()).first;
            }
            else {
                // TODO: proper error
                throw std::runtime_error("bla bla");
            }
        }
        else {
            return *subs.insert_or_assign(results.get_query()).first;
        }
    });
}

REALM_EXPORT bool realm_subscriptionset_remove(SubscriptionSet& subs, uint16_t* name_buf, size_t name_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        Utf16StringAccessor name(name_buf, name_len);
        if (auto it = subs.find(name); it != subs.end()) {
            subs.erase(it);
            return true;
        }
        
        return false;
    });
}

REALM_EXPORT size_t realm_subscriptionset_remove_by_type(SubscriptionSet& subs, uint16_t* type_buf, size_t type_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        size_t removed = 0;
        Utf16StringAccessor type(type_buf, type_len);

        for (auto it = subs.begin(); it != subs.end(); it++) {
            if (it->object_class_name() == type) {
                it = subs.erase(it);
                removed++;
            }
        }

        return removed;
    });
}

REALM_EXPORT size_t realm_subscriptionset_remove_all(SubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        auto size = subs.size();
        for (auto it = subs.begin(); it != subs.end(); it++) {
            it = subs.erase(it);
        }

        return size;
    });
}

REALM_EXPORT void realm_subscriptionset_destroy(SubscriptionSet* subs)
{
    delete subs;
}

REALM_EXPORT SubscriptionSet* realm_subscriptionset_begin_write(SubscriptionSet& subs, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return new SubscriptionSet(subs.make_mutable_copy());
    });
}

REALM_EXPORT void realm_subscriptionset_cancel_write(SubscriptionSet& subs, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        // TODO this should be cancel
        subs.commit();
    });
}

REALM_EXPORT void realm_subscriptionset_commit_write(SubscriptionSet& subs, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        subs.commit();
    });
}

REALM_EXPORT size_t realm_subscriptionset_get_error(SubscriptionSet& subs, uint16_t* buffer, size_t buffer_length, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return stringdata_to_csharpstringbuffer(subs.error_str(), buffer, buffer_length);
    });
}

REALM_EXPORT void realm_subscriptionset_wait_for_state(const SubscriptionSet* subs, void* task_completion_source, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        subs->get_state_change_notification(SubscriptionSet::State::Complete)
            .get_async([task_completion_source](StatusWith<SubscriptionSet::State> status) mutable noexcept {
                if (status.is_ok()) {
                    s_state_wait_callback(task_completion_source, status.get_value(), realm_value_t{});
                }
                else {
                    s_state_wait_callback(task_completion_source, SubscriptionSet::State::Error, to_capi_value(status.get_status().reason()));
                }
            });
    });
}

}

