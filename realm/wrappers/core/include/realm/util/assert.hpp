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
#ifndef REALM_UTIL_ASSERT_HPP
#define REALM_UTIL_ASSERT_HPP

#include <realm/util/features.h>
#include <realm/util/terminate.hpp>
#include <realm/version.hpp>

#if defined(REALM_ENABLE_ASSERTIONS) || defined(REALM_DEBUG)
#  define REALM_ASSERTIONS_ENABLED 1
#endif

#define REALM_ASSERT_RELEASE(condition) \
    ((condition) ? static_cast<void>(0) : \
    realm::util::terminate(REALM_VER_CHUNK " Assertion failed: " #condition, __FILE__, __LINE__))

#if REALM_ASSERTIONS_ENABLED
#  define REALM_ASSERT(condition) REALM_ASSERT_RELEASE(condition)
#else
#  define REALM_ASSERT(condition) static_cast<void>(0)
#endif

#ifdef REALM_DEBUG
#  define REALM_ASSERT_DEBUG(condition) REALM_ASSERT_RELEASE(condition)
#else
#  define REALM_ASSERT_DEBUG(condition) static_cast<void>(0)
#endif

// Becase the assert is used in noexcept methods, it's a bad idea to allocate buffer space for the message
// so therefore we must pass it to terminate which will 'cerr' it for us without needing any buffer
#if defined(REALM_ENABLE_ASSERTIONS) || defined(REALM_DEBUG)
#  define REALM_ASSERT_3(left, condition, right) \
    ((left condition right) ? static_cast<void>(0) : \
        realm::util::terminate(REALM_VER_CHUNK " Assertion failed: " #left " " #condition " " #right, \
                                 __FILE__, __LINE__, left, right))
#else
#  define REALM_ASSERT_3(left, condition, right) static_cast<void>(0)
#endif

#define REALM_UNREACHABLE() \
    realm::util::terminate(REALM_VER_CHUNK " Unreachable code", __FILE__, __LINE__)


#ifdef REALM_HAVE_CXX11_STATIC_ASSERT
#  define REALM_STATIC_ASSERT(condition, message) static_assert(condition, message)
#else
#  define REALM_STATIC_ASSERT(condition, message) typedef \
    realm::util::static_assert_dummy<sizeof(realm::util:: \
        REALM_STATIC_ASSERTION_FAILURE<bool(condition)>)> \
    REALM_JOIN(_realm_static_assert_, __LINE__) REALM_UNUSED
#  define REALM_JOIN(x,y) REALM_JOIN2(x,y)
#  define REALM_JOIN2(x,y) x ## y
namespace realm {
namespace util {
    template<bool> struct REALM_STATIC_ASSERTION_FAILURE;
    template<> struct REALM_STATIC_ASSERTION_FAILURE<true> {};
    template<int> struct static_assert_dummy {};
}
}
#endif


#endif // REALM_UTIL_ASSERT_HPP
