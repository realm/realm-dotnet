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

#include <realm.hpp>
#include <realm/util/uri.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include "sync/sync_manager.hpp"
#include "sync/sync_session.hpp"

using namespace realm;
using namespace realm::binding;

using SharedSyncSession = std::shared_ptr<SyncSession>;

extern "C" {
REALM_EXPORT void realm_syncsession_refresh_access_token(SharedSyncSession& session, const uint16_t* token_buf, size_t token_len, const uint16_t* server_path_buf, size_t server_path_len, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        Utf16StringAccessor token(token_buf, token_len);
        Utf16StringAccessor server_path(server_path_buf, server_path_len);

        realm::util::Uri server_url(session->config().realm_url);
        server_url.set_path(server_path);

        session->refresh_access_token(token, server_url.recompose());
    });
}

REALM_EXPORT void realm_syncsession_destroy(SharedSyncSession* session)
{
    delete session;
}
}

