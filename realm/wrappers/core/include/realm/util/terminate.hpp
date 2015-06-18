/*************************************************************************
 *
 * REALM CONFIDENTIAL
 * __________________
 *
 *  [2011] - [2012] Realm Inc
 *  All Rights Reserved.
 *
 * NOTICE:  All information contained herein is, and remains
 * the property of Realm Incorporated and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to Realm Incorporated
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from Realm Incorporated.
 *
 **************************************************************************/
#ifndef REALM_UTIL_TERMINATE_HPP
#define REALM_UTIL_TERMINATE_HPP

#include <sstream>
#include <cstdlib>
#include <string>
#include <stdint.h>
#include <realm/util/features.h>

#define REALM_TERMINATE(msg) realm::util::terminate((msg), __FILE__, __LINE__)

namespace realm {
namespace util {
REALM_NORETURN void terminate_internal(std::stringstream&) REALM_NOEXCEPT;

REALM_NORETURN inline void terminate(const char* message, const char* file, long line) REALM_NOEXCEPT {
    std::stringstream ss;
    ss << file << ":" << line << ": " << message << "\n";
    terminate_internal(ss);
}

template <typename T1, typename T2>
REALM_NORETURN void terminate(const char* message, const char* file, long line, T1 info1, T2 info2) REALM_NOEXCEPT {
    std::stringstream ss;
    ss << file << ":" << line << ": " << message << " [" << info1 << ", " << info2 << "]\n";
    terminate_internal(ss);
}

} // namespace util
} // namespace realm

#endif // REALM_UTIL_TERMINATE_HPP
