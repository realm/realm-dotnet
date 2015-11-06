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

#ifndef REALM_EXTERNAL_COMMIT_HELPER_HPP
#define REALM_EXTERNAL_COMMIT_HELPER_HPP

#include <mutex>
#include <vector>

namespace realm {
class Realm;

namespace _impl {

    class ExternalCommitHelper {
    public:
        ExternalCommitHelper(Realm* realm);
        ~ExternalCommitHelper();

        void notify_others();
        void add_realm(Realm* realm);
        void remove_realm(Realm* realm);
    };

} // namespace _impl
} // namespace realm

#endif /* REALM_EXTERNAL_COMMIT_HELPER_HPP */
