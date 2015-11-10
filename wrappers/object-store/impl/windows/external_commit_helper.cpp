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

#include "external_commit_helper.hpp"

#include "../shared_realm.hpp"

using namespace realm;

// Fake external commit helper for windows. Doesn't do anything yet.
realm::_impl::ExternalCommitHelper::ExternalCommitHelper(Realm* realm)
{
}

realm::_impl::ExternalCommitHelper::~ExternalCommitHelper()
{
}

void realm::_impl::ExternalCommitHelper::add_realm(realm::Realm* realm)
{
}

void realm::_impl::ExternalCommitHelper::remove_realm(realm::Realm* realm)
{
}

void realm::_impl::ExternalCommitHelper::notify_others()
{
}

