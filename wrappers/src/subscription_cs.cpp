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

#include <future>
#include <realm.hpp>
#include "sync_manager_cs.hpp"
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "sync/partial_sync.hpp"
#include "schema_cs.hpp"
#include <realm/parser/parser.hpp>
#include <realm/parser/query_builder.hpp>
#include "keypath_helpers.hpp"

using namespace realm;
using namespace realm::binding;
using namespace realm::partial_sync;

typedef void (*ManagedSubscriptionCallback)(void* managed_subscription);

struct SubscriptionNotificationTokenContext {
    SubscriptionNotificationToken token;
    void* managed_subscription;
    ManagedSubscriptionCallback callback;
};

extern "C" {

REALM_EXPORT Subscription* realm_subscription_create(Results& results, uint16_t* name_buf, int32_t name_len, int64_t time_to_live, bool update, StringValue* inclusions, int inclusions_len, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto name = name_len >= 0 ? util::Optional<std::string>(Utf16StringAccessor(name_buf, name_len).to_string()) : none;
        auto optional_ttl = time_to_live >= 0 ? util::Optional<int64_t>(time_to_live) : none;
        
        std::vector<StringData> paths;
        for (auto i = 0; i < inclusions_len; i++) {
            paths.emplace_back(inclusions[i].value);
        }

        parser::KeyPathMapping mapping;
        realm::alias_backlinks(mapping, *results.get_realm());

        auto inclusion_paths = realm::generate_include_from_keypaths(paths, *results.get_realm(), results.get_object_schema(), mapping);
        
        realm::partial_sync::SubscriptionOptions options;
        options.user_provided_name = name;
        options.time_to_live_ms = optional_ttl;
        options.update = update;
        options.inclusions = inclusion_paths;

        auto result = realm::partial_sync::subscribe(results, options);
        return new Subscription(std::move(result));
    });
}

REALM_EXPORT SubscriptionState realm_subscription_get_state(Subscription* subscription, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return subscription->state();
    });
}

REALM_EXPORT NativeException::Marshallable realm_subscription_get_error(Subscription* subscription)
{
    if (subscription->error()) {
        try {
            std::rethrow_exception(subscription->error());
        }
        catch (...) {
            return convert_exception().for_marshalling();
        }
    }
    else {
        NativeException no_error = { RealmErrorType::NoError };
        return no_error.for_marshalling();
    }
}

REALM_EXPORT SubscriptionNotificationTokenContext* realm_subscription_add_notification_callback(Subscription* subscription, void* managed_subscription, ManagedSubscriptionCallback callback, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto context = new SubscriptionNotificationTokenContext();
        context->managed_subscription = managed_subscription;
        context->callback = callback;
        context->token = subscription->add_notification_callback([context]() {
            context->callback(context->managed_subscription);
        });

        return context;
    });
}

REALM_EXPORT void* realm_subscription_destroy_notification_token(SubscriptionNotificationTokenContext* context, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        auto managed_subscription = context->managed_subscription;
        delete context;
        return managed_subscription;
    });
}

REALM_EXPORT void realm_subscription_unsubscribe(Subscription* subscription, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        partial_sync::unsubscribe(*subscription);
    });
}

REALM_EXPORT void realm_subscription_destroy(Subscription* subscription)
{
    delete subscription;
}

}
