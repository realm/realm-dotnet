/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
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

