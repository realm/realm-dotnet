////////////////////////////////////////////////////////////////////////////
//
// Copyright 2015 Realm Inc.
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

#include "impl/weak_realm_notifier.hpp"
#include "shared_realm.hpp"

#include <fcntl.h> 
#include <unistd.h>
#include <android/log.h>
#include <android/looper.h>

#define LOGE(...) ((void)__android_log_print(ANDROID_LOG_ERROR, "realm-object-store", __VA_ARGS__))

namespace realm {
namespace _impl {

WeakRealmNotifier::WeakRealmNotifier(const std::shared_ptr<Realm>& realm, bool cache)
: WeakRealmNotifierBase(realm, cache)
{
    ALooper* looper = ALooper_forThread();
    if (!looper) {
        return;
    }

    int message_pipe[2];
    if (pipe2(message_pipe, O_CLOEXEC | O_NONBLOCK)) {
        LOGE("could not create WeakRealmNotifier ALooper message pipe: %s", strerror(errno));
        return;
    }

    if (ALooper_addFd(looper, message_pipe[0], 3 /* LOOPER_ID_USER */, ALOOPER_EVENT_INPUT, &looper_callback, nullptr) != 1) {
        LOGE("Error adding WeakRealmNotifier callback to looper.");
        close(message_pipe[0]);
        close(message_pipe[1]);
        
        return;
    }

    m_message_pipe.read = message_pipe[0];
    m_message_pipe.write = message_pipe[1];
    m_thread_has_looper = true;
}

WeakRealmNotifier::WeakRealmNotifier(WeakRealmNotifier&& rgt)
: WeakRealmNotifierBase(std::move(rgt))
, m_message_pipe(std::move(rgt.m_message_pipe))
, m_thread_has_looper(rgt.m_thread_has_looper)
{
    rgt.m_thread_has_looper = false;
}

WeakRealmNotifier& WeakRealmNotifier::operator=(WeakRealmNotifier&& rgt)
{
    WeakRealmNotifierBase::operator=(std::move(rgt));
    m_message_pipe = std::move(rgt.m_message_pipe);
    m_thread_has_looper = rgt.m_thread_has_looper;
    rgt.m_thread_has_looper = false;

    return *this;
}

WeakRealmNotifier::~WeakRealmNotifier()
{
    if (m_thread_has_looper) {
        ALooper_removeFd(ALooper_forThread(), m_message_pipe.read);
        close(m_message_pipe.read);
        close(m_message_pipe.write);
    }
}

void WeakRealmNotifier::notify()
{
    if (m_thread_has_looper && !expired()) {
        
        // we need to pass the weak Realm pointer to the other thread.
        // to do so we allocate a weak pointer on the heap and send its addess over a pipe.
        // the other end of the pipe is read by the realm thread. when it's done with the pointer, it deletes it.
        auto realmPtr = new std::weak_ptr<Realm>(realm());
        if (write(m_message_pipe.write, realmPtr, sizeof(realmPtr)) != sizeof(realmPtr)) {
            LOGE("Buffer overrun when writing to WeakRealmNotifier's ALooper message pipe.");
        }
    }
}

int WeakRealmNotifier::looper_callback(int fd, int events, void* data)
{
    // this is a pointer to a heap-allocated weak Realm pointer created by the notifiying thread.
    // the actual address to the pointer is communicated over a pipe.
    // we have to delete it so as to not leak, using the same memory allocation facilities it was allocated with.
    std::weak_ptr<Realm>* realmPtr = nullptr;
    while (read(fd, &realmPtr, sizeof(realmPtr)) == sizeof(realmPtr)) {
        if (auto realm = realmPtr->lock()) {
            if (!realm->is_closed()) {
                realm->notify();
            }
        }

        delete realmPtr;
    }

    // return 1 to continue receiving events
    return 1;
}

}
}
