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

#include <functional>
#include <realm/object-store/util/scheduler.hpp>
#include "realm_export_decls.hpp"
#include <thread>
#include "marshalling.hpp"

namespace realm::binding {

using GetContextT = void*();
using IsOnContextT = bool(void* context, void* target_context);
using PostOnContextT = void(void* context, void* user_data);
using ReleaseContextT = void(void* context);

std::function<GetContextT> s_get_context;
std::function<IsOnContextT> s_is_on_context;
std::function<PostOnContextT> s_post_on_context;
std::function<ReleaseContextT> s_release_context;

struct SynchronizationContextScheduler : public util::Scheduler {
public:
    SynchronizationContextScheduler(void* context)
    : m_context(context)
    { }

    bool can_invoke() const noexcept override { return true; }

    bool is_same_as(const util::Scheduler* other) const noexcept override
    {
        auto o = dynamic_cast<const SynchronizationContextScheduler*>(other);
        return o != nullptr && s_is_on_context(m_context, o->m_context);
    }

    bool is_on_thread() const noexcept override
    {
        // comparing against the null context means comparing against the current context
        return s_is_on_context(m_context, nullptr);
    }

    void invoke(util::UniqueFunction<void()>&& func) override
    {
        s_post_on_context(m_context, new util::UniqueFunction<void()>(std::move(func)));
    }

    ~SynchronizationContextScheduler() override
    {
        s_release_context(m_context);
    }
private:
    void* m_context;
};

extern "C" {

REALM_EXPORT void realm_install_scheduler_callbacks(GetContextT* get, PostOnContextT* post, ReleaseContextT* release, IsOnContextT* is_on)
{
    s_get_context = wrap_managed_callback(get);
    s_post_on_context = wrap_managed_callback(post);
    s_release_context = wrap_managed_callback(release);
    s_is_on_context = wrap_managed_callback(is_on);

    util::Scheduler::set_default_factory([]() -> std::shared_ptr<util::Scheduler> {
        void* context = s_get_context();
        if (context) {
            return std::make_shared<SynchronizationContextScheduler>(context);
        }

        return util::Scheduler::make_generic();
    });

    realm::binding::s_can_call_managed = true;
}

REALM_EXPORT void realm_scheduler_invoke_function(void* function_ptr, bool execute_func)
{
    auto& func = *reinterpret_cast<util::UniqueFunction<void()>*>(function_ptr);

    if (execute_func) {
        func();
    }

    delete& func;
}

} // extern "C"
} // namespace realm::binding
