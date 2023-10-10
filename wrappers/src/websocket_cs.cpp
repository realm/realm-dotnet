////////////////////////////////////////////////////////////////////////////
//
// Copyright 2023 Realm Inc.
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

#include <functional>

#include <realm/sync/network/websocket.hpp>

#include "websocket_cs.hpp"
#include "realm_export_decls.hpp"
#include "marshalling.hpp"

namespace realm::binding {
using namespace realm::sync;

struct MarshaledEndpoint {
    realm_string_t address;
    uint16_t port;
    realm_string_t path;
    MarshaledVector<realm_string_t> protocols;
    bool is_ssl;
};

using CreateTimerT = void*(void* managed_provider, int64_t delay_ms, SyncSocketProvider::FunctionHandler* callback);
using CancelTimerT = void(void* managed_timer);
using PostWorkT = void(void* managed_provider, SyncSocketProvider::FunctionHandler* callback);
using WebSocketConnectT = void*(void* managed_provider, WebSocketObserver* observer, MarshaledEndpoint endpoint);
using WebSocketWriteT = void(void* managed_websocket, realm_binary_t data, SyncSocketProvider::FunctionHandler* callback);
using WebSocketCloseT = void(void* managed_websocket);
using SyncProviderDisposeT = void(void* managed_provider);

std::function<CreateTimerT> s_create_timer;
std::function<CancelTimerT> s_cancel_timer;
std::function<PostWorkT> s_post_work;
std::function<WebSocketConnectT> s_websocket_connect;
std::function<WebSocketWriteT> s_websocket_write;
std::function<WebSocketCloseT> s_websocket_close;
std::function<SyncProviderDisposeT> s_provider_dispose;

struct Timer final : SyncSocketProvider::Timer {
    using LongMiliseconds = std::chrono::duration<int64_t, std::chrono::milliseconds::period>;

public:
    Timer(std::chrono::milliseconds delay, SyncSocketProvider::FunctionHandler&& handler, void* managed_provider)
        : m_managed_timer(s_create_timer(managed_provider, LongMiliseconds(delay).count(), new SyncSocketProvider::FunctionHandler(std::move(handler))))
    {
    }
    ~Timer() final
    {
        cancel();
    }
    void cancel() final
    {
        if (m_managed_timer) {
            s_cancel_timer(m_managed_timer);
            m_managed_timer = nullptr;
        }
    }

private:
    void* m_managed_timer;
};

struct WebSocket final : WebSocketInterface {
public:
    WebSocket(std::unique_ptr<WebSocketObserver> observer, WebSocketEndpoint&& endpoint, void* managed_provider)
    : m_observer(std::move(observer))
    {
        MarshaledEndpoint marshaled_endpoint;
        marshaled_endpoint.address = to_capi(endpoint.address);
        marshaled_endpoint.port = endpoint.port;
        marshaled_endpoint.path = to_capi(endpoint.path);
        marshaled_endpoint.is_ssl = endpoint.is_ssl;

        std::vector<realm_string_t> protocols;
        protocols.reserve(endpoint.protocols.size());
        for (const auto& protocol : endpoint.protocols) {
            protocols.push_back(to_capi(protocol));
        }
        marshaled_endpoint.protocols = protocols;

        m_managed_websocket = s_websocket_connect(managed_provider, m_observer.get(), marshaled_endpoint);
    }

    std::string_view get_appservices_request_id() const noexcept final {
        return {};
    }

    void async_write_binary(util::Span<const char> data, SyncSocketProvider::FunctionHandler&& handler) final {
        realm_binary_t binary;
        binary.data = reinterpret_cast<const uint8_t*>(data.data());
        binary.size = data.size();

        s_websocket_write(m_managed_websocket, binary, new SyncSocketProvider::FunctionHandler(std::move(handler)));
    }

    ~WebSocket() {
        s_websocket_close(m_managed_websocket);
    }

private:
    void* m_managed_websocket;
    std::unique_ptr<WebSocketObserver> m_observer;
};

class SocketProvider final : public SyncSocketProvider {
public:
    SocketProvider(void* managed_provider)
    : m_managed_provider(managed_provider)
    {}

    ~SocketProvider() {
        s_provider_dispose(m_managed_provider);
    }

    SyncTimer create_timer(std::chrono::milliseconds delay, FunctionHandler&& handler) final {
        return std::make_unique<realm::binding::Timer>(delay, std::move(handler), m_managed_provider);
    }

    void post(FunctionHandler&& handler) final {
        s_post_work(m_managed_provider, new FunctionHandler(std::move(handler)));
    }

    std::unique_ptr<WebSocketInterface> connect(std::unique_ptr<WebSocketObserver> observer, WebSocketEndpoint&& endpoint) final {
        return std::make_unique<realm::binding::WebSocket>(std::move(observer), std::move(endpoint), m_managed_provider);
    }

private:
    void* m_managed_provider;
};

std::shared_ptr<SyncSocketProvider> make_websocket_provider(void* managed_provider) { return std::make_shared<SocketProvider>(managed_provider); }

extern "C" {
    REALM_EXPORT void realm_websocket_install_callbacks(PostWorkT* post_work, SyncProviderDisposeT* provider_dispose, CreateTimerT* create_timer, CancelTimerT* cancel_timer,
                                                        WebSocketConnectT* websocket_connect, WebSocketWriteT* websocket_write, WebSocketCloseT* websocket_close) {
        s_post_work = wrap_managed_callback(post_work);
        s_provider_dispose = wrap_managed_callback(provider_dispose);
        s_create_timer = wrap_managed_callback(create_timer);
        s_cancel_timer = wrap_managed_callback(cancel_timer);
        s_websocket_connect = wrap_managed_callback(websocket_connect);
        s_websocket_write = wrap_managed_callback(websocket_write);
        s_websocket_close = wrap_managed_callback(websocket_close);

        realm::binding::s_can_call_managed = true;
    }

    REALM_EXPORT void realm_websocket_run_callback(SyncSocketProvider::FunctionHandler* handler, ErrorCodes::Error error_code, realm_string_t reason) {
        std::unique_ptr<SyncSocketProvider::FunctionHandler> safe_handler(handler);

        Status status = Status::OK();
        if (error_code != ErrorCodes::OK) {
            status = Status(error_code, from_capi(reason));
        }

        (*safe_handler)(std::move(status));
    }

    REALM_EXPORT void realm_websocket_delete_callback(SyncSocketProvider::FunctionHandler* handler) {
        delete handler;
    }

    REALM_EXPORT void realm_websocket_observer_connected_handler(WebSocketObserver* observer, realm_string_t protocol) {
        observer->websocket_connected_handler(from_capi(protocol));
    }

    REALM_EXPORT void realm_websocket_observer_error_handler(WebSocketObserver* observer) {
        observer->websocket_error_handler();
    }

    REALM_EXPORT void realm_websocket_observer_binary_message_received(WebSocketObserver* observer, realm_binary_t data) {
        observer->websocket_binary_message_received({reinterpret_cast<const char*>(data.data), data.size});
    }

    REALM_EXPORT void realm_websocket_observer_closed_handler(WebSocketObserver* observer, bool was_clean, websocket::WebSocketError error_code, realm_string_t reason) {
        observer->websocket_closed_handler(was_clean, error_code, capi_to_std(reason));
    }
}

} // namespace realm::binding
