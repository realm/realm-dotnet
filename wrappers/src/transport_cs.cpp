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

namespace realm {
namespace binding {
    GenericNetworkTransport::NetworkTransportFactory s_transport_factory;
}
}

struct HttpClientRequest {
    HttpMethod method;

    const char* url;
    size_t url_len;

    uint64_t timeout_ms;

    std::pair<char*, char*>* headers;
    int headers_len;

    const char* body;
    size_t body_len;
};

using ExecuteRequest = void(HttpClientRequest request, void* callback);
using ResponseFunction = std::function<void(const Response)>;

struct HttpClientResponse {
    int http_status_code;
    int custom_status_code;

    uint16_t* body_buf;
    size_t body_len;
};

struct HttpClientTransport : public GenericNetworkTransport {
public:
    HttpClientTransport(ExecuteRequest* execute)
        : m_execute(execute)
    { }

    void send_request_to_server(const Request request, ResponseFunction completionBlock) override {
        std::vector<std::pair<char*, char*>> headers;
        for (auto& kvp : request.headers) {
            headers.push_back(std::make_pair(const_cast<char*>(kvp.first.c_str()), const_cast<char*>(kvp.second.c_str())));
        }

        HttpClientRequest client_request = {
            request.method,
            request.url.c_str(),
            request.url.length(),
            request.timeout_ms,
            headers.data(),
            (int)headers.size(),
            request.body.c_str(),
            request.body.length()
        };

        m_execute(std::move(client_request), new ResponseFunction(completionBlock));
    }
private:
    ExecuteRequest* m_execute;
};

extern "C" {
    REALM_EXPORT void realm_http_transport_install_callbacks(ExecuteRequest* execute)
    {
        s_transport_factory = [=]() -> std::unique_ptr<HttpClientTransport> {
            return std::make_unique<HttpClientTransport>(execute);
        };
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

        auto& func = *reinterpret_cast<std::function<void(const Response)>*>(function_ptr);
        func(std::move(response));
        delete& func;
    }
}
