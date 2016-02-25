#include "debug.hpp"
#include "realm_export_decls.hpp"

namespace realm {

using DebugLogT = void(*)(void* utf8Str, size_t strLen);
static DebugLogT debug_log_function = nullptr;

void debug_log(std::string message)
{
  debug_log_function((void*)message.data(), message.size());
}

}

extern "C" {

REALM_EXPORT void bind_debug_log(realm::DebugLogT debug_log)
{
  realm::debug_log_function = debug_log;
}

}

