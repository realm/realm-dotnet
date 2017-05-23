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

#include <utility>
#include "util/generic/event_loop_signal.hpp"
#include "realm_export_decls.hpp"

using namespace realm::util;

extern "C" {

REALM_EXPORT void realm_install_eventloop_callbacks(decltype(s_get_eventloop) get, decltype(s_post_on_eventloop) post, decltype(s_release_eventloop) release)
{
    s_get_eventloop = get;
    s_post_on_eventloop = post;
    s_release_eventloop = release;
}

}
