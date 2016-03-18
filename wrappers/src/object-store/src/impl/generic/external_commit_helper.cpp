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

#include "impl/external_commit_helper.hpp"

#include "impl/realm_coordinator.hpp"

#include <realm/commit_log.hpp>
#include <realm/replication.hpp>

using namespace realm;
using namespace realm::_impl;

ExternalCommitHelper::ExternalCommitHelper(RealmCoordinator& parent)
{
}

ExternalCommitHelper::~ExternalCommitHelper()
{
}
