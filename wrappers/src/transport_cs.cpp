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

using namespace realm::util;
using namespace realm::app;
using namespace realm::binding;

struct HttpClientRequest {
    HttpMethod method;

    realm_value_t url;

    uint64_t timeout_ms;

    std::pair<char*, char*>* headers;
    size_t headers_len;

    realm_value_t body;

    void* managed_http_client;
};

using ExecuteRequestT = void(HttpClientRequest request, void* callback);
using ResponseFunction = util::UniqueFunction<void(const Response&)>;

namespace realm {
namespace binding {
std::function<ExecuteRequestT> s_execute_request;

struct HttpClientResponse {
    int http_status_code;
    int custom_status_code;

    uint16_t* body_buf;
    size_t body_len;
};

HttpClientTransport::HttpClientTransport(GCHandleHolder managed_http_client) : m_managed_http_client(std::move(managed_http_client)) {}

void HttpClientTransport::send_request_to_server(const Request& request, ResponseFunction&& completionBlock) {
    std::vector<std::pair<char*, char*>> headers;
    for (auto& kvp : request.headers) {
        headers.push_back(std::make_pair(const_cast<char*>(kvp.first.c_str()), const_cast<char*>(kvp.second.c_str())));
    }

    HttpClientRequest client_request = {
        request.method,
        to_capi_value(request.url),
        request.timeout_ms,
        headers.data(),
        headers.size(),
        to_capi_value(request.body),
        m_managed_http_client.handle(),
    };

    s_execute_request(std::move(client_request), new ResponseFunction(std::move(completionBlock)));
}

}
}

extern "C" {
    REALM_EXPORT void realm_http_transport_install_callbacks(ExecuteRequestT* execute)
    {
        s_execute_request = wrap_managed_callback(execute);

        realm::binding::s_can_call_managed = true;
    }

    REALM_EXPORT void realm_http_transport_respond(HttpClientResponse client_response, std::pair<char*, char*>* headers, int headers_len, void* function_ptr)
    {
        std::map<std::string, std::string> headers_map;
        for (auto i = 0; i < headers_len; i++) {
            auto header = headers[i];
            headers_map.emplace(header.first, header.second);
        }

        Response response = {
            client_response.http_status_code,
            client_response.custom_status_code,
            std::move(headers_map),
            Utf16StringAccessor(client_response.body_buf, client_response.body_len)
        };

        auto& func = *reinterpret_cast<ResponseFunction*>(function_ptr);
        func(std::move(response));
        delete& func;
    }
}
