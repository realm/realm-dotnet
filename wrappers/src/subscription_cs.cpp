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

using namespace realm;
using namespace realm::binding;
using namespace realm::partial_sync;

using SharedSubscription = std::shared_ptr<Subscription>;

typedef void (*ManagedSubscriptionCallback)(void* managed_subscription);

struct SubscriptionNotificationTokenContext {
    NotificationToken token;
    void* managed_subscription;
    ManagedSubscriptionCallback callback;
};

extern "C" {

REALM_EXPORT SharedSubscription* realm_subscription_create(Results& results, uint16_t* name_buf, size_t name_len, bool has_name, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        util::Optional<std::string> name;
        if (has_name) {
            name = util::Optional<std::string>(Utf16StringAccessor(name_buf, name_len).to_string());
        }
        else {
            name = util::none;
        }
        
        auto result = realm::partial_sync::subscribe(results, name);
        return new SharedSubscription(&result);
    });
}

REALM_EXPORT Results* realm_subscription_get_results(SharedSubscription& subscription, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return new Results(subscription->results());
    });
}

REALM_EXPORT SubscriptionState realm_subscription_get_state(SharedSubscription& subscription, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&]() {
        return subscription->state();
    });
}

REALM_EXPORT NativeException::Marshallable realm_subscription_get_error(SharedSubscription& subscription)
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

REALM_EXPORT SubscriptionNotificationTokenContext* realm_subscription_add_notification_callback(SharedSubscription& subscription, void* managed_subscription, ManagedSubscriptionCallback callback, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [=]() {
        auto context = new SubscriptionNotificationTokenContext();
        context->managed_subscription = managed_subscription;
        context->callback = callback;
        context->token = subscription->add_notification_callback([context]() {
            context->callback(context->managed_subscription);
        }).registration_token;
        
        return context;
    });
}

REALM_EXPORT void realm_subscription_destroy_notification_token(SubscriptionNotificationTokenContext* token_ptr, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&]() {
        delete token_ptr;
    });
}

}
