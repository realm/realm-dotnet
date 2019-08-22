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
#include "sync/async_open_task.hpp"

using namespace realm;
using namespace realm::binding;
using SharedAsyncOpenTask = std::shared_ptr<AsyncOpenTask>;

namespace realm {
namespace binding {
	void (*s_async_open_callback)(size_t, uint64_t transferred_bytes, uint64_t transferrable_bytes);
}
}


extern "C" {
REALM_EXPORT void realm_install_async_open_task_callbacks(decltype(s_async_open_callback) async_open_callback)
{
	s_async_open_callback = async_open_callback;
}

REALM_EXPORT void realm_async_open_task_destroy(SharedAsyncOpenTask* task, NativeException::Marshallable& ex)
{
	handle_errors(ex, [&] {
		delete task;
	});
}

REALM_EXPORT void realm_async_open_task_cancel(SharedAsyncOpenTask& task, NativeException::Marshallable& ex)
{
	handle_errors(ex, [&] {
		task->cancel();
	});
}

REALM_EXPORT uint64_t realm_async_open_task_register_progress_notifier(const SharedAsyncOpenTask& task, size_t managed_state, NativeException::Marshallable& ex)
{
	return handle_errors(ex, [&] {
		return task->register_download_progress_notifier([managed_state](uint64_t transferred, uint64_t transferable) {
				s_async_open_callback(managed_state, transferred, transferable);
			});
	});
}
}