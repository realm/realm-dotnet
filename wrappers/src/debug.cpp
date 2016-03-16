/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

#include "debug.hpp"
#include "realm_export_decls.hpp"

namespace realm {

#if defined(DEBUG) || !defined(NDEBUG)

using DebugLoggerT = void(*)(void* utf8Str, size_t strLen);
static DebugLoggerT debug_log_function = nullptr;

void debug_log(const std::string message)
{
  debug_log_function((void*)message.data(), message.size());
}

#endif

}

extern "C" {

#if defined(DEBUG) || !defined(NDEBUG)

REALM_EXPORT void set_debug_logger(realm::DebugLoggerT debug_logger)
{
  realm::debug_log_function = debug_logger;
}

#endif

}
