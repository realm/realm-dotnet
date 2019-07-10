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

#include <functional>
#include "util/generic/event_loop_signal.hpp"
#include "realm_export_decls.hpp"

using namespace realm::util;

using GetContextT = void*();
using PostOnContextT = void(void* context, void(*handler)(void* user_data), void* user_data);
using ReleaseContextT = void(void* context);

struct SynchronizationContextEventLoop : public GenericEventLoop {
public:
    SynchronizationContextEventLoop(void* context, PostOnContextT* post, ReleaseContextT* release)
    : m_context(context)
    , m_post(post)
    , m_release(release)
    { }

    void post(std::function<void()> action) override
    {
        m_post(m_context, &handle, new std::function<void()>(std::move(action)));
    }

    ~SynchronizationContextEventLoop()
    {
        m_release(m_context);
    }
private:
    static void handle(void* user_data)
    {
        auto& func = *reinterpret_cast<std::function<void()>*>(user_data);
        func();
        delete &func;
    }

    void* m_context;

    PostOnContextT* m_post;
    ReleaseContextT* m_release;
};

extern "C" {

REALM_EXPORT void realm_install_eventloop_callbacks(GetContextT* get, PostOnContextT* post, ReleaseContextT* release)
{
    GenericEventLoop::set_event_loop_factory([=] {
        auto* eventloop = new SynchronizationContextEventLoop(get(), post, release);
        return std::unique_ptr<GenericEventLoop>(eventloop);
    });
}

}
