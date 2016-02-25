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

#include "cached_realm.hpp"

#include <atomic>

namespace realm
{
namespace _impl
{

CachedRealm::CachedRealm(const std::shared_ptr<Realm>& realm, bool cache)
: CachedRealmBase(realm, cache)
{
}

CachedRealm::~CachedRealm()
{
}

void CachedRealm::enable_auto_refresh()
{
    m_handler = create_handler_for_current_thread();
}

void CachedRealm::notify()
{
    notify_handler(m_handler);
}


create_handler_function create_handler_for_current_thread = nullptr;
notify_handler_function notify_handler = nullptr;

}
}
