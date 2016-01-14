/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
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
