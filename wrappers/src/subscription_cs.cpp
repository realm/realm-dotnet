//
//  subscription_cs.cpp
//  wrappers-sync
//
//  Created by Nikola Irinchev on 3/23/18.
//  Copyright © 2018 Realm. All rights reserved.
//

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
        
        IncludeDescriptor inclusion_paths;
        if (inclusions_len >= 0) {
            DescriptorOrdering combined_orderings;
            parser::KeyPathMapping mapping;
            alias_backlinks(mapping, results.get_realm());

            for (auto i = 0; i < inclusions_len; i++) {
                auto inclusion_path = inclusions[i].value;
                DescriptorOrdering ordering;
                parser::DescriptorOrderingState ordering_state = parser::parse_include_path(inclusion_path);
                query_builder::apply_ordering(ordering, results.get_query().get_table(), ordering_state, mapping);
                combined_orderings.append_include(ordering.compile_included_backlinks());
            }
            
            if (combined_orderings.will_apply_include()) {
                inclusion_paths = combined_orderings.compile_included_backlinks();
            }
        }
        
        realm::partial_sync::SubscriptionOptions options {
            name,
            optional_ttl,
            update,
            inclusion_paths
        };
        
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
