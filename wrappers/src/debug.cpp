/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

#include "debug.hpp"
#include "realm_export_decls.hpp"

namespace realm {

using DebugLoggerT = void(*)(void* utf8Str, size_t strLen);
static DebugLoggerT debug_log_function = nullptr;

void debug_log(const std::string message)
{
  // second check against -1 based on suspicions from stack traces of this as sentinel value
  if (debug_log_function != nullptr && debug_log_function != reinterpret_cast<DebugLoggerT>(-1))
    debug_log_function((void*)message.data(), message.size());
}

}

extern "C" {

REALM_EXPORT void set_debug_logger(realm::DebugLoggerT debug_logger)
{
  realm::debug_log_function = debug_logger;
}

}
