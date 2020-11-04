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

using namespace realm::util;

using GetContextT = void*();
using IsOnContextT = bool(void* context, void* target_context);
using PostOnContextT = void(void* context, void* user_data);
using ReleaseContextT = void(void* context);

struct SynchronizationContextScheduler : public Scheduler {
public:
    SynchronizationContextScheduler(void* context, PostOnContextT* post, ReleaseContextT* release, IsOnContextT* is_on_context)
    : m_context(context)
    , m_post(post)
    , m_release(release)
    , m_is_on_context(is_on_context)
    { }

    bool can_deliver_notifications() const noexcept override { return true; }

    bool is_same_as(const Scheduler* other) const noexcept override
    {
        auto o = dynamic_cast<const SynchronizationContextScheduler*>(other);
        if (o == nullptr || m_is_on_context != o->m_is_on_context) {
            return false;
        }

        return m_is_on_context(m_context, o->m_context);
    }

    bool is_on_thread() const noexcept override
    {
        // comparing against the null context means comparing against the current context
        return m_is_on_context(m_context, nullptr);
    }

    void set_notify_callback(std::function<void()> callback) override
    {
        m_callback = std::move(callback);
    }

    void notify() override
    {
        m_post(m_context, new std::function<void()>(m_callback));
    }

    ~SynchronizationContextScheduler()
    {
        m_release(m_context);
    }
private:
    void* m_context;

    PostOnContextT* m_post;
    ReleaseContextT* m_release;
    IsOnContextT* m_is_on_context;

    std::function<void()> m_callback;
};

struct ThreadScheduler : public Scheduler {
public:
    ThreadScheduler() = default;

    bool is_on_thread() const noexcept override { return m_id == std::this_thread::get_id(); }
    bool is_same_as(const Scheduler* other) const noexcept override
    {
        auto o = dynamic_cast<const ThreadScheduler*>(other);
        return (o && (o->m_id == m_id));
    }
    bool can_deliver_notifications() const noexcept override { return false; }

    void set_notify_callback(std::function<void()>) override { }
    void notify() override { }

private:
    std::thread::id m_id = std::this_thread::get_id();
};

extern "C" {

REALM_EXPORT void realm_install_scheduler_callbacks(GetContextT* get, PostOnContextT* post, ReleaseContextT* release, IsOnContextT* is_on_context)
{
    Scheduler::set_default_factory([=]() -> std::unique_ptr<Scheduler> {
        void* context = get();
        if (context) {
            return std::make_unique<SynchronizationContextScheduler>(context, post, release, is_on_context);
        }

        return std::make_unique<ThreadScheduler>();
    });
}

REALM_EXPORT void realm_scheduler_invoke_function(void* function_ptr)
{
    auto& func = *reinterpret_cast<std::function<void()>*>(function_ptr);
    func();
    delete& func;
}

}
