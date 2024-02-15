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

#include <realm.hpp>
#include "error_handling.hpp"
#include "marshalling.hpp"
#include "realm_export_decls.hpp"
#include <realm/object-store/sync/async_open_task.hpp>
#include "sync_session_cs.hpp"

using namespace realm;
using namespace realm::binding;
using SharedAsyncOpenTask = std::shared_ptr<AsyncOpenTask>;

extern "C" {
REALM_EXPORT void realm_asyncopentask_destroy(SharedAsyncOpenTask* task)
{
    delete task;
}

REALM_EXPORT void realm_asyncopentask_cancel(SharedAsyncOpenTask& task, NativeException::Marshallable& ex)
{
    handle_errors(ex, [&] {
        task->cancel();
    });
}

REALM_EXPORT uint64_t realm_asyncopentask_register_progress_notifier(const SharedAsyncOpenTask& task, void* managed_state, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        return task->register_download_progress_notifier([managed_state](uint64_t transferred, uint64_t transferable, double progress_estimate) {
            s_progress_callback(managed_state, transferred, transferable, progress_estimate);
        });
    });
}

REALM_EXPORT void realm_asyncopentask_unregister_progress_notifier(const SharedAsyncOpenTask& task, uint64_t token, NativeException::Marshallable& ex)
{
    return handle_errors(ex, [&] {
        task->unregister_download_progress_notifier(token);
    });
}

}
