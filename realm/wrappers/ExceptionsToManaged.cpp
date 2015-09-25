#include "ExceptionsToManaged.h"


#ifdef WIN32
#define REALM_CORE_WRAPPER_API __declspec( dllexport )
#else
#define REALM_CORE_WRAPPER_API
#endif

using namespace realm;

using ManagedExceptionThrowerT = void(*)(size_t exceptionCode, void* utf8Str, size_t strLen);

// CALLBACK TO THROW IN MANAGED SPACE
static ManagedExceptionThrowerT ManagedExceptionThrower = nullptr;


void realm::ThrowManaged(const std::exception& exc, RealmExceptionCodes exceptionCode, const std::string& message)
{
    if (ManagedExceptionThrower) {
        ManagedExceptionThrower((size_t)exceptionCode, (void*)message.data(), message.size());
    }
}


void realm::ThrowManaged()
{
    if (ManagedExceptionThrower) {
        ManagedExceptionThrower((size_t)RealmExceptionCodes::Exception_FatalError, 0, 0);
    }
}



extern "C" {
    
    REALM_CORE_WRAPPER_API void set_exception_thrower(ManagedExceptionThrowerT userThrower)
    {
        ManagedExceptionThrower = userThrower;
    }

}