////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

#include "transport_cs.hpp"
#include "marshalling.hpp"

#include <functional>
#include <realm/object-store/util/scheduler.hpp>
#include "realm_export_decls.hpp"
#include <thread>
#include <realm/object-store/sync/generic_network_transport.hpp>

namespace realm::binding {

struct HttpClientRequest {
    HttpMethod method;

    realm_string_t url;

    uint64_t timeout_ms;

    MarshaledVector<std::pair<realm_string_t, realm_string_t>> headers;

    realm_string_t body;

    void* managed_http_client;
};

using ExecuteRequestT = void(HttpClientRequest request, void* callback);
using ResponseFunction = util::UniqueFunction<void(const Response&)>;

std::function<ExecuteRequestT> s_execute_request;

struct HttpClientResponse {
    int http_status_code;
    int custom_status_code;

    MarshaledVector<std::pair<realm_string_t, realm_string_t>> headers;

    realm_string_t body;
};

HttpClientTransport::HttpClientTransport(GCHandleHolder managed_http_client) : m_managed_http_client(std::move(managed_http_client)) {}

void HttpClientTransport::send_request_to_server(const Request& request, ResponseFunction&& completionBlock) {
    std::vector<std::pair<realm_string_t, realm_string_t>> headers;
    for (auto& kvp : request.headers) {
        headers.push_back(std::make_pair(to_capi(kvp.first), to_capi(kvp.second)));
    }

    HttpClientRequest client_request = {
        request.method,
        to_capi(request.url),
        request.timeout_ms,
        headers,
        to_capi(request.body),
        m_managed_http_client.handle(),
    };

    s_execute_request(std::move(client_request), new ResponseFunction(std::move(completionBlock)));
}

extern "C" {
    REALM_EXPORT void realm_http_transport_install_callbacks(ExecuteRequestT* execute)
    {
        s_execute_request = wrap_managed_callback(execute);

        realm::binding::s_can_call_managed = true;
    }

    REALM_EXPORT void realm_http_transport_respond(HttpClientResponse client_response, void* function_ptr)
    {
        std::unique_ptr<ResponseFunction> func(reinterpret_cast<ResponseFunction*>(function_ptr));

        std::map<std::string, std::string> headers_map;
        for (auto i = 0; i < client_response.headers.count; i++) {
            auto& header = client_response.headers.items[i];
            headers_map.emplace(from_capi(header.first), from_capi(header.second));
        }

        Response response = {
            client_response.http_status_code,
            client_response.custom_status_code,
            std::move(headers_map),
            from_capi(client_response.body)
        };

        (*func)(std::move(response));
    }
} // extern "C"
} // namespace realm::binding
